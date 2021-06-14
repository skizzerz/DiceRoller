using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// An ephemeral node used in construction of the AST. Once the AST
    /// is fully constructed, no nodes of this type should exist in it.
    /// </summary>
    internal class RollPartialNode : PartialNode
    {
        /// <summary>
        /// The underlying roll being made.
        /// </summary>
        public RollNode Roll { get; internal set; }

        /// <inheritdoc/>
        protected override FunctionScope FunctionScope => FunctionScope.Basic;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollPartialNode"/> class.
        /// </summary>
        /// <param name="roll">Underlying roll that may be augmented with functions.</param>
        internal RollPartialNode(RollNode roll)
        {
            Roll = roll;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder("RPARTIAL<<");
            sb.Append(Roll.ToString());

            foreach (FunctionTiming timing in Enum.GetValues(typeof(FunctionTiming)))
            {
                foreach (var fn in Functions.Where(f => f.Slot.Timing == timing))
                {
                    sb.Append(fn.ToString());
                }
            }

            sb.Append(">>");

            return sb.ToString();
        }

        /// <summary>
        /// Creates the RollNode subtree.
        /// </summary>
        /// <returns>The full subtree for the RollNode and all functions attached to it. The leaf of the tree is returned.</returns>
        internal DiceAST CreateRollNode()
        {
            DiceAST roll = Roll;

            foreach (FunctionTiming timing in Enum.GetValues(typeof(FunctionTiming)))
            {
                AddFunctionNodes(timing, ref roll);
            }

            return roll;
        }
    }
}
