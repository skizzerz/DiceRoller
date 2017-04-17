using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;

namespace TestDiceRoller.Grammar
{
    [TestClass]
    public class CritRollShould : TestBase
    {
        private static RollerConfig CritConf => new RollerConfig() { GetRandomBytes = GetRNG(0, 1, 18, 19) };

        [TestMethod]
        public void Successfully_CritFumbleExtra()
        {
            EvaluateRoll("4d20cs19f2", CritConf, 4, "4d20.critical(=19).fumble(=2) => 1 + 2! + 19! + 20 => 42");
        }

        [TestMethod]
        public void Successfully_CritFumbleExtraSeparate()
        {
            EvaluateRoll("4d20cs19cf2", CritConf, 4, "4d20.critical(=19).fumble(=2) => 1 + 2! + 19! + 20 => 42");
        }

        [TestMethod]
        public void Successfully_CritFumbleFunction()
        {
            EvaluateRoll("4d20.critical(=19).fumble(=2)", CritConf, 4, "4d20.critical(=19).fumble(=2) => 1 + 2! + 19! + 20 => 42");
        }

        [TestMethod]
        public void Successfully_CritExtra()
        {
            EvaluateRoll("4d20cs19", CritConf, 4, "4d20.critical(=19) => 1! + 2 + 19! + 20 => 42");
        }

        [TestMethod]
        public void Successfully_CritFunction()
        {
            EvaluateRoll("4d20.critical(=19)", CritConf, 4, "4d20.critical(=19) => 1! + 2 + 19! + 20 => 42");
        }

        [TestMethod]
        public void Successfully_FumbleExtra()
        {
            EvaluateRoll("4d20cf2", CritConf, 4, "4d20.fumble(=2) => 1 + 2! + 19 + 20! => 42");
        }

        [TestMethod]
        public void Successfully_FumbleFunction()
        {
            EvaluateRoll("4d20.fumble(=2)", CritConf, 4, "4d20.fumble(=2) => 1 + 2! + 19 + 20! => 42");
        }
    }
}
