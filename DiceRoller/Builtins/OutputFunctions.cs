using System;
using System.Collections.Generic;

using Dice.AST;

namespace Dice.Builtins
{
    /// <summary>
    /// Functions that do not change the roll result but manipulate the output in some fashion.
    /// </summary>
    public static class OutputFunctions
    {
        /// <summary>
        /// Expands a grouped dice roll into the individual dice roll rather than one meta-result per grouping.
        /// </summary>
        /// <param name="context">Function context.</param>
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
