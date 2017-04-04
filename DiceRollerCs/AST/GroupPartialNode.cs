using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dice.Exceptions;

namespace Dice.AST
{
    /// <summary>
    /// An ephemeral node used in construction of the AST. Once the AST
    /// is fully constructed, no nodes of this type should exist in it.
    /// </summary>
    public class GroupPartialNode : DiceAST
    {
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
                    throw new InvalidDiceExprException("Cannot apply advantage/disadvantage onto a roll with a keep/drop expression");
                }

                haveAdvantage = true;
            }
            else if (haveAdvantage)
            {
                throw new InvalidDiceExprException("Cannot apply a keep/drop expression onto a roll with advantage/disadvantage");
            }

            Keep.Add(keep ?? throw new ArgumentNullException("keep"));
        }

        internal void AddSort(SortNode sort)
        {
            if (Sort != null)
            {
                throw new InvalidDiceExprException("Cannot add more than one sort expression to a roll");
            }

            Sort = sort ?? throw new ArgumentNullException("sort");
        }

        internal void AddSuccess(ComparisonNode comp)
        {
            Success.Add(comp ?? throw new ArgumentNullException("comp"));
        }

        internal void AddFailure(ComparisonNode comp)
        {
            Failure.Add(comp ?? throw new ArgumentNullException("comp"));
        }

        internal void AddFunction(FunctionNode fn)
        {
            Functions.Add(fn ?? throw new ArgumentNullException("fn"));
        }

        /// <summary>
        /// Creates the GroupNode from all of the partial pieces and returns the root of the GroupNode's subtree
        /// </summary>
        /// <returns></returns>
        internal DiceAST CreateGroupNode()
        {

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
