using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.Exceptions
{
    public class DiceDivideByZeroException : DiceException
    {
        public DiceDivideByZeroException()
            : base("Attempted to divide by 0")
        { }
    }
}
