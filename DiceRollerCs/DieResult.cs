using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Dice
{
    /// <summary>
    /// Contains the result of an individual die roll.
    /// </summary>
    public struct DieResult
    {
        /// <summary>
        /// What type of die was rolled
        /// </summary>
        public DieType DieType { get; set; }

        /// <summary>
        /// How many sides the die had
        /// </summary>
        public int NumSides { get; set; }

        /// <summary>
        /// What the result of the roll was
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Any special flags giving more information about the roll
        /// </summary>
        public DieFlags Flags { get; set; }

        /// <summary>
        /// What type of special die this is. Calling this is an error if DieType != DieType.SpecialDie
        /// </summary>
        public SpecialDie SpecialDie
        {
            get { return (SpecialDie)Value; }
            set { Value = (decimal)value; }
        }

        public DieResult(SpecialDie specialDie)
            : this()
        {
            DieType = DieType.Special;
            SpecialDie = specialDie;
        }

        /// <summary>
        /// Mark a die as a success
        /// </summary>
        /// <param name="die"></param>
        /// <returns></returns>
        public DieResult Success()
        {
            return new DieResult()
            {
                DieType = DieType,
                NumSides = NumSides,
                Value = Value,
                Flags = (Flags & ~DieFlags.Failure) | DieFlags.Success
            };
        }

        /// <summary>
        /// Mark a die as a failure
        /// </summary>
        /// <param name="die"></param>
        /// <returns></returns>
        public DieResult Failure()
        {
            return new DieResult()
            {
                DieType = DieType,
                NumSides = NumSides,
                Value = Value,
                Flags = (Flags & ~DieFlags.Success) | DieFlags.Failure
            };
        }

        /// <summary>
        /// Mark a die as dropped. We also strip success/failure info when dropping dice
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public DieResult Drop()
        {
            return new DieResult()
            {
                DieType = DieType,
                NumSides = NumSides,
                Value = Value,
                Flags = (Flags & ~(DieFlags.Success | DieFlags.Failure)) | DieFlags.Dropped
            };
        }

        /// <summary>
        /// Checks if this die is a special die of the specified type
        /// </summary>
        /// <param name="specialDie"></param>
        /// <returns></returns>
        public bool IsSpecialDie(SpecialDie specialDie)
        {
            return DieType == DieType.Special && SpecialDie == specialDie;
        }

        /// <summary>
        /// Checks if the die is not a special die and has not been dropped
        /// </summary>
        /// <returns></returns>
        public bool IsLiveDie()
        {
            return DieType != DieType.Special && !Flags.HasFlag(DieFlags.Dropped);
        }

        public override bool Equals(object obj)
        {
            if (obj is DieResult d)
            {
                return DieType == d.DieType && NumSides == d.NumSides && Value == d.Value && Flags == d.Flags;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return new { DieType, NumSides, Value, Flags }.GetHashCode();
        }

        public static bool operator ==(DieResult a, DieResult b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(DieResult a, DieResult b)
        {
            return !a.Equals(b);
        }
    }
}
