using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;

namespace TestDiceRoller.AST
{
    [TestClass]
    public class MathNodeShould : TestBase
    {
        [TestMethod]
        public void Successfully_Add()
        {
            var node = new MathNode(MathOp.Add, One, One);
            EvaluateNode(node, Data(Roll1Conf), 0, "1 + 1 => 1 + 1 => 2");
        }

        [TestMethod]
        public void Successfully_Subtract()
        {
            var node = new MathNode(MathOp.Subtract, One, One);
            EvaluateNode(node, Data(Roll1Conf), 0, "1 - 1 => 1 - 1 => 0");
        }

        [TestMethod]
        public void Successfully_Multiply()
        {
            var node = new MathNode(MathOp.Multiply, One, One);
            EvaluateNode(node, Data(Roll1Conf), 0, "1 * 1 => 1 * 1 => 1");
        }

        [TestMethod]
        public void Successfully_Divide()
        {
            var node = new MathNode(MathOp.Divide, One, One);
            EvaluateNode(node, Data(Roll1Conf), 0, "1 / 1 => 1 / 1 => 1");
        }

        [TestMethod]
        public void ThrowDivideByZero_WhenDividingByZero()
        {
            var node = new MathNode(MathOp.Divide, One, Zero);
            EvaluateNode(node, Data(Roll1Conf), DiceErrorCode.DivideByZero);
        }

        [TestMethod]
        public void Successfully_EliminateParens_MultipleAdds()
        {
            var inner = new MathNode(MathOp.Add, One, One);
            var node = new MathNode(MathOp.Add, inner, One);
            EvaluateNode(node, Data(Roll1Conf), 0, "1 + 1 + 1 => 1 + 1 + 1 => 3");
        }

        [TestMethod]
        public void Successfully_EliminateParens_MultipleMultiply()
        {
            var inner = new MathNode(MathOp.Multiply, Two, Two);
            var node = new MathNode(MathOp.Multiply, inner, Two);
            EvaluateNode(node, Data(Roll1Conf), 0, "2 * 2 * 2 => 2 * 2 * 2 => 8");
        }

        [TestMethod]
        public void Successfully_OrderOfOperations_AddMultiply()
        {
            var inner = new MathNode(MathOp.Multiply, Two, Two);
            var node = new MathNode(MathOp.Add, One, inner);
            EvaluateNode(node, Data(Roll1Conf), 0, "1 + 2 * 2 => 1 + 2 * 2 => 5");
        }

        [TestMethod]
        public void Successfully_OrderOfOperations_Negate()
        {
            var inner = new MathNode(MathOp.Add, Two, Two);
            var node = new MathNode(MathOp.Negate, null, inner);
            EvaluateNode(node, Data(Roll1Conf), 0, "-(2 + 2) => -(2 + 2) => -4");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowArgumentNullException_WhenLeftNullAndNotUnary()
        {
            new MathNode(MathOp.Add, null, One);
        }

        [TestMethod]
        public void Successfully_DoUnaryWithLeftNull()
        {
            var node = new MathNode(MathOp.Negate, null, Two);
            EvaluateNode(node, Data(Roll1Conf), 0, "-2 => -2 => -2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowArgumentNullException_WhenRightNull()
        {
            new MathNode(MathOp.Add, One, null);
        }

        [TestMethod]
        public void Successfully_DontAddParens_SimpleGroup()
        {
            var group = new GroupNode(null, new List<DiceAST> { _2d20, _2d20 });
            var node = new MathNode(MathOp.Multiply, group, Two);
            EvaluateNode(node, Data(Roll9Conf), 4, "{2d20, 2d20} * 2 => (18 + 18) * 2 => 72");
        }

        [TestMethod]
        public void Successfully_AddParens_ComplexGroupMultiply()
        {
            var group = new GroupNode(Two, new List<DiceAST> { _2d20, _2d20 });
            var node = new MathNode(MathOp.Multiply, group, Two);
            EvaluateNode(node, Data(Roll9Conf), 8, "2{2d20, 2d20} * 2 => ((18 + 18) + (18 + 18)) * 2 => 144");
        }

        [TestMethod]
        public void Successfully_DontAddParens_ComplexGroupAdd()
        {
            var group = new GroupNode(Two, new List<DiceAST> { _2d20, _2d20 });
            var node = new MathNode(MathOp.Add, group, Two);
            EvaluateNode(node, Data(Roll9Conf), 8, "2{2d20, 2d20} + 2 => (18 + 18) + (18 + 18) + 2 => 74");
        }
    }
}
