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

			if (numDice == 0)
			{
				// short-circuit if we aren't actually rolling any dice
				return 0;
			}

			byte[] roll = new byte[4];
			uint sides = (uint)numSides;
			DieType dt = DieType.Normal;

			switch (RollType)
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

			for (uint i = 0; i < numDice; i++)
			{
				do
				{
					_rand.GetBytes(roll);
				} while (!IsFairRoll(roll, sides));

				// rollAmt is a number from 0 to sides-1, need to convert to a proper number
				uint rollAmt = BitConverter.ToUInt32(roll, 0) % sides;

				switch (RollType)
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

				Value += rollAmt;
				_values.Add(new DieResult()
				{
					DieType = dt,
					NumSides = (uint)numSides,
					Value = rollAmt
				});
			}

			return (ulong)numDice;
		}

		/// <summary>
		/// Ensure that a roll lies within the allowed range of values.
		/// If a roll is too high, we could introduce bias into the result.
		/// </summary>
		/// <param name="roll"></param>
		/// <param name="numSides"></param>
		/// <returns></returns>
		private bool IsFairRoll(byte[] roll, uint numSides)
		{
			uint rollAmt = BitConverter.ToUInt32(roll, 0);
			uint numSets = UInt32.MaxValue / numSides;

			return rollAmt < numSets * numSides;
		}
	}
}
