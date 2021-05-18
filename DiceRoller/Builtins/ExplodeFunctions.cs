using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Dice.AST;

namespace Dice.Builtins
{
    public static class ExplodeFunctions
    {
        [DiceFunctionPrecondition(FunctionTiming.Explode)]
        public static void ValidateExplode(IReadOnlyList<FunctionContext> contexts)
        {
            if (contexts.Select(c => c.Name).Distinct().Count() > 1)
            {
                throw new DiceException(DiceErrorCode.MixedExplodeType);
            }

            if (contexts.Any(c => c.Arguments.Count == 0) && contexts.Any(c => c.Arguments.Count > 0))
            {
                throw new DiceException(DiceErrorCode.MixedExplodeComp);
            }
        }

        [DiceFunction("explode", "!e", Scope = FunctionScope.Basic, Timing = FunctionTiming.Explode)]
        public static void Explode(FunctionContext context)
        {
            var comparisons = context.Arguments.OfType<ComparisonNode>().ToList();

            if (comparisons.Count < context.Arguments.Count)
            {
                throw new DiceException(DiceErrorCode.IncorrectArgType, context.Name);
            }

            DoExplode(context, comparisons, compound: false, penetrate: false);
        }

        [DiceFunction("compound", "!c", Scope = FunctionScope.Basic, Timing = FunctionTiming.Explode)]
        public static void Compound(FunctionContext context)
        {
            var comparisons = context.Arguments.OfType<ComparisonNode>().ToList();

            if (comparisons.Count < context.Arguments.Count)
            {
                throw new DiceException(DiceErrorCode.IncorrectArgType, context.Name);
            }

            DoExplode(context, comparisons, compound: true, penetrate: false);
        }

        [DiceFunction("penetrate", "!p", Scope = FunctionScope.Basic, Timing = FunctionTiming.Explode)]
        public static void Penetrate(FunctionContext context)
        {
            var comparisons = context.Arguments.OfType<ComparisonNode>().ToList();

            if (comparisons.Count < context.Arguments.Count)
            {
                throw new DiceException(DiceErrorCode.IncorrectArgType, context.Name);
            }

            DoExplode(context, comparisons, compound: false, penetrate: true);
        }

        [DiceFunction("compoundPenetrate", Scope = FunctionScope.Basic, Timing = FunctionTiming.Explode)]
        public static void CompoundPenetrate(FunctionContext context)
        {
            var comparisons = context.Arguments.OfType<ComparisonNode>().ToList();

            if (comparisons.Count < context.Arguments.Count)
            {
                throw new DiceException(DiceErrorCode.IncorrectArgType, context.Name);
            }

            DoExplode(context, comparisons, compound: true, penetrate: true);
        }

        private static void DoExplode(FunctionContext context, List<ComparisonNode> comparisons, bool compound, bool penetrate)
        {
            long rolls = 0;
            Func<DieResult, decimal, bool> shouldExplode;
            var values = new List<DieResult>();
            decimal penetrateValueAdjustment = penetrate ? 1 : 0;

            context.Value = 0;
            context.Values = values;
            context.ValueType = ResultType.Total;

            if (comparisons.Count > 0)
            {
                shouldExplode = (d, x) => comparisons.Any(c => c.Compare(d.Value + x));
            }
            else
            {
                shouldExplode = (d, x) => d.Value + x == d.NumSides;
            }


            foreach (var die in context.Expression.Values)
            {
                var accum = die;

                if (!die.IsLiveDie())
                {
                    // special die results can't explode as they aren't actually dice
                    // dropped dice are no longer part of the resultant expression so should not explode
                    values.Add(die);
                    continue;
                }

                var rt = die.DieType switch
                {
                    DieType.Normal => RollType.Normal,
                    DieType.Fudge => RollType.Fudge,
                    _ => throw new InvalidOperationException("Unsupported die type for explosion"),
                };

                context.Value += die.Value;
                if (!shouldExplode(die, 0))
                {
                    values.Add(die);
                    continue;
                }

                if (!compound)
                {
                    values.Add(die);
                }

                DieResult result;

                do
                {
                    rolls++;

                    if (rolls > context.Data.Config.MaxRerolls)
                    {
                        break;
                    }

                    var numSides = die.NumSides;
                    if (penetrate && comparisons.Count == 0)
                    {
                        // if penetrating dice are used, d100p penetrates to d20p,
                        // and d20p penetrates to d6p (however, the d20p from
                        // the d100p does not further drop to d6p).
                        // only do this if a custom comparison expression was not used.
                        if (numSides == 100)
                        {
                            numSides = 20;
                        }
                        else if (numSides == 20)
                        {
                            numSides = 6;
                        }
                    }

                    result = context.RollExtra(rt, numSides);
                    // if we're penetrating, subtract 1 from all subsequent rolls
                    result.Value -= penetrateValueAdjustment;

                    context.Value += result.Value;
                    if (compound)
                    {
                        accum.Value += result.Value;
                    }
                    else
                    {
                        values.Add(new DieResult(SpecialDie.Add));
                        values.Add(result);
                    }
                } while (shouldExplode(result, penetrateValueAdjustment));

                if (compound)
                {
                    values.Add(accum);
                }
            }
        }
    }
}
