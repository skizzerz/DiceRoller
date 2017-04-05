using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Dice
{
    /// <summary>
    /// Represents an error code that is thrown as part as an exception.
    /// The DiceException class exposes this error code, which can be used
    /// as a key in the calling application to customize error messages without
    /// needing to resort to string parsing.
    /// </summary>
    public enum DiceErrorCode
    {
        /// <summary>
        /// A negative number of dice cannot be rolled.
        /// </summary>
        [Description("A negative number of dice cannot be rolled.")]
        NegativeDice = 1,
        /// <summary>
        /// Dice must have between 1 and {0} sides.
        /// </summary>
        [Description("Dice must have between 1 and {0} sides.")]
        BadSides = 2,
        /// <summary>
        /// Only the following die sizes are valid: 2, 3, 4, 6, 8, 10, 12, 20, 100, 1000, 10000.
        /// </summary>
        [Description("Only the following die sizes are valid: 2, 3, 4, 6, 8, 10, 12, 20, 100, 1000, 10000.")]
        WrongSides = 3,
        /// <summary>
        /// Attempted to divide by 0.
        /// </summary>
        [Description("Attempted to divide by 0.")]
        DivideByZero = 4,
        /// <summary>
        /// Maximum recursion depth of {0} exceeded.
        /// </summary>
        [Description("Maximum recursion depth of {0} exceeded.")]
        RecursionDepthExceeded = 5,
        /// <summary>
        /// Cannot apply advantage/disadvantage and a keep/drop expression on the same roll.
        /// </summary>
        [Description("Cannot apply advantage/disadvantage and a keep/drop expression on the same roll.")]
        NoAdvantageKeep = 6,
        /// <summary>
        /// Cannot add more than one sort expression to a roll.
        /// </summary>
        [Description("Cannot add more than one sort expression to a roll.")]
        TooManySort = 7,
        /// <summary>
        /// Maximum number of dice {0} exceeded.
        /// </summary>
        [Description("Maximum number of dice {0} exceeded.")]
        TooManyDice = 8,
        /// <summary>
        /// No function named {0} is registered for the current scope.
        /// </summary>
        [Description("No function named {0} is registered for the current scope.")]
        NoSuchFunction = 9,
        /// <summary>
        /// Incorrect number of arguments passed to internal function {0}.
        /// </summary>
        [Description("Incorrect number of arguments passed to internal function {0}.")]
        IncorrectArity = 10,
    }
}
