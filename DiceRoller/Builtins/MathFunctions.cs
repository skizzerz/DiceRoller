using System;
using System.Collections.Generic;
using System.Linq;

namespace Dice.Builtins
{
    /// <summary>
    /// Functions that perform mathematical operations.
    /// </summary>
    public static class MathFunctions
    {
        /// <summary>
        /// Computes the floor of the argument (the largest integer less than or equal to the number).
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("floor", Scope = FunctionScope.Global, ArgumentPattern = "E")]
        public static void Floor(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var arg = context.Arguments[0];
            var values = new List<DieResult>
            {
                new DieResult(context.Name),
                new DieResult(SpecialDie.OpenParen)
            };

            values.AddRange(arg.Values);
            values.Add(new DieResult(SpecialDie.CloseParen));

            context.Value = Math.Floor(arg.Value);
            context.Values = values;
            context.ValueType = arg.ValueType;
        }

        /// <summary>
        /// Computes the ceiling of the argument (the smallest integer greater than or equal to the number).
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("ceil", Scope = FunctionScope.Global, ArgumentPattern = "E")]
        public static void Ceiling(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var arg = context.Arguments[0];
            var values = new List<DieResult>
            {
                new DieResult(context.Name),
                new DieResult(SpecialDie.OpenParen)
            };

            values.AddRange(arg.Values);
            values.Add(new DieResult(SpecialDie.CloseParen));

            context.Value = Math.Ceiling(arg.Value);
            context.Values = values;
            context.ValueType = arg.ValueType;
        }

        /// <summary>
        /// Rounds the argument, with 0.5 rounding away from 0 (1.5 rounds to 2, -1.5 rounds to -2).
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("round", Scope = FunctionScope.Global, ArgumentPattern = "E")]
        public static void Round(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var arg = context.Arguments[0];
            var values = new List<DieResult>
            {
                new DieResult(context.Name),
                new DieResult(SpecialDie.OpenParen)
            };

            values.AddRange(arg.Values);
            values.Add(new DieResult(SpecialDie.CloseParen));

            context.Value = Math.Round(arg.Value, MidpointRounding.AwayFromZero);
            context.Values = values;
            context.ValueType = arg.ValueType;
        }

        /// <summary>
        /// Computes the absolute value of the argument.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("abs", Scope = FunctionScope.Global, ArgumentPattern = "E")]
        public static void Abs(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var arg = context.Arguments[0];
            var values = new List<DieResult>
            {
                new DieResult(context.Name),
                new DieResult(SpecialDie.OpenParen)
            };

            values.AddRange(arg.Values);
            values.Add(new DieResult(SpecialDie.CloseParen));

            context.Value = Math.Abs(arg.Value);
            context.Values = values;
            context.ValueType = arg.ValueType;
        }

        /// <summary>
        /// Returns the maximum value of two arguments.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("max", Scope = FunctionScope.Global, ArgumentPattern = "EE")]
        public static void Max(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var arg1 = context.Arguments[0];
            var arg2 = context.Arguments[1];
            context.Value = Math.Max(arg1.Value, arg2.Value);
            bool keptFirst = context.Value == arg1.Value;

            List<DieResult> values = new List<DieResult>()
            {
                new DieResult(context.Name),
                new DieResult(SpecialDie.OpenParen)
            };

            if (!keptFirst)
            {
                values.AddRange(arg1.Values.Select(d => d.Drop()));
            }
            else
            {
                values.AddRange(arg1.Values);
            }

            values.Add(new DieResult(SpecialDie.Comma));

            if (keptFirst)
            {
                values.AddRange(arg2.Values.Select(d => d.Drop()));
            }
            else
            {
                values.AddRange(arg2.Values);
            }

            values.Add(new DieResult(SpecialDie.CloseParen));

            // we maintain a ValueType of successes only if all sides which contain actual rolls have a successes ValueType
            bool haveTotal = false;
            bool haveRoll = false;

            if (arg1.Values.Any(d => d.DieType.IsRoll()) == true)
            {
                haveRoll = true;

                if (arg1.ValueType == ResultType.Total)
                {
                    haveTotal = true;
                }
            }

            if (arg2.Values.Any(d => d.DieType.IsRoll()))
            {
                haveRoll = true;

                if (arg2.ValueType == ResultType.Total)
                {
                    haveTotal = true;
                }
            }

            if (!haveRoll || haveTotal)
            {
                context.ValueType = ResultType.Total;
            }
            else
            {
                context.ValueType = ResultType.Successes;
            }

            context.Values = values;
        }

        /// <summary>
        /// Returns the minimum value of two arguments.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("min", Scope = FunctionScope.Global, ArgumentPattern = "EE")]
        public static void Min(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var arg1 = context.Arguments[0];
            var arg2 = context.Arguments[1];
            context.Value = Math.Min(arg1.Value, arg2.Value);
            bool keptFirst = context.Value == arg1.Value;

            List<DieResult> values = new List<DieResult>()
            {
                new DieResult(context.Name),
                new DieResult(SpecialDie.OpenParen)
            };

            if (!keptFirst)
            {
                values.AddRange(arg1.Values.Select(d => d.Drop()));
            }
            else
            {
                values.AddRange(arg1.Values);
            }

            values.Add(new DieResult(SpecialDie.Comma));

            if (keptFirst)
            {
                values.AddRange(arg2.Values.Select(d => d.Drop()));
            }
            else
            {
                values.AddRange(arg2.Values);
            }

            values.Add(new DieResult(SpecialDie.CloseParen));

            // we maintain a ValueType of successes only if all sides which contain actual rolls have a successes ValueType
            bool haveTotal = false;
            bool haveRoll = false;

            if (arg1.Values.Any(d => d.DieType.IsRoll()) == true)
            {
                haveRoll = true;

                if (arg1.ValueType == ResultType.Total)
                {
                    haveTotal = true;
                }
            }

            if (arg2.Values.Any(d => d.DieType.IsRoll()))
            {
                haveRoll = true;

                if (arg2.ValueType == ResultType.Total)
                {
                    haveTotal = true;
                }
            }

            if (!haveRoll || haveTotal)
            {
                context.ValueType = ResultType.Total;
            }
            else
            {
                context.ValueType = ResultType.Successes;
            }

            context.Values = values;
        }
    }
}
