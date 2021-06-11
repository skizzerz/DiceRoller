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
    /// <list type="bullet">
    /// <item><description>Adding new items, including in the middle of the enum</description></item>
    /// </list>
    /// The following changes to this enum are considered breaking:
    /// <list type="bullet">
    /// <item><description>Removing items</description></item>
    /// <item><description>Rearranging items</description></item>
    /// </list>
    /// </remarks>
    public enum FunctionTiming
    {
        /// <summary>
        /// The function should be executed before any other expressions.
        /// </summary>
        First,

        /// <summary>
        /// Execute before bultin explode functions.
        /// </summary>
        BeforeExplode,

        /// <summary>
        /// Builtin explode functions.
        /// Custom functions should generally not use this timing.
        /// </summary>
        Explode,

        /// <summary>
        /// Execute after builtin explode functions.
        /// </summary>
        AfterExplode,

        /// <summary>
        /// Execute before builtin reroll functions.
        /// </summary>
        BeforeReroll,

        /// <summary>
        /// Builtin reroll functions.
        /// Custom functions should generally not use this timing.
        /// </summary>
        Reroll,

        /// <summary>
        /// Execute after builtin reroll functions.
        /// </summary>
        AfterReroll,

        /// <summary>
        /// Execute before builtin keep/drop functions.
        /// </summary>
        BeforeKeep,

        /// <summary>
        /// Builtin keep/drop functions.
        /// Custom functions should generally not use this timing.
        /// </summary>
        Keep,

        /// <summary>
        /// Execute after builtin keep/drop functions.
        /// </summary>
        AfterKeep,

        /// <summary>
        /// Execute before builtin success/failure functions.
        /// </summary>
        BeforeSuccess,

        /// <summary>
        /// Builtin success/failure functions.
        /// Custom functions should generally not use this timing.
        /// </summary>
        Success,

        /// <summary>
        /// Execute after builtin success/failure functions.
        /// </summary>
        AfterSuccess,

        /// <summary>
        /// Execute before builtin crit/fumble functions.
        /// </summary>
        BeforeCrit,

        /// <summary>
        /// Builtin crit/fumble functions.
        /// Custom functions should generally not use this timing.
        /// </summary>
        Crit,

        /// <summary>
        /// Execute after builtin crit/fumble functions.
        /// </summary>
        AfterCrit,

        /// <summary>
        /// Execute before builtin sorting functions.
        /// </summary>
        BeforeSort,

        /// <summary>
        /// Builtin sorting functions.
        /// Custom functions should generally not use this timing.
        /// </summary>
        Sort,

        /// <summary>
        /// Execute after builtin sorting functions.
        /// </summary>
        AfterSort,

        /// <summary>
        /// The function should be executed after all other expressions.
        /// </summary>
        Last
    }
}
