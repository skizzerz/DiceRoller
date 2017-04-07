using System;
using System.Collections.Generic;
using System.Linq;

namespace Dice.AST
{
    /// <summary>
    /// Represents a node in which we reroll the underlying expression should a comparison match
    /// </summary>
    public class RerollNode : DiceAST
    {
        List<DieResult> _values;

        /// <summary>
        /// The comparison to determine whether or not to reroll
        /// </summary>
        public ComparisonNode Comparison { get; private set; }

        /// <summary>
        /// The expression to reroll
        /// </summary>
        public DiceAST Expression { get; internal set; }

        /// <summary>
        /// Maximum number of times to reroll, or 0 if unlimited rerolls are allowed
        /// </summary>
        public uint MaxRerolls { get; private set; }

        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        internal RerollNode(uint maxRerolls, ComparisonNode comparison)
        {
            Comparison = comparison ?? throw new ArgumentNullException("comparison");
            Expression = null;
            MaxRerolls = maxRerolls;
            _values = new List<DieResult>();
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
            _values.Clear();

            foreach (var die in Expression.Values)
            {
                if (die.DieType == DieType.Group || die.DieType == DieType.Special || die.Flags.HasFlag(DieFlags.Dropped) || !Comparison.Compare(die.Value))
                {
                    _values.Add(die);
                    continue;
                }

                _values.Add(new DieResult()
                {
                    DieType = die.DieType,
                    NumSides = die.NumSides,
                    Value = die.Value,
                    Flags = die.Flags | DieFlags.Dropped
                });

                rolls++;
                rerolls++;
                RollType rt = RollType.Normal;
                switch (die.DieType)
                {
                    case DieType.Normal:
                        rt = RollType.Normal;
                        break;
                    case DieType.Fudge:
                        rt = RollType.Fudge;
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported die type in reroll");
                }

                var reroll = RollNode.DoRoll(conf, rt, die.NumSides, DieFlags.Extra);
                while (rerolls < maxRerolls && Comparison.Compare(reroll.Value))
                {
                    _values.Add(new DieResult()
                    {
                        DieType = reroll.DieType,
                        NumSides = reroll.NumSides,
                        Value = reroll.Value,
                        Flags = reroll.Flags | DieFlags.Dropped
                    });

                    rolls++;
                    rerolls++;
                    reroll = RollNode.DoRoll(conf, rt, die.NumSides, DieFlags.Extra);
                }

                _values.Add(reroll);
            }

            Value = _values.Where(d => d.DieType != DieType.Special && !d.Flags.HasFlag(DieFlags.Dropped)).Sum(d => d.Value);

            return rolls;
        }
    }
}
