using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// Contains the result of an individual die roll.
    /// </summary>
    public struct DieResult
    {
        /// <summary>
        /// What type of die was rolled
        /// </summary>
        public DieType DieType;
        /// <summary>
        /// How many sides the die had
        /// </summary>
        public uint NumSides;
        /// <summary>
        /// What the result of the roll was
        /// </summary>
        public decimal Value;
    }
}
