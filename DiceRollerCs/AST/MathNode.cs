using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

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

        public override string ToString()
        {
            // This logic is necessarily different from DoMath below since we cannot inspect Values.
            // Furthermore, children already have their own grouping, so parens are only necessary if we're trying
            // to break order of operations with nested math nodes
            StringBuilder sb = new StringBuilder();

            if (Left is MathNode ml
                && (Operation == MathOp.Multiply || Operation == MathOp.Divide)
                && (ml.Operation == MathOp.Add || ml.Operation == MathOp.Subtract))
            {
                sb.AppendFormat("({0})", Left.ToString());
            }
            else
            {
                sb.Append(Left.ToString());
            }

            switch (Operation)
            {
                case MathOp.Add:
                    sb.Append(" + ");
                    break;
                case MathOp.Subtract:
                    sb.Append(" - ");
                    break;
                case MathOp.Multiply:
                    sb.Append(" * ");
                    break;
                case MathOp.Divide:
                    sb.Append(" / ");
                    break;
                default:
                    sb.Append("<<UNKNOWN MATH>>");
                    break;
            }

            if (Right is MathNode mr
                && (Operation == MathOp.Divide
                    || Operation == MathOp.Subtract
                    || (Operation == MathOp.Multiply && (mr.Operation == MathOp.Add || mr.Operation == MathOp.Subtract))))
            {
                sb.AppendFormat("({0})", Right.ToString());
            }
            else
            {
                sb.Append(Right.ToString());
            }

            return sb.ToString();
        }

        protected override long EvaluateInternal(RollerConfig conf, DiceAST root, int depth)
        {
            long rolls = Left.Evaluate(conf, root, depth + 1) + Right.Evaluate(conf, root, depth + 1);
            DoMath();

            return rolls;
        }

        protected override long RerollInternal(RollerConfig conf, DiceAST root, int depth)
        {
            long rolls = Left.Reroll(conf, root, depth + 1) + Right.Reroll(conf, root, depth + 1);
            DoMath();

            return rolls;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Cannot be easily refactored.")]
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

            // we maintain a ValueType of successes only if all sides which contain actual rolls have a successes ValueType
            bool haveTotal = false;
            bool haveRoll = false;

            if (Left.Values.Any(d => d.DieType.IsRoll()))
            {
                haveRoll = true;

                if (Left.ValueType == ResultType.Total)
                {
                    haveTotal = true;
                }
            }

            if (Right.Values.Any(d => d.DieType.IsRoll()))
            {
                haveRoll = true;

                if (Right.ValueType == ResultType.Total)
                {
                    haveTotal = true;
                }
            }

            if (!haveRoll || haveTotal)
            {
                ValueType = ResultType.Total;
            }
            else
            {
                ValueType = ResultType.Successes;
            }

            // Insert special DieResults to aid in displaying the grouping of these rolls.
            // Add in parenthesis and the operator used, e.g. 3d6-2d4 with a roll of
            // 3, 4, 5, 1, and 2 would render as ( 3 + 4 + 5 ) - ( 1 + 2 ).
            // Parenthesis are not added if one side has only one child.
            _values.Clear();
            bool addLeftParen = Left.Values.Count != 1;
            bool addRightParen = Right.Values.Count != 1;
            var leftRoll = Left.GetUnderlyingRollNode();
            var rightRoll = Right.GetUnderlyingRollNode();

            if (addLeftParen)
            {
                if (leftRoll is MathNode ml)
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
                else if (leftRoll is GroupNode gl)
                {
                    // group nodes internally are all addition, so we don't need to wrap addition in parens if we're adding/subtracting
                    if (Operation == MathOp.Add || Operation == MathOp.Subtract)
                    {
                        addLeftParen = false;
                    }
                    // the gnarly mess below is testing if the group node is already wrapped in a single set of parens
                    // if it is, then we don't add another set. So, we check for (1+2+3) and don't turn that into ((1+2+3)), but
                    // we DO turn (1+2)+(3+4) into ((1+2)+(3+4)) unless we're adding/subtracting (which is caught above).
                    else if (gl.Values[0].IsSpecialDie(SpecialDie.OpenParen)
                        && gl.Values[gl.Values.Count - 1].IsSpecialDie(SpecialDie.CloseParen)
                        && gl.Values.Count(d => d.IsSpecialDie(SpecialDie.OpenParen)) == 1)
                    {
                        addLeftParen = false;
                    }
                }
                else if (leftRoll is RollNode)
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
                if (rightRoll is MathNode mr)
                {
                    if (Operation == MathOp.Add || mr.Operation == MathOp.Multiply)
                    {
                        addRightParen = false;
                    }
                }
                else if (rightRoll is GroupNode gr)
                {
                    if (Operation == MathOp.Add)
                    {
                        addRightParen = false;
                    }
                    else if (gr.Values[0].IsSpecialDie(SpecialDie.OpenParen)
                        && gr.Values[gr.Values.Count - 1].IsSpecialDie(SpecialDie.CloseParen)
                        && gr.Values.Count(d => d.IsSpecialDie(SpecialDie.OpenParen)) == 1)
                    {
                        addRightParen = false;
                    }
                }
                else if (rightRoll is RollNode)
                {
                    if (Operation == MathOp.Add)
                    {
                        addRightParen = false;
                    }
                }
            }

            if (addLeftParen)
            {
                _values.Add(new DieResult(SpecialDie.OpenParen));
            }
            _values.AddRange(Left.Values);
            if (addLeftParen)
            {
                _values.Add(new DieResult(SpecialDie.CloseParen));
            }

            _values.Add(new DieResult(sd));

            if (addRightParen)
            {
                _values.Add(new DieResult(SpecialDie.OpenParen));
            }
            _values.AddRange(Right.Values);
            if (addRightParen)
            {
                _values.Add(new DieResult(SpecialDie.CloseParen));
            }
        }
    }
}
