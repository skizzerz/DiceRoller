using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;

namespace TestDiceRoller.AST
{
    [TestClass]
    public class SortNodeShould : TestBase
    {
        private static RollerConfig SortConf => new RollerConfig() { GetRandomBytes = GetRNG(5, 8, 2, 17) };

        [TestMethod]
        public void Successfully_SortAscending()
        {
            var node = new SortNode(SortDirection.Ascending) { Expression = _4d20 };
            EvaluateNode(node, Data(SortConf), 4, "4d20.sortAsc() => 3 + 6 + 9 + 18 => 36");
        }

        [TestMethod]
        public void Successfully_SortDescending()
        {
            var node = new SortNode(SortDirection.Descending) { Expression = _4d20 };
            EvaluateNode(node, Data(SortConf), 4, "4d20.sortDesc() => 18 + 9 + 6 + 3 => 36");
        }

        [TestMethod]
        public void Successfully_SortAscendingWithNestedMath()
        {
            var inner1 = new MathNode(MathOp.Add, Three, Two);
            var inner2 = new MathNode(MathOp.Add, Five, Four);
            var sub = new MathNode(MathOp.Subtract, inner1, inner2);
            var inner3 = new MathNode(MathOp.Multiply, Six, One);
            var add = new MathNode(MathOp.Add, sub, inner3);
            var group = new GroupNode(null, new List<DiceAST> { add });
            var node = new SortNode(SortDirection.Ascending) { Expression = group };
            EvaluateNode(node, Data(SortConf), 0, "{3 + 2 - (5 + 4) + 6 * 1}.sortAsc() => (2 + 3 - (4 + 5) + 1 * 6) => 2");
        }
    }
}
