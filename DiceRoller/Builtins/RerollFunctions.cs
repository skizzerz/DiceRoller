using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Dice.AST;

namespace Dice.Builtins
{
    /// <summary>
    /// Functions which reroll dice.
    /// </summary>
    public static class RerollFunctions
    {
        /// <summary>
        /// Validate that only one type of reroll is attached to this dice expression.
        /// </summary>
        /// <param name="sender">Unused.</param>
        /// <param name="e">Event args.</param>
        [SuppressMessage("Security", "CA2109:Review visible event handlers",
            Justification = "Public to allow library consumers to remove this validation event from BuiltinFunctionRegistry")]
        public static void ValidateReroll(object? sender, ValidateEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (e.Timing != FunctionTiming.Reroll)
            {
                return;
            }

            if (e.Contexts.Select(c => c.Name).Distinct().Count() > 1)
            {
                throw new DiceException(DiceErrorCode.MixedReroll);
            }
        }

        /// <summary>
        /// Reroll dice as long as they meet one of the comparisons.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("reroll", "rr", Behavior = FunctionBehavior.CombineArguments, Scope = FunctionScope.Roll, Timing = FunctionTiming.Reroll)]
        public static void Reroll(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var comparisons = context.Arguments.OfType<ComparisonNode>().ToList();

            if (context.Arguments.Count == 0)
            {
                throw new DiceException(DiceErrorCode.IncorrectArity, context.Name);
            }

            if (comparisons.Count < context.Arguments.Count)
            {
                throw new DiceException(DiceErrorCode.IncorrectArgType, context.Name);
            }

            DoReroll(context, new RerollData[] { new RerollData(context.Data.Config.MaxRerolls, new ComparisonNode(comparisons)) });
        }

        /// <summary>
        /// Reroll dice once if they meet any of the comparisons.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("rerollOnce", "ro", Behavior = FunctionBehavior.CombineArguments, Scope = FunctionScope.Roll, Timing = FunctionTiming.Reroll)]
        public static void RerollOnce(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var comparisons = context.Arguments.OfType<ComparisonNode>().ToList();

            if (context.Arguments.Count == 0)
            {
                throw new DiceException(DiceErrorCode.IncorrectArity, context.Name);
            }

            if (comparisons.Count < context.Arguments.Count)
            {
                throw new DiceException(DiceErrorCode.IncorrectArgType, context.Name);
            }

            DoReroll(context, new RerollData[] { new RerollData(1, new ComparisonNode(comparisons)) });
        }

        /// <summary>
        /// Reroll dice a user-specified number of times for each group of comparisons specified.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("rerollN", Behavior = FunctionBehavior.CombineArguments, Scope = FunctionScope.Roll, Timing = FunctionTiming.Reroll)]
        public static void RerollN(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var dataList = new List<RerollData>();

            if (context.Arguments.Count < 2)
            {
                throw new DiceException(DiceErrorCode.IncorrectArity, context.Name);
            }

            List<ComparisonNode>? comparisonList = null;
            int max = 0;
            foreach (var arg in context.Arguments)
            {
                if (arg is ComparisonNode cn)
                {
                    // leading with a comparison?
                    if (comparisonList == null)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, context.Name);
                    }

                    comparisonList.Add(cn);
                }
                else
                {
                    // two non-comparisons are adjacent?
                    if (comparisonList?.Count == 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, context.Name);
                    }

                    // finished a RerollData instance?
                    if (comparisonList != null)
                    {
                        dataList.Add(new RerollData(max, new ComparisonNode(comparisonList)));
                    }

                    // bad value?
                    if (arg.Value < 0 || Math.Floor(arg.Value) != arg.Value || arg.Value > Int32.MaxValue)
                    {
                        throw new DiceException(DiceErrorCode.BadRerollCount);
                    }

                    max = (int)arg.Value;
                    comparisonList = new List<ComparisonNode>();
                }
            }

            // ended with a non-comparison?
            if (comparisonList == null || comparisonList.Count == 0)
            {
                throw new DiceException(DiceErrorCode.IncorrectArgType, context.Name);
            }

            dataList.Add(new RerollData(max, new ComparisonNode(comparisonList)));
            DoReroll(context, dataList);
        }

        /// <summary>
        /// Rerolls the expression attached to the given context.
        /// This will overwrite context.Expression.Value, context.Expression.Values, context.Value, and context.Values.
        /// </summary>
        /// <param name="context">Function context containing expression to reroll.</param>
        /// <param name="rerollData">Data about comparisons to reroll as well as how many times to reroll.</param>
        private static void DoReroll(FunctionContext context, IEnumerable<RerollData> rerollData)
        {
            var values = new List<DieResult>();
            long rerolls = 0;
            var maxRerolls = context.Data.Config.MaxRerolls;

            foreach (var die in context.Expression!.Values)
            {
                bool rerolled = false;

                if (!die.IsLiveDie())
                {
                    values.Add(die);
                    continue;
                }

                foreach (var data in rerollData)
                {
                    if (rerolls >= maxRerolls || data.Current >= data.Max || !data.Comparison.Compare(die.Value))
                    {
                        continue;
                    }

                    rerolled = true;
                    var rr = die;

                    do
                    {
                        rerolls++;
                        data.Current++;
                        if (die != rr)
                        {
                            values.Add(new DieResult(SpecialDie.Add));
                        }

                        values.Add(rr.Drop());
                        rr = context.Reroll(die);
                    } while (rerolls < maxRerolls && data.Current < data.Max && data.Comparison.Compare(rr.Value));

                    values.Add(new DieResult(SpecialDie.Add));
                    values.Add(rr);
                }

                if (!rerolled)
                {
                    values.Add(die);
                }
            }

            var dice = values.Where(d => d.IsLiveDie());

            context.Values = values;
            if (context.Expression.ValueType == ResultType.Total)
            {
                context.Value = dice.Sum(d => d.Value);
                context.ValueType = ResultType.Total;
            }
            else
            {
                context.Value = dice.Sum(d => d.SuccessCount);
                context.ValueType = ResultType.Successes;
            }
        }
    }
}
