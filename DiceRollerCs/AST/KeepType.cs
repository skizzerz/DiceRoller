using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// The type of a keep node. Certain keep types (advantage
    /// and disadvantage) also mean that the underlying roll happens
    /// twice.
    /// </summary>
    public enum KeepType
    {
        /// <summary>
        /// Keep the N highest rolls
        /// </summary>
        KeepHigh,
        /// <summary>
        /// Keep the N lowest rolls
        /// </summary>
        KeepLow,
        /// <summary>
        /// Drop the N highest rolls
        /// </summary>
        DropHigh,
        /// <summary>
        /// Drop the N lowest rolls
        /// </summary>
        DropLow,
        /// <summary>
        /// Roll twice, keeping the highest
        /// </summary>
        Advantage,
        /// <summary>
        /// Roll twice, keeping the lowest
        /// </summary>
        Disadvantage
    }
}
