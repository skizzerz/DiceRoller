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

        protected internal override DiceAST UnderlyingRollNode => Expression.UnderlyingRollNode;

        internal RerollNode(int maxRerolls, ComparisonNode comparison, DiceAST maxRerollsExpr = null)
        {
            Comparison = comparison;
            Expression = null;
            MaxRerolls = maxRerolls;
            MaxRerollsExpr = maxRerollsExpr;
            _values = new List<DieResult>();

            if (maxRerolls < 0 && maxRerollsExpr == null)
            {
                throw new DiceException(DiceErrorCode.BadRerollCount);
            }
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

        protected override long EvaluateInternal(RollData data, DiceAST root, int depth)
        {
            var rolls = MaxRerollsExpr?.Evaluate(data, root, depth + 1) ?? 0;
            rolls += Comparison.Evaluate(data, root, depth + 1);
            rolls += Expression.Evaluate(data, root, depth + 1);
            rolls += MaybeReroll(data, root, depth);

            return rolls;
        }

        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            var rolls = Expression.Reroll(data, root, depth + 1);
            rolls += MaybeReroll(data, root, depth);

            return rolls;
        }

        private long MaybeReroll(RollData data, DiceAST root, int depth)
        {
            long rolls = 0;
            int rerolls = 0;
            int maxRerolls = MaxRerolls;
            if (MaxRerolls < 0)
            {
                if (MaxRerollsExpr.Value < 0 || Math.Floor(MaxRerollsExpr.Value) != MaxRerollsExpr.Value || MaxRerollsExpr.Value > Int32.MaxValue)
                {
                    throw new DiceException(DiceErrorCode.BadRerollCount);
                }

                maxRerolls = (int)MaxRerollsExpr.Value;
            }
            maxRerolls = maxRerolls == 0 ? data.Config.MaxRerolls : Math.Min(maxRerolls, data.Config.MaxRerolls);
            _values.Clear();

            void DoReroll(DieResult die, out DieResult reroll)
            {
                rerolls++;

                if (die.DieType == DieType.Group)
                {
                    var group = data.InternalContext.GetGroupExpression(die.Data);
                    rolls += group.Reroll(data, root, depth + 1);

                    reroll = new DieResult()
                    {
                        DieType = DieType.Group,
                        NumSides = 0,
                        Value = group.Value,
                        // maintain any crit/fumble flags from the underlying dice, combining them together
                        Flags = group.Values
                                .Where(d => d.DieType != DieType.Special && !d.Flags.HasFlag(DieFlags.Dropped))
                                .Select(d => d.Flags & (DieFlags.Critical | DieFlags.Fumble))
                                .Aggregate((d1, d2) => d1 | d2) | DieFlags.Extra,
                        Data = die.Data
                    };
                }
                else
                {
                    rolls++;
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

                    reroll = RollNode.DoRoll(data, rt, die.NumSides, DieFlags.Extra);
                }
            }

            foreach (var die in Expression.Values)
            {
                if (die.DieType == DieType.Special || die.Flags.HasFlag(DieFlags.Dropped) || !Comparison.Compare(die.Value))
                {
                    _values.Add(die);
                    continue;
                }

                _values.Add(die.Drop());
                DoReroll(die, out DieResult rr);

                while (rerolls < maxRerolls && Comparison.Compare(rr.Value))
                {
                    _values.Add(new DieResult(SpecialDie.Add));
                    _values.Add(rr.Drop()); // mark the overall result as dropped
                    DoReroll(die, out rr);
                }

                _values.Add(new DieResult(SpecialDie.Add));
                _values.Add(rr);
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
