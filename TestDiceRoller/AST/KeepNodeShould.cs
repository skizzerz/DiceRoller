using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;

namespace TestDiceRoller.AST
{
    [TestClass]
    public class KeepNodeShould : TestBase
    {
        private static RollerConfig StatConf => new RollerConfig() { GetRandomBytes = GetRNG(4, 2, 5, 0) };
        private static RollerConfig AdvantageConf => new RollerConfig() { GetRandomBytes = GetRNG(1, 18) };

        [TestMethod]
        public void Successfully_DropHighest()
        {
            var node = new KeepNode(KeepType.DropHigh, One) { Expression = _4d6 };
            EvaluateNode(node, Data(StatConf), 4, "4d6.dropHighest(1) => 5 + 3 + 6!* + 1! => 9");
        }

        [TestMethod]
        public void Successfully_DropLowest()
        {
            var node = new KeepNode(KeepType.DropLow, One) { Expression = _4d6 };
            EvaluateNode(node, Data(StatConf), 4, "4d6.dropLowest(1) => 5 + 3 + 6! + 1!* => 14");
        }

        [TestMethod]
        public void Successfully_KeepHighest()
        {
            var node = new KeepNode(KeepType.KeepHigh, One) { Expression = _4d6 };
            EvaluateNode(node, Data(StatConf), 4, "4d6.keepHighest(1) => 5* + 3* + 6! + 1!* => 6");
        }

        [TestMethod]
        public void Successfully_KeepLowest()
        {
            var node = new KeepNode(KeepType.KeepLow, One) { Expression = _4d6 };
            EvaluateNode(node, Data(StatConf), 4, "4d6.keepLowest(1) => 5* + 3* + 6!* + 1! => 1");
        }

        [TestMethod]
        public void Successfully_ApplyAdvantage()
        {
            var node = new KeepNode(KeepType.Advantage, null) { Expression = _1d20 };
            EvaluateNode(node, Data(AdvantageConf), 2, "1d20.advantage() => 2* + 19 => 19");
        }

        [TestMethod]
        public void Successfully_ApplyDisadvantage()
        {
            var node = new KeepNode(KeepType.Disadvantage, null) { Expression = _1d20 };
            EvaluateNode(node, Data(AdvantageConf), 2, "1d20.disadvantage() => 2 + 19* => 2");
        }

        [TestMethod]
        public void ThrowNegativeDice_WhenKeepingNegativeDice()
        {
            var node = new KeepNode(KeepType.KeepHigh, MinusOne) { Expression = _4d6 };
            EvaluateNode(node, Data(StatConf), DiceErrorCode.NegativeDice);
        }

        [TestMethod]
        public void CountSuccesses_WhenKeepingSuccessDice()
        {
            var success = new SuccessNode(greaterEqual5, equal1) { Expression = _4d6 };
            var node = new KeepNode(KeepType.DropLow, One) { Expression = success };
            EvaluateNode(node, Data(StatConf), 4, "4d6.success(>=5).failure(=1).dropLowest(1) => $5 + 3 + $6! + 1!* => 2 successes");
        }

        [TestMethod]
        public void Successfully_AllowDroppingMoreDiceThanExist()
        {
            var node = new KeepNode(KeepType.DropHigh, Five) { Expression = _4d6 };
            EvaluateNode(node, Data(StatConf), 4, "4d6.dropHighest(5) => 5* + 3* + 6!* + 1!* => 0");
        }

        [TestMethod]
        public void Successfully_AllowKeepingZeroDice()
        {
            var node = new KeepNode(KeepType.KeepHigh, Zero) { Expression = _4d6 };
            EvaluateNode(node, Data(StatConf), 4, "4d6.keepHighest(0) => 5* + 3* + 6!* + 1!* => 0");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowArgumentNullException_WhenAmountIsNullAndNotAdvantage()
        {
            new KeepNode(KeepType.KeepHigh, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowArgumentException_WhenAmountNotNullAndIsAdvantage()
        {
            new KeepNode(KeepType.Advantage, One);
        }
    }
}
