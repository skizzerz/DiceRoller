using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly List<DieResult> _values;

        /// <summary>
        /// Macro context passed to the executor function. The executor fills this out.
        /// </summary>
        public MacroContext Context { get; private set; }

        /// <inheritdoc/>
        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MacroNode"/> class.
        /// </summary>
        /// <param name="param">Unparsed parameter of the macro.</param>
        /// <param name="data">Roll config.</param>
        internal MacroNode(string param, RollData data)
        {
            if (String.IsNullOrWhiteSpace(param))
            {
                throw new DiceException(DiceErrorCode.InvalidMacro, new ArgumentException("Macro param cannot consist of only whitespace", nameof(param)));
            }

            Context = new MacroContext(param.Trim(), data);
            _values = new List<DieResult>();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder("[");
            sb.Append(Context.Name);
            for (int i = 1; i < Context.Arguments.Count; i++)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, ":{0}", Context.Arguments[i]);
            }

            sb.Append(']');

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override long EvaluateInternal(RollData data, DiceAST root, int depth)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

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

        /// <inheritdoc/>
        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            // Macros are currently only evaluated once. This may change in the future
            // once it is more understood what these can be used for.
            return 0;
        }
    }
}
