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
        public SortNode Sort { get; private set; }
        public RerollNode RerollNode { get; private set; }
        public ExplodeNode Explode { get; private set; }
        public CritNode Critical { get; private set; }
        public SuccessNode Success { get; private set; }
        public List<FunctionNode> Functions { get; private set; }

        private bool haveAdvantage = false;

        public override IReadOnlyList<DieResult> Values => throw new InvalidOperationException("This node should not exist in an AST");

        internal RollPartialNode(RollNode roll)
        {
            Roll = roll;
            Keep = new List<KeepNode>();
            Sort = null;
            RerollNode = null;
            Explode = null;
            Critical = null;
            Success = null;
            Functions = new List<FunctionNode>();
        }

        // this won't appear in the overall AST, but in the course of debugging it may be worthwhile to print out a partial node
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("RPARTIAL<<");
            sb.Append(Roll.ToString());

            if (RerollNode != null)
            {
                sb.Append(RerollNode.ToString());
            }

            if (Explode != null)
            {
                sb.Append(Explode.ToString());
            }

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

        internal void AddReroll(RerollNode reroll)
        {
            if (RerollNode != null && RerollNode.MaxRerolls != reroll.MaxRerolls)
            {
                throw new DiceException(DiceErrorCode.MixedReroll);
            }

            if (RerollNode == null)
            {
                RerollNode = reroll;
            }
            else
            {
                RerollNode.Comparison.Add(reroll.Comparison);
            }
        }

        internal void AddExplode(ExplodeNode explode)
        {
            if (Explode != null && (Explode.ExplodeType != explode.ExplodeType || Explode.Compound != explode.Compound))
            {
                throw new DiceException(DiceErrorCode.MixedExplodeType);
            }

            if (Explode == null)
            {
                Explode = explode;
            }
            else
            {
                Explode.AddComparison(explode.Comparison);
            }
        }

        internal void AddCritical(CritNode crit)
        {
            if (Critical == null)
            {
                Critical = crit;
                return;
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
                return;
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
            if (Explode != null)
            {
                Explode.Expression = roll;
                roll = Explode;
            }
            AddFunctionNodes(FunctionTiming.AfterExplode, ref roll);
            AddFunctionNodes(FunctionTiming.BeforeReroll, ref roll);
            if (RerollNode != null)
            {
                RerollNode.Expression = roll;
                roll = RerollNode;
            }
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
                if (Success.Success.Comparisons.Count() == 0 && Success.Failure.Comparisons.Count() > 0)
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
            foreach (var fn in Functions.Where(f => f.Timing == timing))
            {
                fn.Context.Expression = node;
                node = fn;
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
