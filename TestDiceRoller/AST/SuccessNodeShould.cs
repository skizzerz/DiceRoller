using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;

namespace TestDiceRoller.AST
{
    [TestClass]
    public class SuccessNodeShould : TestBase
    {
        private static RollerConfig SuccessConf => new RollerConfig() { GetRandomBytes = GetRNG(3, 4, 0, 5) };

        [TestMethod]
        public void Successfully_CountSuccesses()
        {
            var node = new SuccessNode(greaterEqual5, null) { Expression = _4d6 };
            EvaluateNode(node, SuccessConf, 4, "4d6.success(>=5) => 4 + $5 + 1! + $6! => 2 successes");
        }

        [TestMethod]
        public void Successfully_CountSuccessesAndFailures()
        {
            var node = new SuccessNode(greaterEqual5, equal1) { Expression = _4d6 };
            EvaluateNode(node, SuccessConf, 4, "4d6.success(>=5).failure(=1) => 4 + $5 + #1! + $6! => 1 success");
        }

        [TestMethod]
        public void ThrowInvalidSuccess_WhenFailuresButNoSuccessConditions()
        {
            var node = new SuccessNode(null, equal1) { Expression = _4d6 };
            EvaluateNode(node, SuccessConf, DiceErrorCode.InvalidSuccess);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowArgumentException_WhenBothSuccessAndFailureNull()
        {
            new SuccessNode(null, null);
        }

        [TestMethod]
        public void Successfully_IgnoreDroppedDice()
        {
            var drop = new KeepNode(KeepType.DropLow, One) { Expression = _4d6 };
            var node = new SuccessNode(greaterEqual5, equal1) { Expression = drop };
            EvaluateNode(node, SuccessConf, 4, "4d6.dropLowest(1).success(>=5).failure(=1) => 4 + $5 + 1!* + $6! => 2 successes");
        }
    }
}
