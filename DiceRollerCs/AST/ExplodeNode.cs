using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dice.Exceptions;

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
        public DiceAST Expression { get; private set; }

        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        internal ExplodeNode(ExplodeType explodeType, bool compound, ComparisonNode comparison, DiceAST expression)
        {
            ExplodeType = explodeType;
            Compound = compound;
            Comparison = comparison;
            Expression = expression ?? throw new ArgumentNullException("expression");
            _values = new List<DieResult>();
        }

        protected override ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = Comparison?.Evaluate(conf, root, depth + 1) ?? 0;
            rolls += Expression.Evaluate(conf, root, depth + 1);
            rolls += DoExplode(conf, root, depth);

            return rolls;
        }

        protected override ulong RerollInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = 0;
            if (Comparison?.Evaluated == false)
            {
                rolls += Comparison.Evaluate(conf, root, depth + 1);
            }

            rolls += Expression.Reroll(conf, root, depth + 1);
            rolls += DoExplode(conf, root, depth);

            return rolls;
        }

        private ulong DoExplode(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = 0;
            Func<DieResult, bool> shouldExplode;
            Value = 0;
            _values.Clear();

            if (Comparison != null)
            {
                shouldExplode = d => Comparison.Compare(d.Value);
            }
            else
            {
                shouldExplode = d => d.Value == d.NumSides;
            }


            foreach (var die in Expression.Values)
            {
                var accum = die;

                if (die.DieType == DieType.Group || die.DieType == DieType.Special || die.Flags.HasFlag(DieFlags.Dropped))
                {
                    // grouped die results can't explode, as we don't know what to explode them to
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
                    default:
                        throw new InvalidOperationException("Unsupported die type for explosion");
                }

                if (shouldExplode(die))
                {
                    if (!Compound)
                    {
                        _values.Add(die);
                    }

                    DieResult result;

                    do
                    {
                        rolls++;

                        if (rolls > conf.MaxDice)
                        {
                            throw new TooManyDiceException(conf.MaxDice);
                        }

                        if (rolls > conf.MaxRerolls)
                        {
                            break;
                        }

                        var numSides = die.NumSides;
                        if (ExplodeType == ExplodeType.Penetrate)
                        {
                            // if penetrating dice are used, d100p penetrates to d20p,
                            // and d20p penetrates to d6p (however, the d20p from
                            // the d100p does not further drop to d6p).
                            if (numSides == 100)
                            {
                                numSides = 20;
                            }
                            else if (numSides == 20)
                            {
                                numSides = 6;
                            }
                        }

                        result = RollNode.DoRoll(conf, rt, numSides, DieFlags.Extra);
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

                        if (Compound)
                        {
                            accum.Value += result.Value;
                        }
                        else
                        {
                            _values.Add(result);
                        }
                    } while (shouldExplode(result));

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
