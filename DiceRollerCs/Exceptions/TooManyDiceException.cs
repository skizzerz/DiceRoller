using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.Exceptions
{
    public class TooManyDiceException : DiceException
    {
        public TooManyDiceException(uint maxDice)
            : base(String.Format("Maximum number of dice {0} exceeded.", maxDice))
        {
            Data["MaxDice"] = maxDice;
        }
    }
}
