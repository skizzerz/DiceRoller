using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice
{
    /// <summary>
    /// Represents what scope a function call resides in. The All and Roll scopes
    /// are used when registering functions, and are not used when evaluating them.
    /// </summary>
    public enum FunctionScope
    {
        /// <summary>
        /// The function is a global function (not attached to a die roll)
        /// </summary>
        Global,
        /// <summary>
        /// The function is attached to normal, fudge, or grouped rolls, useful
        /// when registering functions that work on multiple roll types.
        /// </summary>
        Roll,
        /// <summary>
        /// The function is attached to a normal or fudge die roll
        /// </summary>
        Basic,
        /// <summary>
        /// The function is attached to a grouped roll
        /// </summary>
        Group,
        /// <summary>
        /// The function can be executed globally as well as attached to any sort
        /// of roll. Useful when registering functions that work everywhere.
        /// </summary>
        All
    }
}
