using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Dice.AST
{
    /// <summary>
    /// Represents dice that reroll when certain conditions are met
    /// </summary>
    public class ExplodeNode : DiceAST
    {
        private List<DieResult> _values;

        /// <summary>
        /// What type of exploding dice we have
        /// </summary>
        public ExplodeType ExplodeType { get; private set; }

        /// <summary>
        /// If true, exploding die results are combined into a single DieResult.
        /// If false, each exploding die is its own DieResult.
        /// </summary>
        public bool Compound { get; private set; }

        /// <summary>
        /// The expression to compare against to determine whether or not to explode.
        /// If null, dice explode when they roll their maximum result.
        /// </summary>
        public ComparisonNode Comparison { get; private set; }

        /// <summary>
        /// The dice being rolled.
        /// </summary>
        public DiceAST Expression { get; internal set; }

        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        protected internal override DiceAST UnderlyingRollNode => Expression.UnderlyingRollNode;

        internal ExplodeNode(ExplodeType explodeType, bool compound, ComparisonNode comparison)
        {
            ExplodeType = explodeType;
            Compound = compound;
            Comparison = comparison;
            Expression = null;
            _values = new List<DieResult>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Expression?.ToString() ?? String.Empty);

            switch (ExplodeType)
            {
                case ExplodeType.Explode:
                    if (Compound)
                    {
                        sb.Append(".compound");
                    }
                    else
                    {
                        sb.Append(".explode");
                    }
                    break;
                case ExplodeType.Penetrate:
                    if (Compound)
                    {
                        sb.Append(".compoundPenetrate");
                    }
                    else
                    {
                        sb.Append(".penetrate");
                    }
                    break;
            }

            sb.AppendFormat("({0})", Comparison?.ToString() ?? String.Empty);

            return sb.ToString();
        }

        internal void AddComparison(ComparisonNode comp)
        {
            if (Comparison == null)
            {
                throw new DiceException(DiceErrorCode.MixedExplodeComp);
            }

            Comparison.Add(comp ?? throw new ArgumentNullException("comp"));
        }

        protected override long EvaluateInternal(RollData data, DiceAST root, int depth)
        {
            long rolls = Comparison?.Evaluate(data, root, depth + 1) ?? 0;
            rolls += Expression.Evaluate(data, root, depth + 1);
            rolls += DoExplode(data);

            return rolls;
        }

        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            long rolls = Expression.Reroll(data, root, depth + 1);
            rolls += DoExplode(data);

            return rolls;
        }

        private long DoExplode(RollData data)
        {
            long rolls = 0;
            Func<DieResult, decimal, bool> shouldExplode;
            decimal addToValue = ExplodeType == ExplodeType.Penetrate ? 1 : 0;

            Value = 0;
            ValueType = ResultType.Total;
            _values.Clear();

            if (Comparison != null)
            {
                shouldExplode = (d, x) => Comparison.Compare(d.Value + x);
            }
            else
            {
                shouldExplode = (d, x) => d.Value + x == d.NumSides;
            }


            foreach (var die in Expression.Values)
            {
                var accum = die;

                if (!die.IsLiveDie())
                {
                    // special die results can't explode as they aren't actually dice
                    // dropped dice are no longer part of the resultant expression so should not explode
                    _values.Add(die);

                    continue;
                }

                RollType rt;
                switch (die.DieType)
                {
                    case DieType.Normal:
                        rt = RollType.Normal;
                        break;
                    case DieType.Fudge:
                        rt = RollType.Fudge;
                        break;
                    case DieType.Group: // we can't explode on groups, so throw an exception
                    default:
                        throw new InvalidOperationException("Unsupported die type for explosion");
                }

                Value += die.Value;
                if (shouldExplode(die, 0))
                {
                    if (!Compound)
                    {
                        _values.Add(die);
                    }

                    DieResult result;

                    do
                    {
                        rolls++;

                        if (rolls > data.Config.MaxDice)
                        {
                            throw new DiceException(DiceErrorCode.TooManyDice, data.Config.MaxDice);
                        }

                        if (rolls > data.Config.MaxRerolls)
                        {
                            break;
                        }

                        var numSides = die.NumSides;
                        if (ExplodeType == ExplodeType.Penetrate && Comparison == null)
                        {
                            // if penetrating dice are used, d100p penetrates to d20p,
                            // and d20p penetrates to d6p (however, the d20p from
                            // the d100p does not further drop to d6p).
                            // only do this if a custom comparison expression was not used.
                            if (numSides == 100)
                            {
                                numSides = 20;
                            }
                            else if (numSides == 20)
                            {
                                numSides = 6;
                            }
                        }

                        result = RollNode.DoRoll(data, rt, numSides, DieFlags.Extra);
                        switch (ExplodeType)
                        {
                            case ExplodeType.Explode:
                                break;
                            case ExplodeType.Penetrate:
                                result.Value -= 1;
                                break;
                            default:
                                throw new InvalidOperationException("Unknown explosion type");
                        }

                        Value += result.Value;
                        if (Compound)
                        {
                            accum.Value += result.Value;
                        }
                        else
                        {
                            _values.Add(new DieResult(SpecialDie.Add));
                            _values.Add(result);
                        }
                    } while (shouldExplode(result, addToValue));

                    if (Compound)
                    {
                        _values.Add(accum);
                    }
                }
                else
                {
                    _values.Add(die);
                }
            }

            return rolls;
        }
    }
}
