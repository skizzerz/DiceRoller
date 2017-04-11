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
        private List<DieResult> _values;

        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        internal LiteralNode(decimal value)
        {
            Value = value;
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

        public override string ToString()
        {
            return Value.ToString();
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
