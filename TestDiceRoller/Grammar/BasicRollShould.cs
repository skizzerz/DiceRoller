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
    }
}
