using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice
{
    /// <summary>
    /// Special dice are inserted in the Values list for a roll node
    /// in order to present information about how the overall Value was
    /// calculated.
    /// </summary>
    public enum SpecialDie
    {
        /// <summary>
        /// Indicates things being added, represented as a "+".
        /// </summary>
        Add = 0,

        /// <summary>
        /// Indicates things being subtracted, represented as a "-".
        /// </summary>
        Subtract = 1,

        /// <summary>
        /// Indicates things being multiplied, represented as a "*" or "×".
        /// </summary>
        Multiply = 2,

        /// <summary>
        /// Indicates things being divided, represented as a "/" or "÷".
        /// </summary>
        Divide = 3,

        /// <summary>
        /// Indicates an open parenthesis "(" used for order of operations or grouping.
        /// </summary>
        OpenParen = 4,

        /// <summary>
        /// Indicates a close parenthesis ")" used for order of operations or grouping.
        /// </summary>
        CloseParen = 5,

        /// <summary>
        /// Indicates a unary negation, represented as a "-".
        /// Distinct from Subtract so that appropriate spacing can be used.
        /// </summary>
        Negate = 6,

        /// <summary>
        /// Indicates a comma separating grouped roll elements.
        /// </summary>
        Comma = 7,

        /// <summary>
        /// Indicates arbitrary text; the DieResult will contain the text itself.
        /// </summary>
        Text = 8
    }
}
