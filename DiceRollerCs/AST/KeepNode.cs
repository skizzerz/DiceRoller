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
                    sb.Append(".keepHigh");
                    break;
                case KeepType.KeepLow:
                    sb.Append(".keepLow");
                    break;
                case KeepType.DropHigh:
                    sb.Append(".dropHigh");
                    break;
                case KeepType.DropLow:
                    sb.Append(".dropLow");
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

            sb.AppendFormat("({0})", Amount.ToString());

            return sb.ToString();
        }

        protected override long EvaluateInternal(RollerConfig conf, DiceAST root, int depth)
        {
            var rolls = Amount.Evaluate(conf, root, depth + 1);
            rolls += Expression.Evaluate(conf, root, depth + 1);
            rolls += ApplyKeep(conf, root, depth);

            return rolls;
        }

        protected override long RerollInternal(RollerConfig conf, DiceAST root, int depth)
        {
            long rolls = 0;

            if (!Amount.Evaluated)
            {
                rolls += Amount.Evaluate(conf, root, depth + 1);
            }

            rolls += Expression.Reroll(conf, root, depth + 1);
            rolls += ApplyKeep(conf, root, depth);

            return rolls;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Cannot be easily refactored.")]
        private long ApplyKeep(RollerConfig conf, DiceAST root, int depth)
        {
            if (KeepType == KeepType.Advantage || KeepType == KeepType.Disadvantage)
            {
                Value = Expression.Value;
                ValueType = Expression.ValueType;
                _values = Expression.Values.ToList();
                var rolls = Expression.Reroll(conf, root, depth + 1);

                if ((KeepType == KeepType.Advantage && Expression.Value > Value)
                    || (KeepType == KeepType.Disadvantage && Expression.Value < Value))
                {
                    for (int i = 0; i < _values.Count; i++)
                    {
                        _values[i] = new DieResult()
                        {
                            DieType = _values[i].DieType,
                            NumSides = _values[i].NumSides,
                            Value = _values[i].Value,
                            Flags = _values[i].Flags | DieFlags.Dropped
                        };
                    }

                    Value = Expression.Value;
                    _values.AddRange(Expression.Values);
                }
                else
                {
                    _values.AddRange(Expression.Values.Select(d => new DieResult()
                    {
                        DieType = d.DieType,
                        NumSides = d.NumSides,
                        Value = d.Value,
                        Flags = d.Flags | DieFlags.Dropped
                    }));
                }

                return rolls;
            }

            var sortedValues = Expression.Values
                .Where(d => d.DieType != DieType.Special && !d.Flags.HasFlag(DieFlags.Dropped))
                .OrderBy(d => d.Value).ToList();
            var amount = (int)Amount.Value;

            if (amount < 0)
            {
                throw new DiceException(DiceErrorCode.NegativeDice);
            }

            switch (KeepType)
            {
                case KeepType.DropHigh:
                case KeepType.KeepLow:
                    sortedValues = sortedValues.Take(sortedValues.Count - amount).ToList();
                    break;
                case KeepType.DropLow:
                case KeepType.KeepHigh:
                    sortedValues = sortedValues.Skip(amount).ToList();
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
                Value = sortedValues.Sum(d => d.Flags.HasFlag(DieFlags.Success) ? 1 : (d.Flags.HasFlag(DieFlags.Failure) ? -1 : 0));
                ValueType = ResultType.Successes;
            }
            _values.Clear();
            foreach (var d in Expression.Values)
            {
                if (d.DieType == DieType.Special || d.Flags.HasFlag(DieFlags.Dropped))
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
                    _values.Add(new DieResult()
                    {
                        DieType = d.DieType,
                        NumSides = d.NumSides,
                        Value = d.Value,
                        Flags = d.Flags | DieFlags.Dropped
                    });
                }
            }

            return 0;
        }
    }
}
