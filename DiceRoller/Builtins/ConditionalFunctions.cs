using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dice.AST;

namespace Dice.Builtins
{
    /// <summary>
    /// Builtin functions that enable conditional execution.
    /// </summary>
    public static class ConditionalFunctions
    {
        /// <summary>
        /// Returns one of two different results depending on whether or not a comparison
        /// succeeds against an expression.
        /// </summary>
        /// <param name="context">Function context.</param>
        [DiceFunction("if", Scope = FunctionScope.Global, ArgumentPattern = "ECEE?")]
        public static void If(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var test = context.Arguments[0];
            var compare = (ComparisonNode)context.Arguments[1];
            var then = context.Arguments[2];
            var otherwise = context.Arguments.ElementAtOrDefault(3);

            var values = new List<DieResult>()
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
    }
}
