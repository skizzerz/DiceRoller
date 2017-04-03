using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// Represents a node which contains a literal value
    /// </summary>
    public class LiteralNode : DiceAST
    {
        public override IReadOnlyList<DieResult> Values
        {
            get { return new List<DieResult>(); }
        }

        internal LiteralNode(decimal value)
        {
            Value = value;
        }

        protected override ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            return 0;
        }

        protected override ulong RerollInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            return 0;
        }
    }
}
