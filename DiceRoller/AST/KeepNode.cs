using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

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
        public DiceAST Expression { get; internal set; }

        /// <summary>
        /// All rolled dice. Dropped dice are marked with the DieFlags.Dropped flag
        /// </summary>
        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        protected internal override DiceAST UnderlyingRollNode => Expression.UnderlyingRollNode;

        internal KeepNode(KeepType keepType, DiceAST amount)
        {
            KeepType = keepType;
            Expression = null;
            _values = new List<DieResult>();

            if (keepType == KeepType.Advantage || keepType == KeepType.Disadvantage)
            {
                if (amount != null)
                {
                    throw new ArgumentException("amount must be null if keep type is advantage or disadvantage");
                }

                Amount = null;
            }
            else
            {
                Amount = amount ?? throw new ArgumentNullException("amount");
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Expression?.ToString() ?? String.Empty);

            switch (KeepType)
            {
                case KeepType.KeepHigh:
                    sb.Append(".keepHighest");
                    break;
                case KeepType.KeepLow:
                    sb.Append(".keepLowest");
                    break;
                case KeepType.DropHigh:
                    sb.Append(".dropHighest");
                    break;
                case KeepType.DropLow:
                    sb.Append(".dropLowest");
                    break;
                case KeepType.Advantage:
                    sb.Append(".advantage");
                    break;
                case KeepType.Disadvantage:
                    sb.Append(".disadvantage");
                    break;
                default:
                    sb.Append(".<<UNKNOWN KEEP>>");
                    break;
            }

            sb.AppendFormat("({0})", Amount?.ToString() ?? String.Empty);

            return sb.ToString();
        }

        protected override long EvaluateInternal(RollData data, DiceAST root, int depth)
        {
            var rolls = Amount?.Evaluate(data, root, depth + 1) ?? 0;
            rolls += Expression.Evaluate(data, root, depth + 1);
            rolls += ApplyKeep(data, root, depth);

            return rolls;
        }

        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            long rolls = Expression.Reroll(data, root, depth + 1);
            rolls += ApplyKeep(data, root, depth);

            return rolls;
        }

        private long ApplyKeep(RollData data, DiceAST root, int depth)
        {
            if (KeepType == KeepType.Advantage || KeepType == KeepType.Disadvantage)
            {
                return ApplyAdvantage(data, root, depth);
            }

            var sortedValues = Expression.Values
                .Where(d => d.IsLiveDie())
                .OrderBy(d => d.Value).ToList();
            var amount = (int)Amount.Value;

            if (amount < 0)
            {
                throw new DiceException(DiceErrorCode.NegativeDice);
            }

            switch (KeepType)
            {
                case KeepType.DropHigh:
                    sortedValues = sortedValues.Take(sortedValues.Count - amount).ToList();
                    break;
                case KeepType.KeepLow:
                    sortedValues = sortedValues.Take(amount).ToList();
                    break;
                case KeepType.DropLow:
                    sortedValues = sortedValues.Skip(amount).ToList();
                    break;
                case KeepType.KeepHigh:
                    sortedValues = sortedValues.Skip(sortedValues.Count - amount).ToList();
                    break;
                default:
                    throw new InvalidOperationException("Unknown keep type");
            }

            if (Expression.ValueType == ResultType.Total)
            {
                Value = sortedValues.Sum(d => d.Value);
                ValueType = ResultType.Total;
            }
            else
            {
                Value = sortedValues.Sum(d => d.SuccessCount);
                ValueType = ResultType.Successes;
            }

            _values.Clear();
            foreach (var d in Expression.Values)
            {
                if (!d.IsLiveDie())
                {
                    // while we apply drop/keep on grouped die results, special dice are passed as-is
                    // also if the die was already dropped, we don't try to drop it again
                    _values.Add(d);
                    continue;
                }

                if (sortedValues.Contains(d))
                {
                    _values.Add(d);
                    sortedValues.Remove(d);
                }
                else
                {
                    _values.Add(d.Drop());
                }
            }

            return 0;
        }

        private long ApplyAdvantage(RollData data, DiceAST root, int depth)
        {
            Value = Expression.Value;
            ValueType = Expression.ValueType;
            _values = Expression.Values.ToList();
            var rolls = Expression.Reroll(data, root, depth + 1);

            if ((KeepType == KeepType.Advantage && Expression.Value > Value)
                || (KeepType == KeepType.Disadvantage && Expression.Value < Value))
            {
                for (int i = 0; i < _values.Count; i++)
                {
                    _values[i] = _values[i].Drop();
                }

                Value = Expression.Value;
                _values.Add(new DieResult(SpecialDie.Add));
                _values.AddRange(Expression.Values);
            }
            else
            {
                _values.Add(new DieResult(SpecialDie.Add));
                _values.AddRange(Expression.Values.Select(d => d.Drop()));
            }

            return rolls;
        }
    }
}
