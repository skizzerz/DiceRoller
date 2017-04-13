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
