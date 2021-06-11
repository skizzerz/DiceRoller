using System.Collections.Generic;
using System.Globalization;

namespace Dice.AST
{
    /// <summary>
    /// Represents a node which contains a literal value.
    /// </summary>
    public class LiteralNode : DiceAST
    {
        private readonly List<DieResult> _values;

        /// <inheritdoc/>
        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralNode"/> class with the specified value.
        /// </summary>
        /// <param name="value">Literal value of this node.</param>
        internal LiteralNode(decimal value)
        {
            Value = value;
            ValueType = ResultType.Total;
            _values = new List<DieResult>()
            {
                new DieResult()
                {
                    DieType = DieType.Literal,
                    NumSides = 0,
                    Value = value,
                    Flags = 0
                }
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        /// <inheritdoc/>
        protected override long EvaluateInternal(RollData data, DiceAST root, int depth)
        {
            return 0;
        }

        /// <inheritdoc/>
        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            return 0;
        }
    }
}
