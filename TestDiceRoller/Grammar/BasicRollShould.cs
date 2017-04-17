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
        public void Successfully_RollOneDie_StandardFudge()
        {
            EvaluateRoll("1dF", Roll1Conf, 1, "1dF => -1! => -1");
        }

        [TestMethod]
        public void Successfully_RollTwoDice_StandardFudge()
        {
            EvaluateRoll("2dF", Roll1Conf, 2, "2dF => -1! + -1! => -2");
        }

        [TestMethod]
        public void Successfully_RollOneDie_NonStandardFudge()
        {
            EvaluateRoll("1dF20", Roll1Conf, 1, "1dF20 => -20! => -20");
        }

        [TestMethod]
        public void Successfully_RollTwoDice_NonStandardFudge()
        {
            EvaluateRoll("2dF20", Roll1Conf, 2, "2dF20 => -20! + -20! => -40");
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

        [TestMethod]
        public void Successfully_RollOneDie_Macro()
        {
            var conf = new RollerConfig() { ExecuteMacro = ExecuteMacro, GetRandomBytes = GetRNG(Roll9()) };
            EvaluateRoll("[one]d[twenty]", conf, 1, "[one]d[twenty] => 9 => 9");
        }

        [TestMethod]
        public void Successfully_RollGroup_OneElement()
        {
            EvaluateRoll("{4d20}", Roll9Conf, 4, "{4d20} => (9 + 9 + 9 + 9) => 36");
        }

        [TestMethod]
        public void Successfully_RollGroupTwice_OneElement()
        {
            EvaluateRoll("2{2d20}", Roll9Conf, 4, "2{2d20} => (9 + 9) + (9 + 9) => 36");
        }

        [TestMethod]
        public void Successfully_RollNestedGroup_OneElement()
        {
            EvaluateRoll("{{4d20}}", Roll9Conf, 4, "{{4d20}} => (36) => 36");
        }

        [TestMethod]
        public void Successfully_RollNestedGroupTwice_OneElement()
        {
            EvaluateRoll("{2{2d20}}", Roll9Conf, 4, "{2{2d20}} => (36) => 36");
        }

        [TestMethod]
        public void Successfully_RollTwiceNestedGroup_OneElement()
        {
            EvaluateRoll("2{{2d20}}", Roll9Conf, 4, "2{{2d20}} => (18) + (18) => 36");
        }

        [TestMethod]
        public void Successfully_RollGroup_TwoElements()
        {
            EvaluateRoll("{2d20,2d20}", Roll9Conf, 4, "{2d20, 2d20} => (18 + 18) => 36");
        }

        [TestMethod]
        public void Successfully_RollGroupTwice_TwoElements()
        {
            EvaluateRoll("2{2d20, 2d20}", Roll9Conf, 8, "2{2d20, 2d20} => (18 + 18) + (18 + 18) => 72");
        }

        [TestMethod]
        public void Successfully_RollNestedGroup_TwoElements()
        {
            EvaluateRoll("{{2d20, 2d20}}", Roll9Conf, 4, "{{2d20, 2d20}} => (36) => 36");
        }

        [TestMethod]
        public void Successfully_RollNestedGroupTwice_TwoElements()
        {
            EvaluateRoll("{2{2d20, 2d20}}", Roll9Conf, 8, "{2{2d20, 2d20}} => (72) => 72");
        }
    }
}
