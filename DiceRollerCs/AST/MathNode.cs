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
                    sd = SpecialDie.Add;
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
            // Add in parenthesis and the operator used, e.g. 3d6-2d4 with a roll of
            // 3, 4, 5, 1, and 2 would render as ( 3 + 4 + 5 ) - ( 1 + 2 ).
            // Parenthesis are not added if one side has only one child.
            _values.Clear();
            bool addLeftParen = Left.Values.Count != 1;
            bool addRightParen = Left.Values.Count != 1;

            if (addLeftParen)
            {
                var ml = Left as MathNode;
                var gl = Left as GroupNode;
                var rl = Left as RollNode;

                if (ml != null)
                {
                    // don't add parens unless it is required to make the order of operations match the tree
                    // (such as an add underneath a multiply)
                    if (Operation == MathOp.Add
                        || Operation == MathOp.Subtract
                        || ml.Operation == MathOp.Multiply
                        || ml.Operation == MathOp.Divide)
                    {
                        addLeftParen = false;
                    }
                }
                else if (gl != null)
                {
                    // group nodes internally are all addition, so we don't need to wrap addition in parens if we're adding/subtracting
                    if (Operation == MathOp.Add || Operation == MathOp.Subtract)
                    {
                        addLeftParen = false;
                    }
                    // the gnarly mess below is testing if the group node is already wrapped in a single set of parens
                    // if it is, then we don't add another set. So, we check for (1+2+3) and don't turn that into ((1+2+3)), but
                    // we DO turn (1+2)+(3+4) into ((1+2)+(3+4)) unless we're adding/subtracting (which is caught above).
                    else if (gl.Values[0].DieType == DieType.Special
                        && ((SpecialDie)gl.Values[0].Value) == SpecialDie.OpenParen
                        && gl.Values[gl.Values.Count - 1].DieType == DieType.Special
                        && ((SpecialDie)gl.Values[gl.Values.Count - 1].Value) == SpecialDie.CloseParen
                        && gl.Values.Count(d => d.DieType == DieType.Special && ((SpecialDie)d.Value) == SpecialDie.OpenParen) == 1)
                    {
                        addLeftParen = false;
                    }
                }
                else if (rl != null)
                {
                    // roll nodes internally are all addition, so we don't need to wrap addition in parens if we're adding/subtracting
                    if (Operation == MathOp.Add || Operation == MathOp.Subtract)
                    {
                        addLeftParen = false;
                    }
                }
            }

            // mostly the same as above, except with extra consideration given to the right side for subtraction/division
            // to ensure that such things ARE wrapped in parens
            if (addRightParen)
            {
                var mr = Right as MathNode;
                var gr = Right as GroupNode;
                var rr = Right as RollNode;

                if (mr != null)
                {
                    if (Operation == MathOp.Add || mr.Operation == MathOp.Multiply)
                    {
                        addRightParen = false;
                    }
                }
                else if (gr != null)
                {
                    if (Operation == MathOp.Add)
                    {
                        addRightParen = false;
                    }
                    else if (gr.Values[0].DieType == DieType.Special
                        && ((SpecialDie)gr.Values[0].Value) == SpecialDie.OpenParen
                        && gr.Values[gr.Values.Count - 1].DieType == DieType.Special
                        && ((SpecialDie)gr.Values[gr.Values.Count - 1].Value) == SpecialDie.CloseParen
                        && gr.Values.Count(d => d.DieType == DieType.Special && ((SpecialDie)d.Value) == SpecialDie.OpenParen) == 1)
                    {
                        addRightParen = false;
                    }
                }
                else if (rr != null)
                {
                    if (Operation == MathOp.Add)
                    {
                        addRightParen = false;
                    }
                }
            }

            if (addLeftParen)
            {
                _values.Add(new DieResult()
                {
                    DieType = DieType.Special,
                    NumSides = 0,
                    Value = (decimal)SpecialDie.OpenParen,
                    Flags = 0
                });
            }
            _values.AddRange(Left.Values);
            if (addLeftParen)
            {
                _values.Add(new DieResult()
                {
                    DieType = DieType.Special,
                    NumSides = 0,
                    Value = (decimal)SpecialDie.CloseParen,
                    Flags = 0
                });
            }

            _values.Add(new DieResult()
            {
                DieType = DieType.Special,
                NumSides = 0,
                Value = (decimal)sd,
                Flags = 0
            });

            if (addRightParen)
            {
                _values.Add(new DieResult()
                {
                    DieType = DieType.Special,
                    NumSides = 0,
                    Value = (decimal)SpecialDie.OpenParen,
                    Flags = 0
                });
            }
            _values.AddRange(Right.Values);
            if (addRightParen)
            {
                _values.Add(new DieResult()
                {
                    DieType = DieType.Special,
                    NumSides = 0,
                    Value = (decimal)SpecialDie.CloseParen,
                    Flags = 0
                });
            }
        }
    }
}
