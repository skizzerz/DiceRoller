using System;
using System.Collections.Generic;
using System.Linq;

namespace Dice.AST
{
    /// <summary>
    /// Represents a math expression on two nodes
    /// </summary>
    public class MathNode : DiceAST
    {
        /// <summary>
        /// The math operation to be performed
        /// </summary>
        public MathOp Operation { get; private set; }

        /// <summary>
        /// Left hand side of the math expression
        /// </summary>
        public DiceAST Left { get; private set; }

        /// <summary>
        /// Right hand side of the math expression
        /// </summary>
        public DiceAST Right { get; private set; }

        public override IReadOnlyList<DieResult> Values
        {
            get
            {
                List<DieResult> values = Left.Values.ToList();
                values.AddRange(Right.Values);

                return values;
            }
        }

        internal MathNode(MathOp operation, DiceAST left, DiceAST right)
        {
            Operation = operation;
            Left = left ?? throw new ArgumentNullException("left");
            Right = right ?? throw new ArgumentNullException("right");
        }

        protected override ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = Left.Evaluate(conf, root, depth + 1) + Right.Evaluate(conf, root, depth + 1);
            DoMath();

            return rolls;
        }

        protected override ulong RerollInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = Left.Reroll(conf, root, depth + 1) + Right.Reroll(conf, root, depth + 1);
            DoMath();

            return rolls;
        }

        private void DoMath()
        {
            switch (Operation)
            {
                case MathOp.Add:
                    Value = Left.Value + Right.Value;
                    break;
                case MathOp.Subtract:
                    Value = Left.Value - Right.Value;
                    break;
                case MathOp.Multiply:
                    Value = Left.Value * Right.Value;
                    break;
                case MathOp.Divide:
                    Value = Left.Value / Right.Value;
                    break;
                default:
                    throw new InvalidOperationException("Math operation not recognized");
            }
        }
    }
}
