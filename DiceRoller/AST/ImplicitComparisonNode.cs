using System;
using System.Collections.Generic;
using System.Text;

namespace Dice.AST
{
    public class ImplicitComparisonNode : ComparisonNode
    {
        public DiceAST Expression { get; internal set; }

        public override IReadOnlyList<DieResult> Values => Expression.Values;

        protected internal override DiceAST UnderlyingRollNode => Expression.UnderlyingRollNode;

        internal ImplicitComparisonNode(DiceAST expression)
            : base(CompareOp.Equals, expression)
        {
            Expression = expression;
        }

        public override string ToString()
        {
            return Expression.ToString();
        }

        protected override long EvaluateInternal(RollData data, DiceAST root, int depth)
        {
            // unlike ComparisonNode, ImplicitComparisonNode may be used in a context where we utilize its Value
            long rolls = Expression.Evaluate(data, root, depth + 1);
            Value = Expression.Value;
            ValueType = Expression.ValueType;

            return rolls;
        }

        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            long rolls = Expression.Reroll(data, root, depth + 1);
            Value = Expression.Value;
            ValueType = Expression.ValueType;

            return rolls;
        }
    }
}
