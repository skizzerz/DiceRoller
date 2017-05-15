using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice
{
    /// <summary>
    /// Represents where in the AST a function node is inserted.
    /// If multiple functions are registered with the same timing, they are
    /// executed in the order they are specified in the dice expression.
    /// </summary>
    // Note: the ordering below is the order in which these are actually executed
    public enum FunctionTiming
    {
        /// <summary>
        /// The function should be executed before any other expressions
        /// </summary>
        First,
        BeforeExplode,
        AfterExplode,
        BeforeReroll,
        AfterReroll,
        BeforeKeep,
        AfterKeep,
        BeforeSuccess,
        AfterSuccess,
        BeforeCrit,
        AfterCrit,
        BeforeSort,
        AfterSort,
        /// <summary>
        /// The function should be executed after all other expressions
        /// </summary>
        Last
    }
}
