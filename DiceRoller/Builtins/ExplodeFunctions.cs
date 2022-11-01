using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dice.AST;

namespace Dice.Builtins
{
    /// <summary>
    /// Built-in functions which add new dice the roll when certain conditions are met.
    /// </summary>
    public static class ExplodeFunctions
    {
        [Flags]
        private enum ExplodeBehavior
        {
            /// <summary>
            /// No special behavior.
            /// </summary>
            None = 0,

            /// <summary>
            /// Accumulate all rolls into a single DieResult.
            /// </summary>
            Compound = 1,

            /// <summary>
            /// Subtract one from each subsequent die roll.
            /// </summary>
            Penetrate = 2,

            /// <summary>
            /// Subtract subsequent dice instead of adding them.
            /// </summary>
            Implode = 4
        }

        /// <summary>
        /// Validate whether or not the combination of explode functions on the roll are valid.
        /// </summary>
        /// <param name="sender">Unused.</param>
        /// <param name="e">Details about the roll.</param>
        [SuppressMessage("Security", "CA2109:Review visible event handlers",
            Justification = "Public to allow library consumers to remove this validation event from BuiltinFunctionRegistry")]
        public static void ValidateExplode(object? sender, ValidateEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (e.Timing != FunctionTiming.Explode)
            {
                return;
            }

            if (e.Contexts.Select(c => c.Name).Distinct().Count() > 1)
            {
                throw new DiceException(DiceErrorCode.MixedExplodeType);
            }

            if (e.Contexts.Any(c => c.Arguments.Count == 0) && e.Contexts.Any(c => c.Arguments.Count > 0))
            {
                throw new DiceException(DiceErrorCode.MixedExplodeComp);
            }
        }

        /// <summary>
        /// When the comparison succeeds, a new die is rolled and added to the roll result.
        /// This continues until the comparison fails.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("explode", "!e",
            Behavior = FunctionBehavior.CombineArguments,
            Scope = FunctionScope.Basic,
            Timing = FunctionTiming.Explode,
            ArgumentPattern = "C*")]
        public static void Explode(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var comparisons = context.Arguments.OfType<ComparisonNode>().ToList();
            DoExplode(context, comparisons, ExplodeBehavior.None);
        }

        /// <summary>
        /// When the comparison succeeds, a new die is rolled and added to the roll result.
        /// This happens at most once.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("explodeOnce", "!eo",
            Behavior = FunctionBehavior.CombineArguments,
            Scope = FunctionScope.Basic,
            Timing = FunctionTiming.Explode,
            ArgumentPattern = "C*")]
        public static void ExplodeOnce(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var comparisons = context.Arguments.OfType<ComparisonNode>().ToList();
            DoExplode(context, comparisons, ExplodeBehavior.None, max: 1);
        }

        /// <summary>
        /// When the comparison succeeds, a new die is rolled and merged into the existing die.
        /// This continues until the comparison fails.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("compound", "!c",
            Behavior = FunctionBehavior.CombineArguments,
            Scope = FunctionScope.Basic,
            Timing = FunctionTiming.Explode,
            ArgumentPattern = "C*")]
        public static void Compound(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var comparisons = context.Arguments.OfType<ComparisonNode>().ToList();
            DoExplode(context, comparisons, ExplodeBehavior.Compound);
        }

        /// <summary>
        /// When the comparison succeeds, a new die is rolled and added to the roll result;
        /// 1 is subtracted from each such die. This continues until the comparison fails.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("penetrate", "!p",
            Behavior = FunctionBehavior.CombineArguments,
            Scope = FunctionScope.Basic,
            Timing = FunctionTiming.Explode,
            ArgumentPattern = "C*")]
        public static void Penetrate(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var comparisons = context.Arguments.OfType<ComparisonNode>().ToList();
            DoExplode(context, comparisons, ExplodeBehavior.Penetrate);
        }

        /// <summary>
        /// When the comparison succeeds, a new die is rolled and merged into the existing die;
        /// 1 is subtracted from each such die. This continues until the comparison fails.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("compoundPenetrate",
            Behavior = FunctionBehavior.CombineArguments,
            Scope = FunctionScope.Basic,
            Timing = FunctionTiming.Explode,
            ArgumentPattern = "C*")]
        public static void CompoundPenetrate(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var comparisons = context.Arguments.OfType<ComparisonNode>().ToList();
            DoExplode(context, comparisons, ExplodeBehavior.Compound | ExplodeBehavior.Penetrate);
        }

        /// <summary>
        /// When the comparison succeeds, a new die is rolled and subtracted from the roll result.
        /// This continues until the comparison fails.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("implode", "!i",
            Behavior = FunctionBehavior.CombineArguments,
            Scope = FunctionScope.Basic,
            Timing = FunctionTiming.Explode,
            ArgumentPattern = "C*")]
        public static void Implode(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var comparisons = context.Arguments.OfType<ComparisonNode>().ToList();
            DoExplode(context, comparisons, ExplodeBehavior.Implode);
        }

        /// <summary>
        /// When the comparison succeeds, a new die is rolled and subtracted from the roll result.
        /// This happens at most once.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("implodeOnce", "!io",
            Behavior = FunctionBehavior.CombineArguments,
            Scope = FunctionScope.Basic,
            Timing = FunctionTiming.Explode,
            ArgumentPattern = "C*")]
        public static void ImplodeOnce(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var comparisons = context.Arguments.OfType<ComparisonNode>().ToList();
            DoExplode(context, comparisons, ExplodeBehavior.Implode, max: 1);
        }

        /// <summary>
        /// When the comparison succeeds, a new die is rolled and subtracted from the roll result.
        /// This continues until the comparison fails.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("compoundImplode",
            Behavior = FunctionBehavior.CombineArguments,
            Scope = FunctionScope.Basic,
            Timing = FunctionTiming.Explode,
            ArgumentPattern = "C*")]
        public static void CompoundImplode(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var comparisons = context.Arguments.OfType<ComparisonNode>().ToList();
            DoExplode(context, comparisons, ExplodeBehavior.Compound | ExplodeBehavior.Implode);
        }

        private static void DoExplode(FunctionContext context, List<ComparisonNode> comparisons, ExplodeBehavior behavior, int max = Int32.MaxValue)
        {
            bool compound = behavior.HasFlag(ExplodeBehavior.Compound);
            bool penetrate = behavior.HasFlag(ExplodeBehavior.Penetrate);
            bool implode = behavior.HasFlag(ExplodeBehavior.Implode);
            long rolls = 0;
            Func<DieResult, decimal, bool> shouldExplode;
            var values = new List<DieResult>();
            // this value is subtracted from the result of each subsequent roll
            decimal penetrateValueAdjustment = penetrate ? 1 : 0;
            // this value is multiplied by the final value of each subsequent roll
            decimal implodeAdjustment = implode ? -1 : 1;
            SpecialDie explodeOp = implode ? SpecialDie.Subtract : SpecialDie.Add;

            context.Value = 0;
            context.Values = values;
            context.ValueType = ResultType.Total;

            if (comparisons.Count > 0)
            {
                shouldExplode = (d, x) => comparisons.Any(c => c.Compare(d.Value + x));
            }
            else if (implode)
            {
                shouldExplode = (d, x) => d.Value + x == d.DieType switch
                {
                    DieType.Normal => 1,
                    DieType.Fudge => -d.NumSides,
                    _ => throw new InvalidOperationException("Unexpected die type when evaluating explode expression")
                };
            }
            else
            {
                shouldExplode = (d, x) => d.Value + x == d.NumSides;
            }

            foreach (var die in context.Expression!.Values)
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

                    context.Value += implodeAdjustment * result.Value;
                    if (compound)
                    {
                        accum.Value += implodeAdjustment * result.Value;
                    }
                    else
                    {
                        values.Add(new DieResult(explodeOp));
                        values.Add(result);
                    }
                } while (rolls < max && shouldExplode(result, penetrateValueAdjustment));

                if (compound)
                {
                    values.Add(accum);
                }
            }
        }
    }
}
