using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    public class ComparisonNode : DiceAST
    {
        /// <summary>
        /// Type of comparison to perform
        /// </summary>
        public CompareOp Operation { get; private set; }

        /// <summary>
        /// Value to compare against
        /// </summary>
        public DiceAST Expression { get; private set; }

        public override IReadOnlyList<DieResult> Values
        {
            get { return Expression.Values; }
        }

        internal ComparisonNode(CompareOp operation, DiceAST expression)
        {
            Expression = expression ?? throw new ArgumentNullException("expression");
            Operation = operation;
        }

        protected override ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            // this doesn't increase depth as there is no actual logic that a ComparisonNode itself performs
            // (in other words, the Expression can be viewed as the ComparisonNode's evaluation)
            var rolls = Expression.Evaluate(conf, root, depth);
            Value = Expression.Value;

            return rolls;
        }

        protected override ulong RerollInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            var rolls = Expression.Reroll(conf, root, depth);
            Value = Expression.Value;

            return rolls;
        }

        public bool Compare(decimal theirValue)
        {
            switch (Operation)
            {
                case CompareOp.Equals:
                    return theirValue == Value;
                case CompareOp.GreaterEquals:
                    return theirValue >= Value;
                case CompareOp.GreaterThan:
                    return theirValue > Value;
                case CompareOp.LessEquals:
                    return theirValue <= Value;
                case CompareOp.LessThan:
                    return theirValue < Value;
                case CompareOp.NotEquals:
                    return theirValue != Value;
                default:
                    throw new InvalidOperationException("Unknown Comparison Operation");
            }
        }
    }
}
