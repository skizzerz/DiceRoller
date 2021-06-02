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
    internal class GroupPartialNode : PartialNode
    {
        public DiceAST? NumTimes { get; internal set; }
        public List<DiceAST> GroupExpressions { get; private set; }
        public SortNode? Sort { get; private set; }

        protected override FunctionScope FunctionScope => FunctionScope.Group;

        internal GroupPartialNode()
        {
            GroupExpressions = new List<DiceAST>();
            Sort = null;
            Functions = new List<FunctionNode>();
            NumTimes = null;
        }

        internal void AddExpression(DiceAST expression)
        {
            GroupExpressions.Add(expression);
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
        /// Creates the GroupNode from all of the partial pieces and returns the root of the GroupNode's subtree
        /// </summary>
        /// <param name="numTimes">Expression to roll the group some number of times, may be null</param>
        /// <returns></returns>
        internal DiceAST CreateGroupNode()
        {
            DiceAST group = new GroupNode(NumTimes, GroupExpressions);
            AddFunctionNodes(FunctionTiming.First, ref group);
            AddFunctionNodes(FunctionTiming.BeforeExplode, ref group);
            AddFunctionNodes(FunctionTiming.Explode, ref group);
            AddFunctionNodes(FunctionTiming.AfterExplode, ref group);
            AddFunctionNodes(FunctionTiming.BeforeReroll, ref group);
            AddFunctionNodes(FunctionTiming.Reroll, ref group);
            AddFunctionNodes(FunctionTiming.AfterReroll, ref group);
            AddFunctionNodes(FunctionTiming.BeforeKeep, ref group);
            AddFunctionNodes(FunctionTiming.Keep, ref group);
            AddFunctionNodes(FunctionTiming.AfterKeep, ref group);
            AddFunctionNodes(FunctionTiming.BeforeSuccess, ref group);
            AddFunctionNodes(FunctionTiming.Success, ref group);
            AddFunctionNodes(FunctionTiming.AfterSuccess, ref group);
            AddFunctionNodes(FunctionTiming.BeforeCrit, ref group);
            AddFunctionNodes(FunctionTiming.Crit, ref group);
            AddFunctionNodes(FunctionTiming.AfterCrit, ref group);
            AddFunctionNodes(FunctionTiming.BeforeSort, ref group);
            if (Sort != null)
            {
                Sort.Expression = group;
                group = Sort;
            }
            AddFunctionNodes(FunctionTiming.AfterSort, ref group);
            AddFunctionNodes(FunctionTiming.Last, ref group);

            return group;
        }

        // this won't appear in the overall AST, but in the course of debugging it may be worthwhile to print out a partial node
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("GPARTIAL<<");
            if (NumTimes != null)
            {
                if (NumTimes is LiteralNode || NumTimes is MacroNode)
                {
                    sb.Append(NumTimes.ToString());
                }
                else
                {
                    sb.AppendFormat("({0})", NumTimes.ToString());
                }
            }

            sb.Append('{');
            sb.Append(String.Join(", ", GroupExpressions.Select(o => o.ToString())));
            sb.Append('}');

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
    }
}
