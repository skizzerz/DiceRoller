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
            if (String.IsNullOrWhiteSpace(param))
            {
                throw new DiceException(DiceErrorCode.InvalidMacro, new ArgumentException("Macro param cannot consist of only whitespace", "param"));
            }

            Context = new MacroContext(param.Trim());
            _values = new List<DieResult>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder("[");
            sb.Append(Context.Name);
            for (int i = 1; i < Context.Arguments.Count; i++)
            {
                sb.AppendFormat(":{0}", Context.Arguments[i]);
            }
            sb.Append("]");

            return sb.ToString();
        }

        protected override long EvaluateInternal(RollData data, DiceAST root, int depth)
        {
            if (data.MacroRegistry.Contains(Context.Name))
            {
                data.MacroRegistry.Get(Context.Name).callback(Context);
            }
            else if (data.Config.MacroRegistry.Contains(Context.Name))
            {
                data.Config.MacroRegistry.Get(Context.Name).callback(Context);
            }

            data.MacroRegistry.GlobalCallbacks?.Invoke(Context);
            data.Config.MacroRegistry.GlobalCallbacks?.Invoke(Context);

            Value = Context.Value;
            ValueType = Context.ValueType;
            data.InternalContext.AllMacros.Add(Value);

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

        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            // Macros are currently only evaluated once. This may change in the future
            // once it is more understood what these can be used for.
            return 0;
        }
    }
}
