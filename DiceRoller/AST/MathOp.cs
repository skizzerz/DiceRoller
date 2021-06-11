using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// Represents the operation performed on each side of a math expression.
    /// </summary>
    public enum MathOp
    {
        /// <summary>
        /// Add two parameters.
        /// </summary>
        Add,

        /// <summary>
        /// Subtract one parameter from the other.
        /// </summary>
        Subtract,

        /// <summary>
        /// Multiply two parameters.
        /// </summary>
        Multiply,

        /// <summary>
        /// Divide one parameter by the other.
        /// </summary>
        Divide,

        /// <summary>
        /// Negate the parameter. This operation is unary.
        /// </summary>
        Negate
    }
}
