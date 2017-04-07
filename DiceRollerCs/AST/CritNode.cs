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
        public ComparisonNode CritComparison { get; private set; }

        /// <summary>
        /// What gets marked as a fumble. It is possible for a roll to be marked as both a crit
        /// as well as a fumble. Can be null.
        /// </summary>
        public ComparisonNode FumbleComparison { get; private set; }

        /// <summary>
        /// The underlying dice expression to mark crits and fumbles on.
        /// </summary>
        public DiceAST Expression { get; internal set; }

        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        internal CritNode(DiceAST expression, ComparisonNode crit, ComparisonNode fumble)
        {
            Expression = expression ?? throw new ArgumentNullException("expression");
            CritComparison = crit;
            FumbleComparison = fumble;
            _values = new List<DieResult>();

            if (crit == null && fumble == null)
            {
                throw new ArgumentException("crit and fumble cannot both be null.");
            }
        }

        protected override ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = Expression.Evaluate(conf, root, depth + 1);
            rolls += CritComparison?.Evaluate(conf, root, depth + 1) ?? 0;
            rolls += FumbleComparison?.Evaluate(conf, root, depth + 1) ?? 0;
            MarkCrits();

            return rolls;
        }

        protected override ulong RerollInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = Expression.Reroll(conf, root, depth + 1);

            if (CritComparison?.Evaluated == false)
            {
                rolls += CritComparison.Evaluate(conf, root, depth + 1);
            }

            if (FumbleComparison?.Evaluated == false)
            {
                rolls += FumbleComparison.Evaluate(conf, root, depth + 1);
            }

            MarkCrits();

            return rolls;
        }

        private void MarkCrits()
        {
            Value = Expression.Value;
            _values.Clear();
            DieFlags mask = 0;

            if (CritComparison != null)
            {
                mask |= DieFlags.Critical;
            }

            if (FumbleComparison != null)
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

                if (CritComparison?.Compare(die.Value) == true)
                {
                    flags |= DieFlags.Critical;
                }

                if (FumbleComparison?.Compare(die.Value) == true)
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
