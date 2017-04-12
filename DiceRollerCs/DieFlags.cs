using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice
{
    /// <summary>
    /// Information about a particular die, such as whether it is a critical
    /// or fumble, or whether or not the die result was kept.
    /// </summary>
    [Flags]
    public enum DieFlags
    {
        /// <summary>
        /// Indicates the die was a critical, meant for display purposes
        /// </summary>
        Critical = 1,
        /// <summary>
        /// Indicates the die was a fumble, meant for display purposes
        /// </summary>
        Fumble = 2,
        /// <summary>
        /// Indicates the die counted as a success
        /// </summary>
        Success = 4,
        /// <summary>
        /// Indicates the die counted as a failure
        /// </summary>
        Failure = 8,
        /// <summary>
        /// Indicates the die result was not kept (dropped)
        /// </summary>
        Dropped = 16,
        /// <summary>
        /// Indicates the die was added due to an explosion or reroll,
        /// and was not one of the original dice rolled
        /// </summary>
        Extra = 32,
        /// <summary>
        /// Indicates the die was generated as part of a macro.
        /// </summary>
        Macro = 64
    }
}
