using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dice.Exceptions;

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

        public IReadOnlyList<DieResult> Values
        {
            get
            {

            }
        }

        internal MathNode(MathOp operation, DiceAST left, DiceAST right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }

            if (right == null)
            {
                throw new ArgumentNullException("right");
            }

            Operation = operation;
            Left = left;
            Right = right;
        }

        protected override ulong EvaluateInternal(RollerConfig conf, DiceAST root, uint depth)
        {
            ulong rolls = Left.Evaluate(conf, root, depth + 1) + Right.Evaluate(conf, root, depth + 1);

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

            return rolls;
        }
    }
}
