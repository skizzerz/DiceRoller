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
    public class GroupPartialNode : DiceAST
    {
        public DiceAST NumTimes { get; internal set; }
        public List<DiceAST> GroupExpressions { get; private set; }
        // a GroupNode can have at most SortNode attached to it,
        // however any number of KeepNodes and success/failure Comparisons can be applied.
        public List<KeepNode> Keep { get; private set; }
        public SortNode Sort { get; private set; }
        public List<ComparisonNode> Success { get; private set; }
        public List<ComparisonNode> Failure { get; private set; }
        public List<FunctionNode> Functions { get; private set; }

        private bool haveAdvantage = false;

        public override IReadOnlyList<DieResult> Values => throw new InvalidOperationException("This node should not exist in an AST");

        internal GroupPartialNode()
        {
            GroupExpressions = new List<DiceAST>();
            Keep = new List<KeepNode>();
            Sort = null;
            Success = new List<ComparisonNode>();
            Failure = new List<ComparisonNode>();
            Functions = new List<FunctionNode>();
            NumTimes = null;
        }

        internal void AddExpression(DiceAST expression)
        {
            GroupExpressions.Add(expression ?? throw new ArgumentNullException("expression"));
        }

        internal void AddKeep(KeepNode keep)
        {
            if (keep.KeepType == KeepType.Advantage || keep.KeepType == KeepType.Disadvantage)
            {
                if (Keep.Count > 0)
                {
                    throw new DiceException(DiceErrorCode.NoAdvantageKeep);
                }

                haveAdvantage = true;
            }
            else if (haveAdvantage)
            {
                throw new DiceException(DiceErrorCode.NoAdvantageKeep);
            }

            Keep.Add(keep ?? throw new ArgumentNullException("keep"));
        }

        internal void AddSort(SortNode sort)
        {
            if (Sort != null)
            {
                throw new DiceException(DiceErrorCode.TooManySort);
            }

            Sort = sort ?? throw new ArgumentNullException("sort");
        }

        internal void AddSuccess(ComparisonNode comp)
        {
            if (comp == null)
            {
                return;
            }

            Success.Add(comp);
        }

        internal void AddFailure(ComparisonNode comp)
        {
            if (comp == null)
            {
                return;
            }

            Failure.Add(comp);
        }

        internal void AddFunction(FunctionNode fn)
        {
            Functions.Add(fn ?? throw new ArgumentNullException("fn"));
        }

        /// <summary>
        /// Creates the GroupNode from all of the partial pieces and returns the root of the GroupNode's subtree
        /// </summary>
        /// <param name="numTimes">Expression to roll the group some number of times, may be null</param>
        /// <returns></returns>
        internal DiceAST CreateGroupNode()
        {
            var group = new GroupNode(NumTimes, GroupExpressions);


        }

        protected override ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            throw new InvalidOperationException("This node should not exist in an AST");
        }

        protected override ulong RerollInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            throw new InvalidOperationException("This node should not exist in an AST");
        }
    }
}
