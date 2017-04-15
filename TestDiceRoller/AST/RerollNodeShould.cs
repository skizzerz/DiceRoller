using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;

namespace TestDiceRoller.AST
{
    [TestClass]
    public class RerollNodeShould : TestBase
    {
        private static RollerConfig RerollConf => new RollerConfig() { GetRandomBytes = GetRNG(0, 1, 0, 9) };

        [TestMethod]
        public void Successfully_Reroll_WhenConditionMet()
        {
            var node = new RerollNode(0, less5) { Expression = _1d20 };
            EvaluateNode(node, RerollConf, 4, "1d20.reroll(<5) => 1!* + 2* + 1!* + 10 => 10");
        }

        [TestMethod]
        public void Successfully_NotReroll_WhenConditionNotMet()
        {
            var node = new RerollNode(0, less5) { Expression = _1d20 };
            EvaluateNode(node, Roll9Conf, 1, "1d20.reroll(<5) => 9 => 9");
        }

        [TestMethod]
        public void Successfully_RerollOnce_WhenConditionMet()
        {
            var node = new RerollNode(1, less5) { Expression = _1d20 };
            EvaluateNode(node, RerollConf, 2, "1d20.rerollOnce(<5) => 1!* + 2 => 2");
        }

        [TestMethod]
        public void Successfully_RerollN_WhenConditionMet()
        {
            var node = new RerollNode(2, less5, Two) { Expression = _1d20 };
            EvaluateNode(node, RerollConf, 3, "1d20.rerollN(2, <5) => 1!* + 2* + 1! => 1");
        }
    }
}
