using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice
{
    /// <summary>
    /// Whether a RollResult's value represents the sum of the dice rolled
    /// or the number of successes.
    /// </summary>
    public enum ResultType
    {
        Total,
        Successes
    }
}
