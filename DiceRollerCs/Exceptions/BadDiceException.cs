using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.Exceptions
{
    public class BadDiceException : DiceException
    {
        public BadDiceException()
            : base("A negative number of dice cannot be rolled.")
        {

        }
    }
}
