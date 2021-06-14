using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dice.AST
{
    /// <summary>
    /// Base class for partial nodes; this should not exist in an AST.
    /// </summary>
    internal abstract class PartialNode : DiceAST
    {
        /// <summary>
        /// Function scope used when AddFunctionNodes is called.
        /// </summary>
        protected abstract FunctionScope FunctionScope { get; }

        /// <summary>
        /// Functions attached to the partial node. Will be formed properly into a subtree when the node is finished.
        /// </summary>
        public List<FunctionNode> Functions { get; protected set; } = new List<FunctionNode>();

        /// <inheritdoc/>
        public override IReadOnlyList<DieResult> Values => throw new InvalidOperationException("This node should not exist in an AST");

        /// <summary>
        /// Add a function node to this partial node. Should only be called by the parser listener.
        /// </summary>
        /// <param name="fn">Function to add.</param>
        internal void AddFunction(FunctionNode fn)
        {
            Functions.Add(fn);
        }

        /// <summary>
        /// Build a subtree by adding all functions attached to this partial node as children
        /// to the passed-in node.
        /// </summary>
        /// <param name="timing">Timing filter; only functions with the given timing are added.</param>
        /// <param name="node">Node to add functions as children to. Will be replaced with the final descendent added.</param>
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

        /// <inheritdoc/>
        protected override long EvaluateInternal(RollData data, DiceAST root, int depth)
        {
            throw new InvalidOperationException("This node should not exist in an AST");
        }

        /// <inheritdoc/>
        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            throw new InvalidOperationException("This node should not exist in an AST");
        }
    }
}
