using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;

namespace TestDiceRoller.AST
{
    [TestClass]
    public class CritNodeShould : TestBase
    {
        private static RollerConfig CritConf => new RollerConfig() { GetRandomBytes = GetRNG(0, 1, 18, 19) };

        [TestMethod]
        public void Successfully_MarkCritsAndFumbles()
        {
            var node = new CritNode(equal19, equal2) { Expression = _4d20 };
            EvaluateNode(node, CritConf, 4, "4d20.critical(=19).fumble(=2) => 1 + 2! + 19! + 20 => 42");
        }

        [TestMethod]
        public void Successfully_MarkCrits_AndKeepFumblesSame()
        {
            var node = new CritNode(equal19, null) { Expression = _4d20 };
            EvaluateNode(node, CritConf, 4, "4d20.critical(=19) => 1! + 2 + 19! + 20 => 42");
        }

        [TestMethod]
        public void Successfully_MarkFumbles_AndKeepCritsSame()
        {
            var node = new CritNode(null, equal2) { Expression = _4d20 };
            EvaluateNode(node, CritConf, 4, "4d20.fumble(=2) => 1 + 2! + 19 + 20! => 42");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowArgumentException_WhenBothCritsAndFumblesNull()
        {
            new CritNode(null, null);
        }
    }
}
