using System;
using System.Text;
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
        public int MaxRerolls { get; private set; }

        /// <summary>
        /// If this node was generated via the rerollN() function, this contains the expression used for N.
        /// Otherwise, this is null.
        /// </summary>
        public DiceAST MaxRerollsExpr { get; private set; }

        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        internal RerollNode(int maxRerolls, ComparisonNode comparison, DiceAST maxRerollsExpr = null)
        {
            Comparison = comparison ?? throw new ArgumentNullException("comparison");
            Expression = null;
            MaxRerolls = maxRerolls;
            MaxRerollsExpr = maxRerollsExpr;
            _values = new List<DieResult>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Expression?.ToString() ?? String.Empty);

            if (MaxRerollsExpr != null)
            {
                sb.AppendFormat(".rerollN({0}, ", MaxRerollsExpr.ToString());
            }
            else if (MaxRerolls == 1)
            {
                sb.Append(".rerollOnce(");
            }
            else if (MaxRerolls == 0)
            {
                sb.Append(".reroll(");
            }
            else
            {
                // MaxRerolls was not 0 or 1 and MaxRerollsExpr is null, this should never happen.
                sb.Append(".<<UNKNOWN REROLL>>(");
            }

            sb.Append(Comparison.ToString());
            sb.Append(")");

            return sb.ToString();
        }

        protected override long EvaluateInternal(RollerConfig conf, DiceAST root, int depth)
        {
            var rolls = Comparison.Evaluate(conf, root, depth + 1);
            rolls += Expression.Evaluate(conf, root, depth + 1);
            rolls += MaybeReroll(conf);

            return rolls;
        }

        protected override long RerollInternal(RollerConfig conf, DiceAST root, int depth)
        {
            var rolls = Expression.Reroll(conf, root, depth + 1);
            rolls += MaybeReroll(conf);

            return rolls;
        }

        private long MaybeReroll(RollerConfig conf)
        {
            long rolls = 0;
            int rerolls = 0;
            var maxRerolls = MaxRerolls == 0 ? conf.MaxRerolls : Math.Min(MaxRerolls, conf.MaxRerolls);
            _values.Clear();

            foreach (var die in Expression.Values)
            {
                if (die.DieType == DieType.Group || die.DieType == DieType.Special || die.Flags.HasFlag(DieFlags.Dropped) || !Comparison.Compare(die.Value))
                {
                    _values.Add(die);
                    continue;
                }

                _values.Add(die.Drop());

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
                    _values.Add(new DieResult(SpecialDie.Add));
                    _values.Add(reroll.Drop());

                    rolls++;
                    rerolls++;
                    reroll = RollNode.DoRoll(conf, rt, die.NumSides, DieFlags.Extra);
                }

                _values.Add(new DieResult(SpecialDie.Add));
                _values.Add(reroll);
            }

            var dice = _values.Where(d => d.DieType != DieType.Special && !d.Flags.HasFlag(DieFlags.Dropped));

            if (Expression.ValueType == ResultType.Total)
            {
                Value = dice.Sum(d => d.Value);
                ValueType = ResultType.Total;
            }
            else
            {
                Value = dice.Sum(d => d.SuccessCount);
                ValueType = ResultType.Successes;
            }

            return rolls;
        }
    }
}
