using System;
using System.Collections.Generic;

namespace Dice.AST
{
    /// <summary>
    /// Represents a node in which we reroll the underlying expression should a comparison match
    /// </summary>
    public class RerollNode : DiceAST
    {
        /// <summary>
        /// The comparison to determine whether or not to reroll
        /// </summary>
        public ComparisonNode Comparison { get; private set; }

        /// <summary>
        /// The expression to reroll
        /// </summary>
        public DiceAST Expression { get; private set; }

        /// <summary>
        /// Maximum number of times to reroll, or 0 if unlimited rerolls are allowed
        /// </summary>
        public uint MaxRerolls { get; private set; }

        public override IReadOnlyList<DieResult> Values
        {
            get { return Expression.Values; }
        }

        internal RerollNode(uint maxRerolls, DiceAST expression, ComparisonNode comparison)
        {
            Comparison = comparison ?? throw new ArgumentNullException("comparison");
            Expression = expression ?? throw new ArgumentNullException("expression");
            MaxRerolls = maxRerolls;
        }

        protected override ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            var rolls = Comparison.Evaluate(conf, root, depth + 1);
            rolls += Expression.Evaluate(conf, root, depth + 1);
            rolls += MaybeReroll(conf, root, depth);
            Value = Expression.Value;

            return rolls;
        }

        protected override ulong RerollInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            var rolls = Expression.Reroll(conf, root, depth + 1);
            rolls += MaybeReroll(conf, root, depth);
            Value = Expression.Value;

            return rolls;
        }

        private ulong MaybeReroll(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = 0;
            uint rerolls = 0;
            var maxRerolls = MaxRerolls == 0 ? conf.MaxRerolls : Math.Min(MaxRerolls, conf.MaxRerolls);

            while (!Comparison.Compare(Expression.Value))
            {
                rolls += Expression.Reroll(conf, root, depth + 1);
                rerolls++;
                if (rerolls >= conf.MaxRerolls)
                {
                    break;
                }
            }

            return rolls;
        }
    }
}
