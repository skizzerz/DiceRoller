using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;

namespace TestDiceRoller.Grammar
{
    [TestClass]
    public class SortRollShould : TestBase
    {
        private static RollerConfig SortConf => new RollerConfig() { GetRandomBytes = GetRNG(5, 8, 2, 17) };

        [TestMethod]
        public void Successfully_SortAscendingExtra()
        {
            EvaluateRoll("4d20sa", SortConf, 4, "4d20.sortAsc() => 3 + 6 + 9 + 18 => 36");
        }

        [TestMethod]
        public void Successfully_SortAscendingFunction()
        {
            EvaluateRoll("4d20.sortAsc()", SortConf, 4, "4d20.sortAsc() => 3 + 6 + 9 + 18 => 36");
        }

        [TestMethod]
        public void Successfully_SortAscendingExtra_Fudge()
        {
            EvaluateRoll("4dF10sa", SortConf, 4, "4dF10.sortAsc() => -8 + -5 + -2 + 7 => -8");
        }

        [TestMethod]
        public void Successfully_SortAscendingFunction_Fudge()
        {
            EvaluateRoll("4dF10.sortAsc()", SortConf, 4, "4dF10.sortAsc() => -8 + -5 + -2 + 7 => -8");
        }

        [TestMethod]
        public void Successfully_SortAscendingExtra_Group()
        {
            EvaluateRoll("{2d20, 2d20}sa", SortConf, 4, "{2d20, 2d20}.sortAsc() => (15 + 21) => 36");
        }

        [TestMethod]
        public void Successfully_SortAscendingFunction_Group()
        {
            EvaluateRoll("{2d20, 2d20}.sortAsc()", SortConf, 4, "{2d20, 2d20}.sortAsc() => (15 + 21) => 36");
        }

        [TestMethod]
        public void Successfully_SortDescendingExtra()
        {
            EvaluateRoll("4d20sd", SortConf, 4, "4d20.sortDesc() => 18 + 9 + 6 + 3 => 36");
        }

        [TestMethod]
        public void Successfully_SortDescendingFunction()
        {
            EvaluateRoll("4d20.sortDesc()", SortConf, 4, "4d20.sortDesc() => 18 + 9 + 6 + 3 => 36");
        }

        [TestMethod]
        public void Successfully_SortDescendingExtra_Fudge()
        {
            EvaluateRoll("4dF10sd", SortConf, 4, "4dF10.sortDesc() => 7 + -2 + -5 + -8 => -8");
        }

        [TestMethod]
        public void Successfully_SortDescendingFunction_Fudge()
        {
            EvaluateRoll("4dF10.sortDesc()", SortConf, 4, "4dF10.sortDesc() => 7 + -2 + -5 + -8 => -8");
        }

        [TestMethod]
        public void Successfully_SortDescendingExtra_Group()
        {
            EvaluateRoll("{2d20, 2d20}sd", SortConf, 4, "{2d20, 2d20}.sortDesc() => (21 + 15) => 36");
        }

        [TestMethod]
        public void Successfully_SortDescendingFunction_Group()
        {
            EvaluateRoll("{2d20, 2d20}.sortDesc()", SortConf, 4, "{2d20, 2d20}.sortDesc() => (21 + 15) => 36");
        }

        [TestMethod]
        public void Successfully_SortAscendingWithNestedMath()
        {
            EvaluateRoll("{3+2-(5+4)+6*1}sa", SortConf, 0, "{3 + 2 - (5 + 4) + 6 * 1}.sortAsc() => (2 + 3 - (4 + 5) + 1 * 6) => 2");
        }
    }
}
