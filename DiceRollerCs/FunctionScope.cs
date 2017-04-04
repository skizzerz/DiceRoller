using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice
{
    /// <summary>
    /// Represents what scope a function call resides in.
    /// </summary>
    public enum FunctionScope
    {
        /// <summary>
        /// The function is a global function (not attached to a die roll)
        /// </summary>
        Global,
        /// <summary>
        /// The function is attached to a normal or fudge die roll
        /// </summary>
        Basic,
        /// <summary>
        /// The function is attached to a grouped roll
        /// </summary>
        Group
    }
}
