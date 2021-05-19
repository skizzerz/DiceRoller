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
    internal class GroupPartialNode : DiceAST
    {
        public DiceAST? NumTimes { get; internal set; }
        public List<DiceAST> GroupExpressions { get; private set; }
        // a GroupNode can have at most SortNode attached to it,
        // however any number of KeepNodes and success/failure Comparisons can be applied.
        public List<KeepNode> Keep { get; private set; }
        public SortNode? Sort { get; private set; }
        public SuccessNode? Success { get; private set; }
        public List<FunctionNode> Functions { get; private set; }

        private bool haveAdvantage = false;

        public override IReadOnlyList<DieResult> Values => throw new InvalidOperationException("This node should not exist in an AST");

        internal GroupPartialNode()
        {
            GroupExpressions = new List<DiceAST>();
            Keep = new List<KeepNode>();
            Sort = null;
            Success = null;
            Functions = new List<FunctionNode>();
            NumTimes = null;
        }

        internal void AddExpression(DiceAST expression)
        {
            GroupExpressions.Add(expression);
        }

        internal void AddKeep(KeepNode keep)
        {
            if (keep.KeepType == KeepType.Advantage || keep.KeepType == KeepType.Disadvantage)
            {
                if (Keep.Count > 0)
                {
                    if (haveAdvantage)
                    {
                        throw new DiceException(DiceErrorCode.AdvantageOnlyOnce);
                    }
                    else
                    {
                        throw new DiceException(DiceErrorCode.NoAdvantageKeep);
                    }
                }

                haveAdvantage = true;
            }
            else if (haveAdvantage)
            {
                throw new DiceException(DiceErrorCode.NoAdvantageKeep);
            }

            Keep.Add(keep);
        }

        internal void AddSort(SortNode sort)
        {
            if (Sort != null)
            {
                throw new DiceException(DiceErrorCode.TooManySort);
            }

            Sort = sort;
        }

        internal void AddSuccess(SuccessNode success)
        {
            if (Success == null)
            {
                Success = success;
            }
            else
            {
                Success.AddSuccess(success.Success);
                Success.AddFailure(success.Failure);
            }
        }

        internal void AddFunction(FunctionNode fn)
        {
            Functions.Add(fn);
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
            foreach (var k in Keep)
            {
                k.Expression = group;
                group = k;
            }
            AddFunctionNodes(FunctionTiming.AfterKeep, ref group);
            AddFunctionNodes(FunctionTiming.BeforeSuccess, ref group);
            if (Success != null)
            {
                if (Success.Success == null)
                {
                    throw new DiceException(DiceErrorCode.InvalidSuccess);
                }

                Success.Expression = group;
                group = Success;
            }
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

        private void AddFunctionNodes(FunctionTiming timing, ref DiceAST node)
        {
            if (Functions.Count == 0)
            {
                return;
            }

            // RollData instance is the same for every function, so just grab an arbitrary one
            var data = Functions.First().Context.Data;
            var needsCombining = new HashSet<string>(
                Functions.Where(f => f.Slot.Timing == timing && f.Slot.Behavior == FunctionBehavior.CombineArguments)
                    .Select(f => f.Slot.Name));
            var combined = new Dictionary<string, FunctionNode>();
            var finishedCombining = new HashSet<string>();

            foreach (var fn in needsCombining)
            {
                var combinedArgs = new List<DiceAST>();
                foreach (var toCombine in Functions.Where(f => f.Slot.Timing == timing && f.Slot.Name == fn))
                {
                    combinedArgs.AddRange(toCombine.Context.Arguments);
                }

                combined[fn] = new FunctionNode(FunctionScope.Group, fn, combinedArgs, data);
            }

            foreach (var fn in Functions.Where(f => f.Slot.Timing == timing))
            {
                if (needsCombining.Contains(fn.Slot.Name))
                {
                    // check if we've already added the combined function
                    if (finishedCombining.Contains(fn.Slot.Name))
                    {
                        continue;
                    }

                    combined[fn.Slot.Name].Context.Expression = node;
                    node = combined[fn.Slot.Name];
                    finishedCombining.Add(fn.Slot.Name);
                }
                else
                {
                    fn.Context.Expression = node;
                    node = fn;
                }
            }
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

            foreach (var k in Keep)
            {
                sb.Append(k.ToString());
            }

            if (Sort != null)
            {
                sb.Append(Sort.ToString());
            }

            if (Success != null)
            {
                sb.Append(Success.ToString());
            }

            foreach (var f in Functions)
            {
                sb.Append(f.ToString());
            }

            sb.Append(">>");

            return sb.ToString();
        }

        protected override long EvaluateInternal(RollData data, DiceAST root, int depth)
        {
            throw new InvalidOperationException("This node should not exist in an AST");
        }

        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            throw new InvalidOperationException("This node should not exist in an AST");
        }
    }
}
