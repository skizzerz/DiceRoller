using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dice.Grammar;

namespace Dice
{
    /// <summary>
    /// This class exposes the Roll method, which is used to roll dice.
    /// </summary>
    public static class Roller
    {
        /// <summary>
        /// Configuration for this Roller, such as maximum number of dice, sides, and nesting depth.
        /// It can also control which grammar is used to parse dice expressions, in the event a custom
        /// grammar is desirable.
        /// </summary>
        public static RollerConfig Config { get; set; }

        static Roller()
        {
            Config = new RollerConfig();
        }

        /// <summary>
        /// Rolls dice according to the given dice expression. Please see
        /// the documentation for more details on the formatting for this string.
        /// </summary>
        /// <param name="diceExpr">Dice expression to roll.</param>
        /// <returns>A RollResult containing the details of the roll.</returns>
        public static RollResult Roll(string diceExpr)
        {
            
        }
    }
}
