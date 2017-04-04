using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// Represents a macro whose exact functionality is not specified by this library.
    /// The RollerConfig specifies a class which all macro calls are dispatched to.
    /// </summary>
    public class MacroNode : DiceAST
    {
        /// <summary>
        /// Macro context passed to the executor function. The executor fills this out.
        /// </summary>
        public MacroContext Context { get; private set; }

        public override IReadOnlyList<DieResult> Values
        {
            get { return Context.Values; }
        }

        internal MacroNode(string param)
        {
            Context = new MacroContext(param);
        }

        protected override ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            conf.ExecuteMacro(Context);
            Value = Context.Value;

            return 0;
        }

        protected override ulong RerollInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            // Macros are currently only evaluated once. This may change in the future
            // once it is more understood what these can be used for.
            if (!Evaluated)
            {
                return Evaluate(conf, root, depth);
            }

            return 0;
        }
    }
}
