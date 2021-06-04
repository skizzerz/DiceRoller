using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;

namespace TestDiceRoller.AST
{
    [TestClass]
    public class SortShould : TestBase
    {
        private static RollerConfig SortConf => new RollerConfig() { GetRandomBytes = GetRNG(5, 8, 2, 17) };

        private enum SortType
        {
            SortAsc,
            SortDesc
        }

        static (RollData, DiceAST) GetSort(SortType type, RollerConfig config, DiceAST expression, DiceAST argument = null)
        {
            var arguments = new List<DiceAST>();
            if (argument != null)
            {
                arguments.Add(argument);
            }

            var data = Data(config);
            var node = new FunctionNode(FunctionScope.Basic, type.ToString(), arguments, data);
            node.Context.Expression = expression;

            return (data, node);
        }

        [TestMethod]
        public void Successfully_SortAscending()
        {
            var (data, node) = GetSort(SortType.SortAsc, SortConf, _4d20);
            EvaluateNode(node, data, 4, "4d20.sortAsc() => 3 + 6 + 9 + 18 => 36");
        }

        [TestMethod]
        public void Successfully_SortDescending()
        {
            var (data, node) = GetSort(SortType.SortDesc, SortConf, _4d20);
            EvaluateNode(node, data, 4, "4d20.sortDesc() => 18 + 9 + 6 + 3 => 36");
        }

        [TestMethod]
        public void Successfully_SortAscendingWithNestedMath()
        {
            var data = Data(SortConf);
            var inner1 = new MathNode(MathOp.Add, Three, Two);
            var inner2 = new MathNode(MathOp.Add, Five, Four);
            var sub = new MathNode(MathOp.Subtract, inner1, inner2);
            var inner3 = new MathNode(MathOp.Multiply, Six, One);
            var add = new MathNode(MathOp.Add, sub, inner3);
            var group = new GroupNode(null, new List<DiceAST> { add });
            var func = new FunctionNode(FunctionScope.Group, "expand", new DiceAST[0], data);
            func.Context.Expression = group;
            var (_, node) = GetSort(SortType.SortAsc, SortConf, func);
            EvaluateNode(node, data, 0, "{3 + 2 - (5 + 4) + 6 * 1}.expand().sortAsc() => (2 + 3 - (4 + 5) + 1 * 6) => 2");
        }
    }
}
