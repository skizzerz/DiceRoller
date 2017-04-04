using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// Represents a function call, either a global function
    /// or one attached to a roll.
    /// </summary>
    public class FunctionNode : DiceAST
    {
        /// <summary>
        /// The context for this function call
        /// </summary>
        public FunctionContext Context { get; private set; }

        public override IReadOnlyList<DieResult> Values
        {
            get { return Context.Values ?? Context.Expression?.Values ?? new List<DieResult>(); }
        }

        internal FunctionNode(FunctionScope scope, string name, IReadOnlyList<DiceAST> arguments, DiceAST expression)
        {
            Context = new FunctionContext(scope, name, arguments, expression);
        }

        protected override ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = Context.Expression?.Evaluate(conf, root, depth + 1) ?? 0;

            foreach (var arg in Context.Arguments)
            {
                rolls += arg.Evaluate(conf, root, depth + 1);
            }


        }
    }
}
