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
        Add = 0,
        Subtract = 1,
        Multiply = 2,
        Divide = 3,
        OpenParen = 4,
        CloseParen = 5
    }
}
