using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.Exceptions
{
    public class DiceRecursionException : DiceException
    {
        public DiceRecursionException(ushort maxRecursion)
            : base(String.Format("Maximum recursion depth of {0} exceeded.", maxRecursion))
        {
            Data["MaxRecursionDepth"] = maxRecursion;
        }
    }
}
