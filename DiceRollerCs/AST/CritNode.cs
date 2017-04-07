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
        private List<DieResult> _values;

        /// <summary>
        /// What gets marked as a crit. It is possible for a roll to be marked as both a crit
        /// as well as a fumble. Can be null.
        /// </summary>
        public ComparisonNode Critical { get; private set; }

        /// <summary>
        /// What gets marked as a fumble. It is possible for a roll to be marked as both a crit
        /// as well as a fumble. Can be null.
        /// </summary>
        public ComparisonNode Fumble { get; private set; }

        /// <summary>
        /// The underlying dice expression to mark crits and fumbles on.
        /// </summary>
        public DiceAST Expression { get; internal set; }

        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        internal CritNode(ComparisonNode crit, ComparisonNode fumble)
        {
            Expression = null;
            Critical = crit;
            Fumble = fumble;
            _values = new List<DieResult>();

            if (crit == null && fumble == null)
            {
                throw new ArgumentException("crit and fumble cannot both be null.");
            }
        }

        internal void AddCritical(ComparisonNode comp)
        {
            if (comp == null)
            {
                throw new ArgumentNullException("comp");
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

        internal void AddFumble(ComparisonNode comp)
        {
            if (comp == null)
            {
                throw new ArgumentNullException("comp");
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

        protected override ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = Expression.Evaluate(conf, root, depth + 1);
            rolls += Critical?.Evaluate(conf, root, depth + 1) ?? 0;
            rolls += Fumble?.Evaluate(conf, root, depth + 1) ?? 0;
            MarkCrits();

            return rolls;
        }

        protected override ulong RerollInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = Expression.Reroll(conf, root, depth + 1);

            if (Critical?.Evaluated == false)
            {
                rolls += Critical.Evaluate(conf, root, depth + 1);
            }

            if (Fumble?.Evaluated == false)
            {
                rolls += Fumble.Evaluate(conf, root, depth + 1);
            }

            MarkCrits();

            return rolls;
        }

        private void MarkCrits()
        {
            Value = Expression.Value;
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
                }

                if (Fumble?.Compare(die.Value) == true)
                {
                    flags |= DieFlags.Fumble;
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
