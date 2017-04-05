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
        private List<DieResult> _values;

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
            get { return _values; }
        }

        internal MathNode(MathOp operation, DiceAST left, DiceAST right)
        {
            Operation = operation;
            Left = left ?? throw new ArgumentNullException("left");
            Right = right ?? throw new ArgumentNullException("right");
            _values = new List<DieResult>();
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
            SpecialDie sd = 0;

            switch (Operation)
            {
                case MathOp.Add:
                    Value = Left.Value + Right.Value;
                    break;
                case MathOp.Subtract:
                    Value = Left.Value - Right.Value;
                    sd = SpecialDie.Subtract;
                    break;
                case MathOp.Multiply:
                    Value = Left.Value * Right.Value;
                    sd = SpecialDie.Multiply;
                    break;
                case MathOp.Divide:
                    if (Right.Value == 0)
                    {
                        // attempted division by 0, this normally throws a DivideByZeroException,
                        // except we want all exceptions that can arise from user input to derive from DiceException
                        throw new DiceException(DiceErrorCode.DivideByZero);
                    }
                    Value = Left.Value / Right.Value;
                    sd = SpecialDie.Divide;
                    break;
                default:
                    throw new InvalidOperationException("Math operation not recognized");
            }

            // Insert special DieResults to aid in displaying the grouping of these rolls.
            // Addition is implicit between dice, so no extra results are inserted for that case.
            // Otherwise, add in parenthesis and the operator used, e.g. 3d6-2d4 with a roll of
            // 3, 4, 5, 1, and 2 would render as ( 3 4 5 ) - ( 1 2 ) and would likely be displayed as
            // (3+4+5)-(1+2).
            _values.Clear();
            if (Operation == MathOp.Add)
            {
                _values.AddRange(Left.Values);
                _values.AddRange(Right.Values);
            }
            else
            {
                _values.Add(new DieResult()
                {
                    DieType = DieType.Special,
                    NumSides = 0,
                    Value = (decimal)SpecialDie.OpenParen,
                    Flags = 0
                });
                _values.AddRange(Left.Values);
                _values.Add(new DieResult()
                {
                    DieType = DieType.Special,
                    NumSides = 0,
                    Value = (decimal)SpecialDie.CloseParen,
                    Flags = 0
                });
                _values.Add(new DieResult()
                {
                    DieType = DieType.Special,
                    NumSides = 0,
                    Value = (decimal)sd,
                    Flags = 0
                });
                _values.Add(new DieResult()
                {
                    DieType = DieType.Special,
                    NumSides = 0,
                    Value = (decimal)SpecialDie.OpenParen,
                    Flags = 0
                });
                _values.AddRange(Right.Values);
            }
        }
    }
}
