using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// Represents marking a die as a critical or fumble, for display purposes.
    /// If no CritNode is explicitly assigned to a die roll, an implicit one is assigned
    /// which marks the lowest roll result as a fumble and the highest roll result as a crit.
    /// </summary>
    public class CritNode : DiceAST
    {
        private readonly List<DieResult> _values;

        /// <summary>
        /// What gets marked as a crit. It is possible for a roll to be marked as both a crit
        /// as well as a fumble. Can be null.
        /// </summary>
        public ComparisonNode? Critical { get; private set; }

        /// <summary>
        /// What gets marked as a fumble. It is possible for a roll to be marked as both a crit
        /// as well as a fumble. Can be null.
        /// </summary>
        public ComparisonNode? Fumble { get; private set; }

        /// <summary>
        /// The underlying dice expression to mark crits and fumbles on.
        /// </summary>
        public DiceAST Expression { get; internal set; }

        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        internal CritNode(ComparisonNode? crit, ComparisonNode? fumble)
        {
            Expression = null!;
            Critical = crit;
            Fumble = fumble;
            _values = new List<DieResult>();

            if (crit == null && fumble == null)
            {
                throw new ArgumentException("crit and fumble cannot both be null.");
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Expression?.ToString() ?? String.Empty);

            if (Critical != null)
            {
                sb.AppendFormat(".critical({0})", Critical.ToString());
            }

            if (Fumble != null)
            {
                sb.AppendFormat(".fumble({0})", Fumble.ToString());
            }

            return sb.ToString();
        }

        internal void AddCritical(ComparisonNode? comp)
        {
            if (comp == null)
            {
                return;
            }

            if (Critical == null)
            {
                Critical = comp;
            }
            else
            {
                Critical.Add(comp);
            }
        }

        internal void AddFumble(ComparisonNode? comp)
        {
            if (comp == null)
            {
                return;
            }

            if (Fumble == null)
            {
                Fumble = comp;
            }
            else
            {
                Fumble.Add(comp);
            }
        }

        protected override long EvaluateInternal(RollData data, DiceAST root, int depth)
        {
            long rolls = Expression.Evaluate(data, root, depth + 1);
            rolls += Critical?.Evaluate(data, root, depth + 1) ?? 0;
            rolls += Fumble?.Evaluate(data, root, depth + 1) ?? 0;
            MarkCrits();

            return rolls;
        }

        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            long rolls = Expression.Reroll(data, root, depth + 1);
            MarkCrits();

            return rolls;
        }

        private void MarkCrits()
        {
            Value = Expression.Value;
            ValueType = Expression.ValueType;
            _values.Clear();
            DieFlags mask = 0;

            if (Critical != null)
            {
                mask |= DieFlags.Critical;
            }

            if (Fumble != null)
            {
                mask |= DieFlags.Fumble;
            }

            foreach (var die in Expression.Values)
            {
                DieFlags flags = 0;

                if (die.DieType == DieType.Special || die.DieType == DieType.Group)
                {
                    // we don't skip over dropped dice here since we DO still want to
                    // mark them as criticals/fumbles as needed.
                    _values.Add(die);
                    continue;
                }

                if (Critical?.Compare(die.Value) == true)
                {
                    flags |= DieFlags.Critical;

                    // if tracking successes; a critical success is worth 2 successes
                    if (ValueType == ResultType.Successes)
                    {
                        // just in case the die wasn't already marked as a success
                        if ((die.Flags & DieFlags.Success) == 0)
                        {
                            flags |= DieFlags.Success;
                            Value++;
                        }

                        Value++;
                    }
                }

                if (Fumble?.Compare(die.Value) == true)
                {
                    flags |= DieFlags.Fumble;

                    // if tracking failures; a critical failure is worth -2 successes
                    if (ValueType == ResultType.Successes)
                    {
                        // just in case the die wasn't already marked as a failure
                        if ((die.Flags & DieFlags.Failure) == 0)
                        {
                            flags |= DieFlags.Failure;
                            Value--;
                        }

                        Value--;
                    }
                }

                _values.Add(new DieResult()
                {
                    DieType = die.DieType,
                    NumSides = die.NumSides,
                    Value = die.Value,
                    // strip any existing crit/fumble flag off and use ours,
                    // assuming a comparison was defined for it.
                    // (we may have an existing flag if the die rolled min or max value)
                    Flags = (die.Flags & ~mask) | flags
                });
            }
        }
    }
}
