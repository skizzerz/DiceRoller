using System;
using System.Collections.Generic;
using System.Text;

namespace Dice.AST
{
    /// <summary>
    /// Represents a marker on the stack
    /// </summary>
    public class SentinelNode : DiceAST
    {
        public string Marker { get; private set; }

        public override IReadOnlyList<DieResult> Values => throw new InvalidOperationException("This node should not exist in an AST");

        internal SentinelNode(string marker)
        {
            Marker = marker;
        }

        public override string ToString()
        {
            return $"SENTINEL<<{Marker}>>";
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
