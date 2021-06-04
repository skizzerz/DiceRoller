using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Dice.Builtins
{
    /// <summary>
    /// Builtin functions that sort the list of dice that were rolled.
    /// </summary>
    public static class SortFunctions
    {
        /// <summary>
        /// Validates that only one sort is being performed on the dice.
        /// </summary>
        /// <param name="sender">Unused.</param>
        /// <param name="e">Event arguments.</param>
        [SuppressMessage("Security", "CA2109:Review visible event handlers",
            Justification = "Public to allow library consumers to remove this validation event from BuiltinFunctionRegistry")]
        public static void ValidateSort(object sender, ValidateEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (e.Timing != FunctionTiming.Sort)
            {
                return;
            }

            if (e.Contexts.Count > 1)
            {
                throw new DiceException(DiceErrorCode.TooManySort);
            }
        }

        /// <summary>
        /// Sort dice in ascending order.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("sortAsc", "sa",
            ArgumentPattern = "",
            Scope = FunctionScope.Roll,
            Timing = FunctionTiming.Sort)]
        public static void SortAscending(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            DoSort(context, ascending: true);
        }

        /// <summary>
        /// Sort dice in descending order.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("sortDesc", "sd",
            ArgumentPattern = "",
            Scope = FunctionScope.Roll,
            Timing = FunctionTiming.Sort)]
        public static void SortDescending(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            DoSort(context, ascending: false);
        }

        private static void DoSort(FunctionContext context, bool ascending)
        {
            var temp = new List<DieResult>();
            var positions = new List<int>();
            SpecialDie? chainType = null;

            var values = context.Expression!.Values.ToList();
            context.Value = context.Expression.Value;
            context.ValueType = context.Expression.ValueType;

            void FixOrder()
            {
                if (temp.Count == 0)
                {
                    return;
                }

                var sorted = ascending ? temp.OrderBy(d => d.Value) : temp.OrderByDescending(d => d.Value);
                var enumerator = sorted.GetEnumerator();
                foreach (var pos in positions)
                {
                    enumerator.MoveNext();
                    values[pos] = enumerator.Current;
                }

                temp.Clear();
                positions.Clear();
            }

            for (int i = 0; i < values.Count; i++)
            {
                var die = values[i];

                if (die.DieType == DieType.Special)
                {
                    switch ((SpecialDie)die.Value)
                    {
                        case SpecialDie.Add:
                            if (chainType == null && temp.Count > 0)
                            {
                                chainType = SpecialDie.Add;
                            }
                            else if (chainType != SpecialDie.Add)
                            {
                                FixOrder();
                            }

                            break;
                        case SpecialDie.Multiply:
                            if (chainType == null && temp.Count > 0)
                            {
                                chainType = SpecialDie.Multiply;
                            }
                            else if (chainType != SpecialDie.Multiply)
                            {
                                FixOrder();
                            }

                            break;
                        default:
                            chainType = null;
                            FixOrder();
                            break;
                    }
                }
                else
                {
                    temp.Add(die);
                    positions.Add(i);
                }
            }

            FixOrder();
            context.Values = values;
        }
    }
}
