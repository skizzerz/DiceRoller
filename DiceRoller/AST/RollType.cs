using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// How rolled values are calculated. See also the DieType enum.
    /// </summary>
    public enum RollType
    {
        /// <summary>
        /// The die range is from 1 to the number of sides.
        /// </summary>
        Normal,

        /// <summary>
        /// The die range is from -N to N, where N is the number of sides.
        /// </summary>
        Fudge
    }
}
