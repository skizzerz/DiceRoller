using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dice.AST
{
    /// <summary>
    /// Represents rolling one or more dice of the same type.
    /// </summary>
    public class RollNode : DiceAST
    {
        private readonly List<DieResult> _values;
        private static readonly long[] _normalSides = new long[] { 1, 2, 3, 4, 6, 8, 10, 12, 20, 100, 1000, 10000 };
        private static readonly RandomNumberGenerator _rand = RandomNumberGenerator.Create();

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
        public DiceAST? NumSides { get; private set; }

        /// <summary>
        /// The results of each individual die rolled.
        /// </summary>
        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollNode"/> class.
        /// </summary>
        /// <param name="rollType">Type of roll being made.</param>
        /// <param name="numDice">Number of dice in the roll.</param>
        /// <param name="numSides">Number of sides per die.</param>
        internal RollNode(RollType rollType, DiceAST numDice, DiceAST? numSides)
        {
            RollType = rollType;
            _values = new List<DieResult>();
            NumDice = numDice ?? throw new ArgumentNullException(nameof(numDice));
            NumSides = numSides;

            if (numSides == null && rollType != RollType.Fudge)
            {
                throw new ArgumentNullException(nameof(numSides));
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(NumDice.Value);

            sb.Append('d');

            if (RollType == RollType.Fudge)
            {
                sb.Append('F');
            }

            if (NumSides != null)
            {
                sb.Append(NumSides.Value);
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override long EvaluateInternal(RollData data, DiceAST root, int depth)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            long rolls = NumDice.Evaluate(data, root, depth + 1);
            rolls += NumSides?.Evaluate(data, root, depth + 1) ?? 0;
            rolls += Roll(data);

            return rolls;
        }

        /// <inheritdoc/>
        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            return Roll(data);
        }

        private long Roll(RollData data)
        {
            long numDice = (long)NumDice.Value;
            long numSides = (long)(NumSides?.Value ?? 1);

            if (numDice < 0)
            {
                throw new DiceException(DiceErrorCode.NegativeDice);
            }

            if (numDice > data.Config.MaxDice)
            {
                throw new DiceException(DiceErrorCode.TooManyDice, data.Config.MaxDice);
            }

            _values.Clear();
            ValueType = ResultType.Total;

            if (numDice == 0)
            {
                Value = 0;
                _values.Add(new DieResult()
                {
                    DieType = DieType.Literal,
                    NumSides = 0,
                    Value = 0,
                    Flags = DieFlags.Extra
                });

                return 0;
            }

            for (int i = 0; i < numDice; i++)
            {
                var result = DoRoll(data, RollType, (int)numSides);

                _values.MaybeAddPlus();
                _values.Add(result);
            }

            Value = _values.Sum(d => d.Value);
            data.InternalContext.AddRollExpression(this);

            return numDice;
        }

        /// <summary>
        /// Performs a single die roll.
        /// </summary>
        /// <param name="data">Roll configuration and related data.</param>
        /// <param name="rollType">Type of roll to make.</param>
        /// <param name="numSides">Number of sides of the die.</param>
        /// <param name="flags">Flags to add to the die.</param>
        /// <returns>
        /// Returns a DieResult containing the result of the roll. It will be marked with the passed-in
        /// flags as well as the Critical flag if the die rolled maximium and the Fumble flag if it rolled minimum.
        /// </returns>
        internal static DieResult DoRoll(RollData data, RollType rollType, int numSides, DieFlags flags = 0)
        {
            if (numSides < 1 || numSides > data.Config.MaxSides)
            {
                throw new DiceException(DiceErrorCode.BadSides, data.Config.MaxSides);
            }

            if (data.Config.NormalSidesOnly && !_normalSides.Contains(numSides))
            {
                throw new DiceException(DiceErrorCode.WrongSides);
            }

            // use >= as the comparison because we will be adding 1 roll later on,
            // so if we're already at the limit that later roll won't be legal
            if (data.InternalContext.AllRolls.Count >= data.Config.MaxDice)
            {
                throw new DiceException(DiceErrorCode.TooManyDice, data.Config.MaxDice);
            }

            byte[] roll = new byte[4];
            uint sides = (uint)numSides;
            int min, max;
            uint rollValue;
            int rollAmt;
            DieType dt;

            switch (rollType)
            {
                case RollType.Normal:
                    dt = DieType.Normal;
                    min = 1;
                    max = numSides;
                    break;
                case RollType.Fudge:
                    dt = DieType.Fudge;
                    // fudge dice go from -sides to sides, so we need to double
                    // numSides and include an extra side for a "0" value as well.
                    sides = (sides * 2) + 1;
                    min = -numSides;
                    max = numSides;
                    break;
                default:
                    throw new InvalidOperationException("Unknown RollType");
            }

            if (data.Config.RollDie != null)
            {
                rollAmt = data.Config.RollDie(min, max);
                if (rollAmt < min || rollAmt > max)
                {
                    throw new InvalidOperationException("RollerConfig.RollDie returned a value not within the expected range.");
                }

                // convert rollValue into the 0-based number for serialization
                rollValue = rollType switch
                {
                    RollType.Normal => (uint)(rollAmt - 1),
                    RollType.Fudge => (uint)(rollAmt + numSides),
                    _ => throw new InvalidOperationException("Unknown RollType"),
                };
            }
            else
            {
                do
                {
                    if (data.Config.GetRandomBytes != null)
                    {
                        data.Config.GetRandomBytes(roll);
                    }
                    else
                    {
                        _rand.GetBytes(roll);
                    }
                } while (!IsFairRoll(roll, sides));

                // rollAmt is a number from 0 to sides-1, need to convert to a proper number
                rollValue = BitConverter.ToUInt32(roll, 0) % sides;
                rollAmt = (int)rollValue;

                switch (rollType)
                {
                    case RollType.Normal:
                        // change from 0 to sides-1 into 1 to sides
                        rollAmt++;
                        break;
                    case RollType.Fudge:
                        // normalize back into -numSides to +numSides
                        rollAmt -= ((int)sides - 1) / 2;
                        break;
                    default:
                        throw new InvalidOperationException("Unknown RollType");
                }
            }

            data.InternalContext.AllRolls.Add(rollValue);

            // finally, mark if this was a critical or fumble. This may be overridden later by a CritNode.
            if (rollAmt == min)
            {
                flags |= DieFlags.Fumble;
            }

            if (rollAmt == max)
            {
                flags |= DieFlags.Critical;
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
        /// <param name="roll">Output of the RNG, should be an array of 4 bytes.</param>
        /// <param name="numSides">Number of sides of the die.</param>
        /// <returns>Returns true if using the random number for this roll would not introduce bias, false otherwise.</returns>
        private static bool IsFairRoll(byte[] roll, uint numSides)
        {
            uint rollAmt = BitConverter.ToUInt32(roll, 0);
            uint numSets = UInt32.MaxValue / numSides;

            return rollAmt < numSets * numSides;
        }
    }
}
