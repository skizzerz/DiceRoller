using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice
{
    /// <summary>
    /// What sort of thing a RollResult represents.
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// The RollResult represents the total of all faces rolled on the dice.
        /// </summary>
        Total,

        /// <summary>
        /// The RollResult represents the number of successes rolled in a pool of dice.
        /// </summary>
        Successes
    }
}
