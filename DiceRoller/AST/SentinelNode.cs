using System;
using System.Collections.Generic;
using System.Text;

namespace Dice.AST
{
    /// <summary>
    /// Represents a marker on the stack.
    /// </summary>
    public class SentinelNode : DiceAST
    {
        /// <summary>
        /// Marker name/type.
        /// </summary>
        public string Marker { get; private set; }

        /// <summary>
        /// Data associated with this marker, if any.
        /// </summary>
        public object? Data { get; private set; }

        /// <inheritdoc/>
        public override IReadOnlyList<DieResult> Values => throw new InvalidOperationException("This node should not exist in an AST");

        /// <summary>
        /// Initializes a new instance of the <see cref="SentinelNode"/> class.
        /// </summary>
        /// <param name="marker">Sentinel type.</param>
        /// <param name="data">Optional data to associate with this node.</param>
        internal SentinelNode(string marker, object? data = null)
        {
            Marker = marker;
            Data = data;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"SENTINEL<<{Marker}>>";
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
