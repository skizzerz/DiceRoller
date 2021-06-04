using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Dice.AST;

namespace Dice.Builtins
{
    public static class CritFunctions
    {
        [DiceFunction("critical", "cs",
            ArgumentPattern = "C+",
            Behavior = FunctionBehavior.CombineArguments,
            Scope = FunctionScope.Roll,
            Timing = FunctionTiming.Crit)]
        public static void Critical(FunctionContext context)
        {
            MarkCrits(context ?? throw new ArgumentNullException(nameof(context)),
                critical: new ComparisonNode(context.Arguments.Cast<ComparisonNode>()));
        }

        [DiceFunction("fumble", "cf",
            ArgumentPattern = "C+",
            Behavior = FunctionBehavior.CombineArguments,
            Scope = FunctionScope.Roll,
            Timing = FunctionTiming.Crit)]
        public static void Fumble(FunctionContext context)
        {
            MarkCrits(context ?? throw new ArgumentNullException(nameof(context)),
                fumble: new ComparisonNode(context.Arguments.Cast<ComparisonNode>()));
        }

        private static void MarkCrits(FunctionContext context, ComparisonNode? critical = null, ComparisonNode? fumble = null)
        {
            var values = new List<DieResult>();
            DieFlags mask = 0;
            context.Value = context.Expression!.Value;
            context.ValueType = context.Expression.ValueType;

            if (critical != null)
            {
                mask |= DieFlags.Critical;
            }

            if (fumble != null)
            {
                mask |= DieFlags.Fumble;
            }

            foreach (var die in context.Expression.Values)
            {
                DieFlags flags = 0;

                if (die.DieType == DieType.Special || die.DieType == DieType.Group)
                {
                    // we don't skip over dropped dice here since we DO still want to
                    // mark them as criticals/fumbles as needed.
                    values.Add(die);
                    continue;
                }

                if (critical?.Compare(die.Value) == true)
                {
                    flags |= DieFlags.Critical;

                    // if tracking successes; a critical success is worth 2 successes
                    if (context.ValueType == ResultType.Successes)
                    {
                        // just in case the die wasn't already marked as a success
                        if ((die.Flags & DieFlags.Success) == 0)
                        {
                            flags |= DieFlags.Success;
                            ++context.Value;
                        }

                        ++context.Value;
                    }
                }

                if (fumble?.Compare(die.Value) == true)
                {
                    flags |= DieFlags.Fumble;

                    // if tracking failures; a critical failure is worth -2 successes
                    if (context.ValueType == ResultType.Successes)
                    {
                        // just in case the die wasn't already marked as a failure
                        if ((die.Flags & DieFlags.Failure) == 0)
                        {
                            flags |= DieFlags.Failure;
                            --context.Value;
                        }

                        --context.Value;
                    }
                }

                values.Add(new DieResult()
                {
                    DieType = die.DieType,
                    NumSides = die.NumSides,
                    Value = die.Value,
                    // strip any existing crit/fumble flag off and use ours,
                    // assuming a comparison was defined for it.
                    // (we may have an existing flag if the die rolled min or max value)
                    Flags = (die.Flags & ~mask) | flags
                });
            }

            context.Values = values;
        }
    }
}
