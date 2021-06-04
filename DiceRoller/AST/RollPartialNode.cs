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
        public RollNode Roll { get; internal set; }

        protected override FunctionScope FunctionScope => FunctionScope.Basic;

        internal RollPartialNode(RollNode roll)
        {
            Roll = roll;
        }

        // this won't appear in the overall AST, but in the course of debugging it may be worthwhile to print out a partial node
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
        /// Creates the RollNode subtree
        /// </summary>
        /// <returns></returns>
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
