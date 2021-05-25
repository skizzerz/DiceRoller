using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dice.AST
{
    /// <summary>
    /// Base class for partial nodes; this should not exist in an AST
    /// </summary>
    internal abstract class PartialNode : DiceAST
    {
        protected abstract FunctionScope FunctionScope { get; }

        public List<FunctionNode> Functions { get; protected set; } = new List<FunctionNode>();

        public override IReadOnlyList<DieResult> Values => throw new InvalidOperationException("This node should not exist in an AST");

        internal void AddFunction(FunctionNode fn)
        {
            Functions.Add(fn);
        }

        protected void AddFunctionNodes(FunctionTiming timing, ref DiceAST node)
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

                combined[fn] = new FunctionNode(FunctionScope, fn, combinedArgs, data);
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
