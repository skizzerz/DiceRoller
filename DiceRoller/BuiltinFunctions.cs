using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using Dice.AST;

namespace Dice
{
    /// <summary>
    /// Built-in functions.
    /// </summary>
    public static class BuiltinFunctions
    {
        // these are reserved in every scope
        internal static readonly Dictionary<string, string> ReservedNames = new Dictionary<string, string>
        {
            // key = lowercased name, value = properly-cased name
            { "reroll", "reroll" },
            { "rerolln", "rerollN" },
            { "rerollonce", "rerollOnce" },
            { "explode", "explode" },
            { "compound", "compound" },
            { "penetrate", "penetrate" },
            { "compoundpenetrate", "compoundPenetrate" },
            { "keephighest", "keepHighest" },
            { "keeplowest", "keepLowest" },
            { "drophighest", "dropHighest" },
            { "droplowest", "dropLowest" },
            { "advantage", "advantage" },
            { "disadvantage", "disadvantage" },
            { "success", "success" },
            { "failure", "failure" },
            { "critical", "critical" },
            { "fumble", "fumble" },
            { "sortasc", "sortAsc" },
            { "sortdesc", "sortDesc" }
        };

        [DiceFunction("floor", Scope = FunctionScope.Global)]
        public static void Floor(FunctionContext context)
        {
            if (context.Arguments.Count != 1)
            {
                throw new DiceException(DiceErrorCode.IncorrectArity, "floor");
            }

            var arg = context.Arguments[0];

            if (arg is ComparisonNode)
            {
                throw new DiceException(DiceErrorCode.IncorrectArgType, "floor");
            }

            var values = new List<DieResult>
            {
                new DieResult("floor"),
                new DieResult(SpecialDie.OpenParen)
            };

            values.AddRange(arg.Values);
            values.Add(new DieResult(SpecialDie.CloseParen));

            context.Value = Math.Floor(arg.Value);
            context.Values = values;
            context.ValueType = arg.ValueType;
        }

        [DiceFunction("ceil", Scope = FunctionScope.Global)]
        public static void Ceiling(FunctionContext context)
        {
            if (context.Arguments.Count != 1)
            {
                throw new DiceException(DiceErrorCode.IncorrectArity, "ceil");
            }

            var arg = context.Arguments[0];

            if (arg is ComparisonNode)
            {
                throw new DiceException(DiceErrorCode.IncorrectArgType, "ceil");
            }

            var values = new List<DieResult>
            {
                new DieResult("ceil"),
                new DieResult(SpecialDie.OpenParen)
            };

            values.AddRange(arg.Values);
            values.Add(new DieResult(SpecialDie.CloseParen));

            context.Value = Math.Ceiling(arg.Value);
            context.Values = values;
            context.ValueType = arg.ValueType;
        }

        [DiceFunction("round", Scope = FunctionScope.Global)]
        public static void Round(FunctionContext context)
        {
            if (context.Arguments.Count != 1)
            {
                throw new DiceException(DiceErrorCode.IncorrectArity, "round");
            }

            var arg = context.Arguments[0];

            if (arg is ComparisonNode)
            {
                throw new DiceException(DiceErrorCode.IncorrectArgType, "round");
            }

            var values = new List<DieResult>
            {
                new DieResult("round"),
                new DieResult(SpecialDie.OpenParen)
            };

            values.AddRange(arg.Values);
            values.Add(new DieResult(SpecialDie.CloseParen));

            context.Value = Math.Round(arg.Value, MidpointRounding.AwayFromZero);
            context.Values = values;
            context.ValueType = arg.ValueType;
        }

        [DiceFunction("abs", Scope = FunctionScope.Global)]
        public static void Abs(FunctionContext context)
        {
            if (context.Arguments.Count != 1)
            {
                throw new DiceException(DiceErrorCode.IncorrectArity, "abs");
            }

            var arg = context.Arguments[0];

            if (arg is ComparisonNode)
            {
                throw new DiceException(DiceErrorCode.IncorrectArgType, "abs");
            }

            var values = new List<DieResult>
            {
                new DieResult("abs"),
                new DieResult(SpecialDie.OpenParen)
            };

            values.AddRange(arg.Values);
            values.Add(new DieResult(SpecialDie.CloseParen));

            context.Value = Math.Abs(arg.Value);
            context.Values = values;
            context.ValueType = arg.ValueType;
        }

        [DiceFunction("max", Scope = FunctionScope.Global)]
        public static void Max(FunctionContext context)
        {
            if (context.Arguments.Count != 2)
            {
                throw new DiceException(DiceErrorCode.IncorrectArity, "max");
            }

            var arg1 = context.Arguments[0];
            var arg2 = context.Arguments[1];

            if (arg1 is ComparisonNode || arg2 is ComparisonNode)
            {
                throw new DiceException(DiceErrorCode.IncorrectArgType, "max");
            }

            context.Value = Math.Max(arg1.Value, arg2.Value);
            bool keptFirst = context.Value == arg1.Value;

            List<DieResult> values = new List<DieResult>()
            {
                new DieResult("max"),
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

        [DiceFunction("min", Scope = FunctionScope.Global)]
        public static void Min(FunctionContext context)
        {
            if (context.Arguments.Count != 2)
            {
                throw new DiceException(DiceErrorCode.IncorrectArity, "min");
            }

            var arg1 = context.Arguments[0];
            var arg2 = context.Arguments[1];

            if (arg1 is ComparisonNode || arg2 is ComparisonNode)
            {
                throw new DiceException(DiceErrorCode.IncorrectArgType, "min");
            }

            context.Value = Math.Min(arg1.Value, arg2.Value);
            bool keptFirst = context.Value == arg1.Value;

            List<DieResult> values = new List<DieResult>()
            {
                new DieResult("min"),
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

        [DiceFunction("if", Scope = FunctionScope.Global)]
        public static void If(FunctionContext context)
        {
            if (context.Arguments.Count < 3 || context.Arguments.Count > 4)
            {
                throw new DiceException(DiceErrorCode.IncorrectArity, "if");
            }

            var test = context.Arguments[0];
            var compare = context.Arguments[1] as ComparisonNode;
            var then = context.Arguments[2];
            var otherwise = context.Arguments.ElementAtOrDefault(3);

            if (test is ComparisonNode || compare == null || then is ComparisonNode || otherwise is ComparisonNode)
            {
                throw new DiceException(DiceErrorCode.IncorrectArgType, "if");
            }

            List<DieResult> values = new List<DieResult>()
            {
                new DieResult(SpecialDie.OpenParen)
            };

            if (compare.Compare(test.Value))
            {
                context.Value = then.Value;
                context.ValueType = then.ValueType;
                values.AddRange(then.Values);
            }
            else if (otherwise != null)
            {
                context.Value = otherwise.Value;
                context.ValueType = otherwise.ValueType;
                values.AddRange(otherwise.Values);
            }
            else
            {
                context.Value = 0;
                values.Add(new DieResult()
                {
                    DieType = DieType.Literal,
                    NumSides = 0,
                    Value = 0,
                    Flags = DieFlags.Macro
                });
            }

            values.Add(new DieResult(SpecialDie.CloseParen));
            context.Values = values;
        }

        [DiceFunction("expand", Scope = FunctionScope.Group, Timing = FunctionTiming.BeforeSort)]
        public static void Expand(FunctionContext context)
        {
            if (context.Arguments.Count > 0)
            {
                throw new DiceException(DiceErrorCode.IncorrectArity, "expand");
            }

            List<DieResult> values = new List<DieResult>();
            context.Value = context.Expression.Value;
            context.ValueType = context.Expression.ValueType;
            var groupNode = (GroupNode)context.Expression.UnderlyingRollNode;

            foreach (var value in context.Expression.Values)
            {
                if (value.DieType != DieType.Group)
                {
                    values.Add(value);
                    continue;
                }

                var groupValues = context.Data.InternalContext.GetGroupValues(value.Data);
                bool markDropped = value.Flags.HasFlag(DieFlags.Dropped);
                bool needParens = groupNode.Expressions.Count > 1;

                if (needParens)
                {
                    values.Add(new DieResult(SpecialDie.OpenParen));
                }

                foreach (var die in groupValues.Values)
                {
                    if (markDropped && die.IsLiveDie())
                    {
                        values.Add(die.Drop());
                    }
                    else
                    {
                        values.Add(die);
                    }
                }

                if (needParens)
                {
                    values.Add(new DieResult(SpecialDie.CloseParen));
                }
            }

            context.Values = values;
        }

        [DiceMacro("numDice")]
        public static void NumDice(MacroContext context)
        {
            long dice = 0;

            foreach (var roll in context.Data.InternalContext.RollExpressions)
            {
                dice += (long)roll.NumDice.Value;
            }

            context.Value = dice;
            context.ValueType = ResultType.Total;
        }
    }
}
