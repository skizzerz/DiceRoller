using System;
using System.Collections.Generic;
using System.Text;

namespace Dice.AST
{
    /// <summary>
    /// A node that could be a comparison or could be a regular expression, depending on context.
    /// These nodes only show up as arguments to extras.
    /// </summary>
    public class ImplicitComparisonNode : ComparisonNode
    {
        /// <summary>
        /// The expression associated with this node. Depending on context,
        /// either the Value of this expression will be used, or this will be treated
        /// as a ComparisonNode with CompareOp.Equals.
        /// </summary>
        public DiceAST Expression { get; internal set; }

        /// <inheritdoc/>
        public override IReadOnlyList<DieResult> Values => Expression.Values;

        /// <inheritdoc/>
        protected internal override DiceAST UnderlyingRollNode => Expression.UnderlyingRollNode;

        /// <summary>
        /// Whether or not this node is probably being evaluated as an expression or a comparison.
        /// Impacts ToString but does not otherwise have any ramifications on the function implementation.
        /// </summary>
        internal bool IsExpression { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplicitComparisonNode"/> class with the specified expression.
        /// </summary>
        /// <param name="expression">Expression that may be a value expression or an implicit equals comparison.</param>
        internal ImplicitComparisonNode(DiceAST expression)
            : base(CompareOp.Equals, expression)
        {
            Expression = expression;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return IsExpression ? Expression.ToString() : base.ToString();
        }

        /// <inheritdoc/>
        protected override long EvaluateInternal(RollData data, DiceAST root, int depth)
        {
            // unlike ComparisonNode, ImplicitComparisonNode may be used in a context where we utilize its Value
            long rolls = Expression.Evaluate(data, root, depth + 1);
            Value = Expression.Value;
            ValueType = Expression.ValueType;

            return rolls;
        }

        /// <inheritdoc/>
        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            long rolls = Expression.Reroll(data, root, depth + 1);
            Value = Expression.Value;
            ValueType = Expression.ValueType;

            return rolls;
        }
    }
}
