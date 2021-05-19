using System;
using System.Collections.Generic;
using System.Text;

namespace Dice
{
    /// <summary>
    /// Specifies how multiples of the same function behave when added to a roll.
    /// Behavior is ignored for global functions.
    /// </summary>
    public enum FunctionBehavior
    {
        /// <summary>
        /// Execute each function sequentially in the order defined in the roll.
        /// This is the default.
        /// </summary>
        ExecuteSequentially,
        /// <summary>
        /// Combine the arguments of all function calls into a single function call.
        /// </summary>
        CombineArguments
    }
}
