using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Dice.AST;

namespace Dice.Builtins
{
    public static class OutputFunctions
    {
        [DiceFunction("expand", Scope = FunctionScope.Group, Timing = FunctionTiming.BeforeSort, ArgumentPattern = "")]
        public static void Expand(FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            List<DieResult> values = new List<DieResult>();
            context.Value = context.Expression!.Value;
            context.ValueType = context.Expression.ValueType;
            var groupNode = (GroupNode)context.Expression.UnderlyingRollNode;

            foreach (var value in context.Expression.Values)
            {
                if (value.DieType != DieType.Group)
                {
                    values.Add(value);
                    continue;
                }

                if (value.Data == null)
                {
                    throw new InvalidOperationException("Grouped die roll is missing group key");
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
    }
}
