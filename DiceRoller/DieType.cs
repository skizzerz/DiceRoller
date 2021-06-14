using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice
{
    /// <summary>
    /// How a die result was calculated. See also the RollType enum.
    /// </summary>
    public enum DieType
    {
        /// <summary>
        /// Die represents a roll from 1 to N, where N is the number of sides.
        /// </summary>
        Normal,

        /// <summary>
        /// Die represents a roll from -N to N, where N is the number of sides.
        /// </summary>
        Fudge,

        /// <summary>
        /// Die represents a grouped roll. The number of sides is meaningless for this die type.
        /// </summary>
        Group,

        /// <summary>
        /// Die represents a literal number. The number of sides is meaningless for this die type.
        /// </summary>
        Literal,

        /// <summary>
        /// Die is not actually a roll at all, but rather a special marker. The Value
        /// can be cast into a SpecialDie enum to determine what kind of special marker it is.
        /// </summary>
        Special
    }
}
