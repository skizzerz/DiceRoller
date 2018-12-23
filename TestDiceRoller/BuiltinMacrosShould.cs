using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;

namespace TestDiceRoller
{
    [TestClass]
    public class BuiltinMacrosShould : TestBase
    {
        [TestMethod]
        public void Successfully_NumDice_Standalone()
        {
            EvaluateRoll("5d4+[numDice]", Roll1Conf, 5, "5d4 + [numDice] => 1! + 1! + 1! + 1! + 1! + 5 => 10");
        }

        [TestMethod]
        public void Successfully_NumDice_NoDice()
        {
            EvaluateRoll("[numDice]", Roll1Conf, 0, "[numDice] => 0 => 0");
        }

        [TestMethod]
        public void Successfully_NumDice_Nested()
        {
            EvaluateRoll("1d20 + [numDice]d6", Roll1Conf, 2, "1d20 + 1d6 => 1! + 1! => 2");
        }
    }
}
