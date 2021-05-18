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
    /// <remarks>
    /// The following changes to this enum are not considered breaking:
    /// - Adding new items, including in the middle of the enum
    /// The following changes to this enum are considered breaking:
    /// - Removing items
    /// - Rearranging items
    /// </remarks>
    public enum FunctionTiming
    {
        /// <summary>
        /// The function should be executed before any other expressions
        /// </summary>
        First,
        BeforeExplode,
        Explode,
        AfterExplode,
        BeforeReroll,
        Reroll,
        AfterReroll,
        BeforeKeep,
        Keep,
        AfterKeep,
        BeforeSuccess,
        Success,
        AfterSuccess,
        BeforeCrit,
        Crit,
        AfterCrit,
        BeforeSort,
        Sort,
        AfterSort,
        /// <summary>
        /// The function should be executed after all other expressions
        /// </summary>
        Last
    }
}
