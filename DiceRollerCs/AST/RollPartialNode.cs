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
    public class RollPartialNode : DiceAST
    {
        public RollNode Roll { get; internal set; }
        public List<KeepNode> Keep { get; private set; }
        public SortNode Sort { get; private set; }
        public RerollNode RerollNode { get; private set; }
        public ExplodeNode Explode { get; private set; }
        public List<ComparisonNode> Critical { get; private set; }
        public List<ComparisonNode> Fumble { get; private set; }
        public List<ComparisonNode> Success { get; private set; }
        public List<ComparisonNode> Failure { get; private set; }
        public List<FunctionNode> Functions { get; private set; }

        private bool haveAdvantage = false;

        public override IReadOnlyList<DieResult> Values => throw new InvalidOperationException("This node should not exist in an AST");

        internal RollPartialNode(RollNode roll)
        {
            Roll = roll ?? throw new ArgumentNullException("roll");
            Keep = new List<KeepNode>();
            Sort = null;
            RerollNode = null;
            Explode = null;
            Critical = new List<ComparisonNode>();
            Fumble = new List<ComparisonNode>();
            Success = new List<ComparisonNode>();
            Failure = new List<ComparisonNode>();
            Functions = new List<FunctionNode>();
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

        internal void AddReroll(RerollNode reroll)
        {
            if (RerollNode != null)
            {
                throw new DiceException(DiceErrorCode.TooManyReroll);
            }

            RerollNode = reroll ?? throw new ArgumentNullException("reroll");
        }

        internal void AddExplode(ExplodeNode explode)
        {
            if (Explode != null)
            {
                throw new DiceException(DiceErrorCode.TooManyExplode);
            }

            Explode = explode ?? throw new ArgumentNullException("explode");
        }

        internal void AddCritical(ComparisonNode comp)
        {
            if (comp == null)
            {
                return;
            }

            Critical.Add(comp);
        }

        internal void AddFumble(ComparisonNode comp)
        {
            if (comp == null)
            {
                return;
            }

            Fumble.Add(comp);
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
        /// Creates the RollNode subtree
        /// </summary>
        /// <returns></returns>
        internal DiceAST CreateGroupNode()
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
            if (Success.Count == 0 && Failure.Count > 0)
            {
                throw new DiceException(DiceErrorCode.InvalidSuccess);
            }
            ComparisonNode succ = null;
            ComparisonNode fail = null;
            if (Success.Count > 0)
            {
                succ = new ComparisonNode(Success);
            }
            if (Failure.Count > 0)
            {
                fail = new ComparisonNode(Failure);
            }
            if (succ != null)
            {
                roll = new SuccessNode(roll, succ, fail);
            }
            AddFunctionNodes(FunctionTiming.AfterSuccess, ref roll);
            AddFunctionNodes(FunctionTiming.BeforeCrit, ref roll);
            ComparisonNode crit = null;
            ComparisonNode fumb = null;
            if (Critical.Count > 0)
            {
                crit = new ComparisonNode(Critical);
            }
            if (Fumble.Count > 0)
            {
                fumb = new ComparisonNode(Fumble);
            }
            if (crit != null || fumb != null)
            {
                roll = new CritNode(roll, crit, fumb);
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
