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
        Add,
        Subtract,
        Multiply,
        Divide
    }
}
