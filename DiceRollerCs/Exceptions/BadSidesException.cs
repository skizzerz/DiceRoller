using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.Exceptions
{
    public class BadSidesException : DiceException
    {
        public BadSidesException(uint maxSides)
            : base(String.Format("Dice must have between 1 and {0} sides.", maxSides))
        {
            Data["Error"] = "MaxSides";
            Data["MaxSides"] = maxSides;
        }

        public BadSidesException()
            : base("Only the following die sizes are valid: 2, 3, 4, 6, 8, 10, 12, 20, 100, 1000, 10000")
        {
            Data["Error"] = "WrongSides";
        }
    }
}
