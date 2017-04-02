using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

using Dice.Exceptions;

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
            NumSides = numSides ?? throw new ArgumentNullException("numSides");
        }

        protected override ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = NumDice.Evaluate(conf, root, depth + 1);
            rolls += NumSides.Evaluate(conf, root, depth + 1);
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
            var numSides = (long)NumSides.Value;

            if (numDice < 0)
            {
                throw new BadDiceException();
            }

            if (numSides < 1 || numSides > conf.MaxSides)
            {
                throw new BadSidesException(conf.MaxSides);
            }

            if (conf.NormalSidesOnly && !_normalSides.Contains(numSides))
            {
                throw new BadSidesException();
            }

            Value = 0;
            _values.Clear();

            for (uint i = 0; i < numDice; i++)
            {
                var result = DoRoll(conf, RollType, (uint)numSides);

                Value += result.Value;
                _values.Add(result);
            }

            return (ulong)numDice;
        }

        internal static DieResult DoRoll(RollerConfig conf, RollType rollType, uint numSides)
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
                Value = rollAmt
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
