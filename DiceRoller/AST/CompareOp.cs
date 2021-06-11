using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// Type of comparison to perform.
    /// </summary>
    public enum CompareOp
    {
        /// <summary>
        /// Check if the expression is equal to the comparison value.
        /// </summary>
        Equals,

        /// <summary>
        /// Check if the expression is greater than the comparison value.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Check if the expression is less than the comparison value.
        /// </summary>
        LessThan,

        /// <summary>
        /// Check if the expression is greater than or equal to the comparison value.
        /// </summary>
        GreaterEquals,

        /// <summary>
        /// Check if the expression is less than or equal to the comparison value.
        /// </summary>
        LessEquals,

        /// <summary>
        /// Check if the expression is not equal to the comparison value.
        /// </summary>
        NotEquals
    }
}
