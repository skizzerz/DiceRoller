using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Dice.AST
{
    public class RollNode : DiceAST
    {
        private List<DieResult> _values;
        private readonly long[] _normalSides = new long[] { 2, 3, 4, 6, 8, 10, 12, 20, 100, 1000, 10000 };
        private static RNGCryptoServiceProvider _rand = new RNGCryptoServiceProvider();

        /// <summary>
        /// What sort of roll is being made.
        /// </summary>
        public RollType RollType { get; private set; }
        /// <summary>
        /// How many dice are being rolled. Decimals are truncated,
        /// and negative numbers throw a BadDiceException.
        /// </summary>
        public DiceAST NumDice { get; private set; }
        /// <summary>
        /// How many sides does each die have? Decimals are truncated.
        /// Invalid numbers (according to roller config) throw a BadSidesException.
        /// May be null in the case of standard fudge dice.
        /// </summary>
        public DiceAST NumSides { get; private set; }
        /// <summary>
        /// The results of each individual die rolled
        /// </summary>
        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        internal RollNode(RollType rollType, DiceAST numDice, DiceAST numSides)
        {
            RollType = rollType;
            _values = new List<DieResult>();
            NumDice = numDice ?? throw new ArgumentNullException("numDice");
            NumSides = numSides;

            if (numSides == null && rollType != RollType.Fudge)
            {
                throw new ArgumentNullException("numSides");
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (NumDice is LiteralNode || NumDice is MacroNode)
            {
                sb.Append(NumDice.ToString());
            }
            else
            {
                sb.AppendFormat("({0})", NumDice.ToString());
            }

            sb.Append("d");

            if (RollType == RollType.Fudge)
            {
                sb.Append("F");
            }

            if (NumSides != null)
            {
                if (NumSides is LiteralNode || NumSides is MacroNode)
                {
                    sb.Append(NumSides.ToString());
                }
                else
                {
                    sb.AppendFormat("({0})", NumSides.ToString());
                }
            }

            return sb.ToString();
        }

        protected override ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = NumDice.Evaluate(conf, root, depth + 1);
            rolls += NumSides?.Evaluate(conf, root, depth + 1) ?? 0;
            rolls += Roll(conf, root, depth, false);

            return rolls;
        }

        protected override ulong RerollInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            return Roll(conf, root, depth, true);
        }

        private ulong Roll(RollerConfig conf, DiceAST root, uint depth, bool reroll)
        {
            var numDice = (long)NumDice.Value;
            var numSides = (long)(NumSides?.Value ?? 1);

            if (numDice < 0)
            {
                throw new DiceException(DiceErrorCode.NegativeDice);
            }

            if (numSides < 1 || numSides > conf.MaxSides)
            {
                throw new DiceException(DiceErrorCode.BadSides, conf.MaxSides);
            }

            if (conf.NormalSidesOnly && !_normalSides.Contains(numSides))
            {
                throw new DiceException(DiceErrorCode.WrongSides);
            }

            _values.Clear();

            for (uint i = 0; i < numDice; i++)
            {
                var result = DoRoll(conf, RollType, (uint)numSides);

                if (i > 0)
                {
                    _values.Add(new DieResult()
                    {
                        DieType = DieType.Special,
                        NumSides = 0,
                        Value = (decimal)SpecialDie.Add,
                        Flags = 0
                    });
                }
                _values.Add(result);
            }

            Value = _values.Sum(d => d.Value);

            return (ulong)numDice;
        }

        internal static DieResult DoRoll(RollerConfig conf, RollType rollType, uint numSides, DieFlags flags = 0)
        {
            byte[] roll = new byte[4];
            uint sides = numSides;
            DieType dt;

            switch (rollType)
            {
                case RollType.Normal:
                    dt = DieType.Normal;
                    break;
                case RollType.Fudge:
                    dt = DieType.Fudge;
                    // fudge dice go from -sides to sides, so we need to double
                    // numSides and include an extra side for a "0" value as well.
                    sides = (sides * 2) + 1;
                    break;
                default:
                    throw new InvalidOperationException("Unknown RollType");
            }

            do
            {
                if (conf.GetRandomBytes != null)
                {
                    conf.GetRandomBytes(roll);
                }
                else
                {
                    _rand.GetBytes(roll);
                }
            } while (!IsFairRoll(roll, sides));

            // rollAmt is a number from 0 to sides-1, need to convert to a proper number
            uint rollAmt = BitConverter.ToUInt32(roll, 0) % sides;

            // first, mark if this was a critical or fumble. This may be overridden later by a CritNode.
            if (rollAmt == 0)
            {
                flags |= DieFlags.Fumble;
            }

            if (rollAmt == sides - 1)
            {
                flags |= DieFlags.Critical;
            }

            switch (rollType)
            {
                case RollType.Normal:
                    // change from 0 to sides-1 into 1 to sides
                    rollAmt++;
                    break;
                case RollType.Fudge:
                    // normalize back into -numSides to +numSides
                    rollAmt -= (sides - 1) / 2;
                    break;
            }

            return new DieResult()
            {
                DieType = dt,
                NumSides = numSides,
                Value = rollAmt,
                Flags = flags
            };
        }

        /// <summary>
        /// Ensure that a roll lies within the allowed range of values.
        /// If a roll is too high, we could introduce bias into the result.
        /// </summary>
        /// <param name="roll"></param>
        /// <param name="numSides"></param>
        /// <returns></returns>
        private static bool IsFairRoll(byte[] roll, uint numSides)
        {
            uint rollAmt = BitConverter.ToUInt32(roll, 0);
            uint numSets = UInt32.MaxValue / numSides;

            return rollAmt < numSets * numSides;
        }
    }
}
