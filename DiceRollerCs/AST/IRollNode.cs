using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// Represents a node which contains an actual dice roll
    /// </summary>
    internal interface IRollNode
    {
        /// <summary>
        /// Re-do the roll without re-evaluating the entire subtree again
        /// </summary>
        /// <param name="conf">Roller config</param>
        /// <param name="root">AST root</param>
        /// <param name="depth">Recursion depth</param>
        /// <returns>Number of dice rolls performed</returns>
        uint Reroll(RollerConfig conf, DiceAST root, uint depth);
    }
}
