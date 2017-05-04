using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.Serialization
{
    internal enum SerializedClass : sbyte
    {
        DieResult = 0,
        RollResult = 1,
        RollPost = 2
    }
}
