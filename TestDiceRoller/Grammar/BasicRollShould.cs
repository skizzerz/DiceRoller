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
        public void Successfully_RollOneDie_Normal_NumberOmitted()
        {
            EvaluateRoll("d20", Roll9Conf, 1, "1d20 => 9 => 9");
        }

        [TestMethod]
        public void Successfully_RollVariableSides_NumberOmitted()
        {
            EvaluateRoll("d(1d10)", Roll9Conf, 2, "1d9 => 9! => 9");
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
        public void Successfully_RollOneDie_StandardFudge_NumberOmitted()
        {
            EvaluateRoll("dF", Roll1Conf, 1, "1dF => -1! => -1");
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
            EvaluateRoll("(1d8)d6", conf, 4, "3d6 => 5 + 5 + 5 => 15");
        }

        [TestMethod]
        public void Successfully_RollVariableSides_Normal()
        {
            EvaluateRoll("1d(1d10)", Roll9Conf, 2, "1d9 => 9! => 9");
        }

        [TestMethod]
        public void Successfully_PerformVariableComparison_Normal()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 0, 1) };
            EvaluateRoll("1d10rr(1d10)", conf, 3, "1d10.reroll(=1) => 1!* + 2 => 2");
        }

        [TestMethod]
        public void Successfully_DoMathOnly()
        {
            EvaluateRoll("2 + 1 * 3", Roll9Conf, 0, "2 + 1 * 3 => 2 + 1 * 3 => 5");
        }

        [TestMethod]
        public void Successfully_MathUnary()
        {
            EvaluateRoll("-1--1", Roll20Conf, 0, "-1 - -1 => -1 - -1 => 0");
        }

        [TestMethod]
        public void Successfully_MathNestedUnary()
        {
            EvaluateRoll("-(-1)", Roll20Conf, 0, "-(-1) => -(-1) => 1");
        }

        [TestMethod]
        public void Successfully_RollGroup_OneElement()
        {
            EvaluateRoll("{4d20}", Roll9Conf, 4, "{4d20} => (36) => 36");
        }

        [TestMethod]
        public void Successfully_RollGroupTwice_OneElement()
        {
            EvaluateRoll("2{2d20}", Roll9Conf, 4, "2{2d20} => (18) + (18) => 36");
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

        [TestMethod]
        public void Successfully_GroupNumTimesExpr()
        {
            EvaluateRoll("(1d10){1d20}", Roll9Conf, 10, "9{1d20} => (9) + (9) + (9) + (9) + (9) + (9) + (9) + (9) + (9) => 81");
        }

        [TestMethod]
        public void Successfully_NoExtraneousParens_MathWithIf()
        {
            EvaluateRoll("if(0, =0, 2d8, 2{2d8}.expand())+4", Roll1Conf, 6, "if(0, =0, 2d8, 2{2d8}.expand()) + 4 => (1! + 1!) + 4 => 6");
            EvaluateRoll("if(1, =0, 2d8, 2{2d8}.expand())+4", Roll1Conf, 6, "if(1, =0, 2d8, 2{2d8}.expand()) + 4 => ((1! + 1!) + (1! + 1!)) + 4 => 8");
        }

        [TestMethod]
        public void Successfully_StripExtraneousParenthesisInExpression()
        {
            EvaluateRoll("(2d20)", Roll9Conf, 2, "2d20 => 9 + 9 => 18");
            EvaluateRoll("(if(0, =0, 2d8, 2{2d8}.expand()))+4", Roll1Conf, 6, "if(0, =0, 2d8, 2{2d8}.expand()) + 4 => (1! + 1!) + 4 => 6");
        }

        [TestMethod]
        public void ThrowRecursionDepthExceeded_WhenExceedingRecursion()
        {
            var conf = new RollerConfig() { MaxRecursionDepth = 0, GetRandomBytes = GetRNG(0, 2) };
            EvaluateRoll("1d20rr1", conf, DiceErrorCode.RecursionDepthExceeded);
        }

        [TestMethod]
        public void ThrowBadSides_WhenExceedingMaxSides()
        {
            var conf = new RollerConfig() { MaxSides = 100, GetRandomBytes = GetRNG(0, 2) };
            EvaluateRoll("1d1000", conf, DiceErrorCode.BadSides);
        }
    }
}
