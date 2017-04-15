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
        private List<DieResult> _values;

        /// <summary>
        /// Macro context passed to the executor function. The executor fills this out.
        /// </summary>
        public MacroContext Context { get; private set; }

        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        internal MacroNode(string param)
        {
            Context = new MacroContext(param);
            _values = new List<DieResult>();
        }

        public override string ToString()
        {
            return String.Format("[{0}]", Context.Param);
        }

        internal decimal Execute()
        {
            EvaluateInternal(null, null, 0);
            return Value;
        }

        protected override long EvaluateInternal(RollerConfig conf, DiceAST root, int depth)
        {
            if (conf.ExecuteMacro == null)
            {
                throw new DiceException(DiceErrorCode.InvalidMacro);
            }

            conf.ExecuteMacro(Context);
            Value = Context.Value;
            ValueType = Context.ValueType;

            if (Context.Value == Decimal.MinValue)
            {
                throw new DiceException(DiceErrorCode.InvalidMacro);
            }

            _values.Clear();
            if (Context.Values != null)
            {
                _values.AddRange(Context.Values);
            }

            if (_values.Count == 0)
            {
                _values.Add(new DieResult()
                {
                    DieType = DieType.Literal,
                    NumSides = 0,
                    Value = Context.Value,
                    Flags = DieFlags.Macro
                });
            }

            return 0;
        }

        protected override long RerollInternal(RollerConfig conf, DiceAST root, int depth)
        {
            // Macros are currently only evaluated once. This may change in the future
            // once it is more understood what these can be used for.
            return 0;
        }
    }
}
