using System;
using System.Collections.Generic;
using System.Linq;

using Dice.Exceptions;

namespace Dice.AST
{
    /// <summary>
    /// A node representing that only some of the rolled dice should be kept
    /// </summary>
    public class KeepNode : DiceAST
    {
        private List<DieResult> _values;

        /// <summary>
        /// What type of keep node this is
        /// </summary>
        public KeepType KeepType { get; private set; }

        /// <summary>
        /// Number of dice to keep/drop, this will be null
        /// if KeepType is Advantage or Disadvantage
        /// </summary>
        public DiceAST Amount { get; private set; }

        /// <summary>
        /// Underlying roll expression
        /// </summary>
        public DiceAST Expression { get; private set; }

        /// <summary>
        /// Values of all kept dice
        /// </summary>
        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        internal KeepNode(KeepType keepType, DiceAST amount, DiceAST expression)
        {
            KeepType = keepType;
            Amount = amount ?? throw new ArgumentNullException("amount");
            Expression = expression ?? throw new ArgumentNullException("expression");
            _values = new List<DieResult>();
        }

        protected override ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            var rolls = Amount.Evaluate(conf, root, depth + 1);
            rolls += Expression.Evaluate(conf, root, depth + 1);
            rolls += ApplyKeep(conf, root, depth);

            return rolls;
        }

        protected override ulong RerollInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = 0;

            if (!Amount.Evaluated)
            {
                rolls += Amount.Evaluate(conf, root, depth + 1);
            }

            rolls += Expression.Reroll(conf, root, depth + 1);
            rolls += ApplyKeep(conf, root, depth);

            return rolls;
        }

        private ulong ApplyKeep(RollerConfig conf, DiceAST root, uint depth)
        {
            if (KeepType == KeepType.Advantage || KeepType == KeepType.Disadvantage)
            {
                Value = Expression.Value;
                _values = Expression.Values.ToList();
                var rolls = Expression.Reroll(conf, root, depth + 1);

                if ((KeepType == KeepType.Advantage && Expression.Value > Value)
                    || (KeepType == KeepType.Disadvantage && Expression.Value < Value))
                {
                    Value = Expression.Value;
                    _values = Expression.Values.ToList();
                }

                return rolls;
            }

            _values = Expression.Values.OrderBy(d => d.Value).ToList();
            var amount = (int)Amount.Value;

            if (amount < 0)
            {
                throw new BadDiceException();
            }

            switch (KeepType)
            {
                case KeepType.DropHigh:
                case KeepType.KeepLow:
                    _values = _values.Take(_values.Count - amount).ToList();
                    break;
                case KeepType.DropLow:
                case KeepType.KeepHigh:
                    _values = _values.Skip(amount).ToList();
                    break;
                default:
                    throw new InvalidOperationException("Unknown keep type");
            }

            Value = _values.Sum(d => d.Value);

            return 0;
        }
    }
}
