using System;
using System.Collections.Generic;
using System.Text;

using Dice.AST;

namespace Dice.Builtins
{
    /// <summary>
    /// Contains data about a reroll condition, including how many times it has been rerolled.
    /// </summary>
    internal class RerollData
    {
        /// <summary>
        /// Current number of rerolls executed for this instance.
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        /// Maximum number of rerolls to execute for this instance.
        /// </summary>
        public int Max { get; set; }

        /// <summary>
        /// Comparison to check when determining whether to reroll as part of this RerollData instance.
        /// </summary>
        public ComparisonNode Comparison { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RerollData"/> class.
        /// </summary>
        /// <param name="max">Maximum number of rerolls to execute.</param>
        /// <param name="comparison">Comparison to determine whether to reroll.</param>
        public RerollData(int max, ComparisonNode comparison)
        {
            Current = 0;
            Max = max;
            Comparison = comparison;
        }
    }
}
