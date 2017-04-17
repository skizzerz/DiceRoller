using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;

namespace TestDiceRoller.Grammar
{
    [TestClass]
    public class BasicRollShould : TestBase
    {
        [TestMethod]
        public void Successfully_RollOneDie_Normal()
        {
            EvaluateRoll("1d20", Roll9Conf, 1, "1d20 => 9 => 9");
        }

        [TestMethod]
        public void Successfully_RollTwoDice_Normal()
        {
            EvaluateRoll("2d20", Roll9Conf, 2, "2d20 => 9 + 9 => 18");
        }

        [TestMethod]
        public void Successfully_RollVariableDice_Normal()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(2, 4, 4, 4) };
            EvaluateRoll("(1d8)d6", conf, 4, "(1d8)d6 => 5 + 5 + 5 => 15");
        }

        [TestMethod]
        public void Successfully_DoMathOnly()
        {
            EvaluateRoll("2 + 1 * 3", Roll9Conf, 0, "2 + 1 * 3 => 2 + 1 * 3 => 5");
        }
    }
}
