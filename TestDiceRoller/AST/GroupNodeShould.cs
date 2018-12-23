using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;

namespace TestDiceRoller.AST
{
    [TestClass]
    public class GroupNodeShould : TestBase
    {
        [TestMethod]
        public void Successfully_EvaluateOnceIfNumTimesIsNull()
        {
            var node = new GroupNode(null, new List<DiceAST> { _1d20, _2d20, _4d20 });
            EvaluateNode(node, Data(Roll9Conf), 7, "{1d20, 2d20, 4d20} => (9 + 18 + 36) => 63");
        }

        [TestMethod]
        public void Successfully_EvaluateZeroTimes()
        {
            var node = new GroupNode(Zero, new List<DiceAST> { _1d20 });
            EvaluateNode(node, Data(Roll9Conf), 0, "0{1d20} => 0 => 0");
        }

        [TestMethod]
        public void Successfully_EvaluateTwice()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(2, 3, 4, 5, 6, 7) };
            var node = new GroupNode(Two, new List<DiceAST> { _2d20 });
            EvaluateNode(node, Data(conf), 4, "2{2d20} => (7) + (11) => 18");
        }

        [TestMethod]
        public void Successfully_EvaluateTwice_Expand()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(2, 3, 4, 5, 6, 7) };
            var data = Data(conf);
            var group = new GroupNode(Two, new List<DiceAST> { _2d20 });
            var node = new FunctionNode(FunctionScope.Group, "expand", new DiceAST[0], data);
            node.Context.Expression = group;
            EvaluateNode(node, data, 4, "2{2d20}.expand() => (3 + 4) + (5 + 6) => 18");
        }

        [TestMethod]
        public void Successfully_ListAllValuesForMultipleExprs_Expand()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(2, 3, 4, 5, 6, 7, 8) };
            var data = Data(conf);
            var group = new GroupNode(null, new List<DiceAST> { _1d20, _2d20, _4d20 });
            var node = new FunctionNode(FunctionScope.Group, "expand", new DiceAST[0], data);
            node.Context.Expression = group;
            EvaluateNode(node, data, 7, "{1d20, 2d20, 4d20}.expand() => ((3) + (4 + 5) + (6 + 7 + 8 + 9)) => 42");
        }

        [TestMethod]
        public void Successfully_ListAllValuesForSingleExpr_Expand()
        {
            var data = Data(Roll9Conf);
            var group = new GroupNode(null, new List<DiceAST> { _2d20 });
            var node = new FunctionNode(FunctionScope.Group, "expand", new DiceAST[0], data);
            node.Context.Expression = group;
            EvaluateNode(node, data, 2, "{2d20}.expand() => (9 + 9) => 18");
        }

        [TestMethod]
        public void Successfully_HandleNestedGroupExpr()
        {
            var inner = new GroupNode(null, new List<DiceAST> { _2d20 });
            var node = new GroupNode(null, new List<DiceAST> { inner });
            EvaluateNode(node, Data(Roll9Conf), 2, "{{2d20}} => (18) => 18");
        }

        [TestMethod]
        public void Successfully_DontRerollNestedRolls()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(2, 2, 2, 2, 3, 3, 3, 3, 3) };
            var roll = new RollNode(RollType.Normal, _1d8, Six);
            var node = new GroupNode(Two, new List<DiceAST> { roll });
            EvaluateNode(node, Data(conf), 7, "2{3d6} => (9) + (12) => 21");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowArgumentNullException_WhenExprsIsNull()
        {
            new GroupNode(One, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowArgumentException_WhenExprsIsEmpty()
        {
            new GroupNode(One, new List<DiceAST>());
        }
    }
}
