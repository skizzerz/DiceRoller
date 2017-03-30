using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice
{
    /// <summary>
    /// Configuration for a Roller, controlling the maximum number of
    /// dice, sides, and nesting depth, as well as which grammar is used.
    /// </summary>
    public class RollerConfig
    {
        /// <summary>
        /// The maximum number of dice that may be rolled, including dice
        /// rolled due to rerolls or exploding dice. Exceeding this limit
        /// will result in a TooManyDiceException being thrown.
        /// Default: 100
        /// </summary>
        public uint MaxDice { get; set; }

        /// <summary>
        /// The maximum number of sides that any individual die can have.
        /// Exceeding this limit will result in a BadSidesException being thrown.
        /// Default: 10,000
        /// </summary>
        public uint MaxSides { get; set; }

        /// <summary>
        /// The maximum amount of nesting that can happen in dice expressions.
        /// Certain dice constructs use multiple levels. Exceeding this limit
        /// will result in a DiceRecursionException being thrown.
        /// Default: 20
        /// </summary>
        public ushort MaxRecursionDepth { get; set; }

        /// <summary>
        /// If true, only standard dice sizes (d2, d3, d4, d6, d8, d10, d12, d20, d100, d1,000, d10,000)
        /// may be rolled. If false, any die size from 1 to MaxSides, inclusive, can be rolled.
        /// Default: false
        /// </summary>
        public bool NormalSidesOnly { get; set; }

        public RollerConfig()
        {
            MaxDice = 100;
            MaxSides = 10000;
            MaxRecursionDepth = 20;
            NormalSidesOnly = false;
        }
    }
}
