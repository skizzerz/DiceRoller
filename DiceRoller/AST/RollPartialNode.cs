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
    internal class RollPartialNode : DiceAST
    {
        public RollNode Roll { get; internal set; }
        public List<KeepNode> Keep { get; private set; }
        public SortNode? Sort { get; private set; }
        public CritNode? Critical { get; private set; }
        public SuccessNode? Success { get; private set; }
        public List<FunctionNode> Functions { get; private set; }

        private bool haveAdvantage = false;

        public override IReadOnlyList<DieResult> Values => throw new InvalidOperationException("This node should not exist in an AST");

        internal RollPartialNode(RollNode roll)
        {
            Roll = roll;
            Keep = new List<KeepNode>();
            Sort = null;
            Critical = null;
            Success = null;
            Functions = new List<FunctionNode>();
        }

        // this won't appear in the overall AST, but in the course of debugging it may be worthwhile to print out a partial node
        public override string ToString()
        {
            var sb = new StringBuilder("RPARTIAL<<");
            sb.Append(Roll.ToString());

            if (Success != null)
            {
                sb.Append(Success.ToString());
            }

            foreach (var k in Keep)
            {
                sb.Append(k.ToString());
            }

            if (Critical != null)
            {
                sb.Append(Critical.ToString());
            }

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

        internal void AddCritical(CritNode crit)
        {
            if (Critical == null)
            {
                Critical = crit;
            }
            else
            {
                Critical.AddCritical(crit.Critical);
                Critical.AddFumble(crit.Fumble);
            }
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
            
            foreach (var k in Keep)
            {
                k.Expression = roll;
                roll = k;
            }
            
            AddFunctionNodes(FunctionTiming.AfterKeep, ref roll);
            AddFunctionNodes(FunctionTiming.BeforeSuccess, ref roll);

            if (Success != null)
            {
                if (Success.Success == null)
                {
                    throw new DiceException(DiceErrorCode.InvalidSuccess);
                }

                Success.Expression = roll;
                roll = Success;
            }

            AddFunctionNodes(FunctionTiming.AfterSuccess, ref roll);
            AddFunctionNodes(FunctionTiming.BeforeCrit, ref roll);

            if (Critical != null)
            {
                Critical.Expression = roll;
                roll = Critical;
            }

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

        private void AddFunctionNodes(FunctionTiming timing, ref DiceAST node)
        {
            var fns = Functions.Where(f => f.Slot.Timing == timing).ToList();

            if (fns.Count == 0)
            {
                return;
            }

            // RollData instance is the same for every function, so just grab an arbitrary one
            var data = fns.First().Context.Data;

            // validate any preconditions for this timing
            // this happens before combining functions so that preconditions can check whether or not such combinations would be valid
            FunctionRegistry.FireValidateEvent(data, new ValidateEventArgs(timing, fns.Select(f => f.Context).ToList()));

            var needsComb = new HashSet<string>(fns.Where(f => f.Slot.Behavior == FunctionBehavior.CombineArguments).Select(f => f.Slot.Name));
            var combined = new Dictionary<string, FunctionNode>();
            var finishedComb = new HashSet<string>();

            foreach (var fn in needsComb)
            {
                var combinedArgs = new List<DiceAST>();
                foreach (var toCombine in fns.Where(f => f.Slot.Name == fn))
                {
                    combinedArgs.AddRange(toCombine.Context.Arguments);
                }

                combined[fn] = new FunctionNode(FunctionScope.Basic, fn, combinedArgs, data);
            }

            foreach (var fn in fns)
            {
                if (needsComb.Contains(fn.Slot.Name))
                {
                    // check if we've already added the combined function
                    if (finishedComb.Contains(fn.Slot.Name))
                    {
                        continue;
                    }

                    combined[fn.Slot.Name].Context.Expression = node;
                    node = combined[fn.Slot.Name];
                    finishedComb.Add(fn.Slot.Name);
                }
                else
                {
                    fn.Context.Expression = node;
                    node = fn;
                }
            }
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
