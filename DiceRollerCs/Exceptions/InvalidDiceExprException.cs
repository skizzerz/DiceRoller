using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.Exceptions
{
    public class InvalidDiceExprException : DiceException
    {
        public InvalidDiceExprException(string message)
            : base(message) { }
    }
}
