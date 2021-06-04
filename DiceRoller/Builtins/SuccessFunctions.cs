using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dice.AST;

namespace Dice.Builtins
{
    /// <summary>
    /// Builtin functions used to transform a result into successes, for
    /// use in dice pool systems.
    /// </summary>
    public static class SuccessFunctions
    {
        /// <summary>
        /// Mark dice as successes if the comparison succeeds. Successes
        /// each add 1 to the final result.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("success", "",
            ArgumentPattern = "C+",
            Behavior = FunctionBehavior.CombineArguments,
            Scope = FunctionScope.Roll,
            Timing = FunctionTiming.Success)]
        public static void Success(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            CountSuccesses(context, success: new ComparisonNode(context.Arguments.Cast<ComparisonNode>()));
        }

        /// <summary>
        /// Mark dice as failures if the comparison succeeds. Failures
        /// each subtract 1 from the final result.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("failure",
            ArgumentPattern = "C+",
            Behavior = FunctionBehavior.CombineArguments,
            Scope = FunctionScope.Roll,
            Timing = FunctionTiming.Success)]
        public static void Failure(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            CountSuccesses(context, failure: new ComparisonNode(context.Arguments.Cast<ComparisonNode>()));
        }

        private static void CountSuccesses(FunctionContext context, ComparisonNode? success = null, ComparisonNode? failure = null)
        {
            // if we're chaining from a success-based roll, keep the number of successes previously as our starting point
            var successes = context.Expression!.ValueType == ResultType.Successes ? context.Expression.Value : 0;
            var values = new List<DieResult>();

            foreach (var die in context.Expression.Values)
            {
                if (die.DieType == DieType.Special)
                {
                    values.Add(die);
                    continue;
                }

                if (die.IsLiveDie() && success?.Compare(die.Value) == true)
                {
                    ++successes;
                    values.Add(die.Success());
                }
                else if (die.IsLiveDie() && failure?.Compare(die.Value) == true)
                {
                    --successes;
                    values.Add(die.Failure());
                }
                else
                {
                    // strip crit/fumble markings from the underlying roll, so that later critical() and fumble()
                    // calls can properly mark critical successes or fumbles
                    values.Add(new DieResult()
                    {
                        DieType = die.DieType,
                        NumSides = die.NumSides,
                        Value = die.Value,
                        Data = die.Data,
                        Flags = die.Flags & ~(DieFlags.Critical | DieFlags.Fumble)
                    });
                }
            }

            context.Value = successes;
            context.ValueType = ResultType.Successes;
            context.Values = values;
        }
    }
}
