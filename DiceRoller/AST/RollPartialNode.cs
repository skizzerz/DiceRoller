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
        public SortNode? Sort { get; private set; }

        protected override FunctionScope FunctionScope => FunctionScope.Basic;

        internal RollPartialNode(RollNode roll)
        {
            Roll = roll;
            Sort = null;
        }

        // this won't appear in the overall AST, but in the course of debugging it may be worthwhile to print out a partial node
        public override string ToString()
        {
            var sb = new StringBuilder("RPARTIAL<<");
            sb.Append(Roll.ToString());

            if (Sort != null)
            {
                sb.Append(Sort.ToString());
            }

            foreach (var f in Functions)
            {
                sb.Append(f.ToString());
            }

            sb.Append(">>");

            return sb.ToString();
        }

        internal void AddSort(SortNode sort)
        {
            if (Sort != null)
            {
                throw new DiceException(DiceErrorCode.TooManySort);
            }

            Sort = sort;
        }

        /// <summary>
        /// Creates the RollNode subtree
        /// </summary>
        /// <returns></returns>
        internal DiceAST CreateRollNode()
        {
            DiceAST roll = Roll;

            AddFunctionNodes(FunctionTiming.First, ref roll);
            AddFunctionNodes(FunctionTiming.BeforeExplode, ref roll);
            AddFunctionNodes(FunctionTiming.Explode, ref roll);
            AddFunctionNodes(FunctionTiming.AfterExplode, ref roll);
            AddFunctionNodes(FunctionTiming.BeforeReroll, ref roll);
            AddFunctionNodes(FunctionTiming.Reroll, ref roll);
            AddFunctionNodes(FunctionTiming.AfterReroll, ref roll);
            AddFunctionNodes(FunctionTiming.BeforeKeep, ref roll);
            AddFunctionNodes(FunctionTiming.Keep, ref roll);
            AddFunctionNodes(FunctionTiming.AfterKeep, ref roll);
            AddFunctionNodes(FunctionTiming.BeforeSuccess, ref roll);
            AddFunctionNodes(FunctionTiming.Success, ref roll);
            AddFunctionNodes(FunctionTiming.AfterSuccess, ref roll);
            AddFunctionNodes(FunctionTiming.BeforeCrit, ref roll);
            AddFunctionNodes(FunctionTiming.Crit, ref roll);
            AddFunctionNodes(FunctionTiming.AfterCrit, ref roll);
            AddFunctionNodes(FunctionTiming.BeforeSort, ref roll);

            if (Sort != null)
            {
                Sort.Expression = roll;
                roll = Sort;
            }

            AddFunctionNodes(FunctionTiming.AfterSort, ref roll);
            AddFunctionNodes(FunctionTiming.Last, ref roll);

            return roll;
        }
    }
}
