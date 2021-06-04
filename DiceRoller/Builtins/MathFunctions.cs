using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Dice.AST;

namespace Dice.Builtins
{
    public static class MathFunctions
    {
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
