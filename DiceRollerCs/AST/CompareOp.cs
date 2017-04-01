using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// The type of comparison to perform
    /// </summary>
    public enum CompareOp
    {
        Equals,
        GreaterThan,
        LessThan,
        GreaterEquals,
        LessEquals,
        NotEquals
    }
}
