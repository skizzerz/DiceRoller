using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;

namespace TestDiceRoller.Grammar
{
    [TestClass]
    public class RerollRollShould : TestBase
    {
        private static RollerConfig RerollConf => new RollerConfig() { GetRandomBytes = GetRNG(0, 1, 0, 9) };
        private static RollerConfig GroupRerollConf => new RollerConfig() { GetRandomBytes = GetRNG(0, 4, 5, 4, 1, 2, 1, 1, 3, 3) };

        [TestMethod]
        public void Successfully_RerollExtra()
        {
            EvaluateRoll("1d20rr<=2", RerollConf, 4, "1d20.reroll(<=2) => 1!* + 2* + 1!* + 10 => 10");
        }

        [TestMethod]
        public void Successfully_RerollFunction()
        {
            EvaluateRoll("1d20.reroll(<=2)", RerollConf, 4, "1d20.reroll(<=2) => 1!* + 2* + 1!* + 10 => 10");
        }

        [TestMethod]
        public void Successfully_RerollExtra_Fudge()
        {
            EvaluateRoll("1dFrr<0", RerollConf, 2, "1dF.reroll(<0) => -1!* + 0 => 0");
        }

        [TestMethod]
        public void Successfully_RerollFunction_Fudge()
        {
            EvaluateRoll("1dF.reroll(<0)", RerollConf, 2, "1dF.reroll(<0) => -1!* + 0 => 0");
        }

        [TestMethod]
        public void Successfully_RerollExtra_Group()
        {
            EvaluateRoll("2{{2d6}}rr<7", GroupRerollConf, 10, "2{{2d6}}.reroll(<7) => (6!* + 5* + 4* + 8) + (11!) => 19");
        }

        [TestMethod]
        public void Successfully_RerollFunction_Group()
        {
            EvaluateRoll("2{{2d6}}.reroll(<7)", GroupRerollConf, 10, "2{{2d6}}.reroll(<7) => (6!* + 5* + 4* + 8) + (11!) => 19");
        }

        [TestMethod]
        public void Successfully_RerollTwoConditionsExtra()
        {
            EvaluateRoll("1d20rr1rr2", RerollConf, 4, "1d20.reroll(=1, =2) => 1!* + 2* + 1!* + 10 => 10");
        }

        [TestMethod]
        public void Successfully_RerollTwoConditionsFunction()
        {
            EvaluateRoll("1d20.reroll(=1, =2)", RerollConf, 4, "1d20.reroll(=1, =2) => 1!* + 2* + 1!* + 10 => 10");
        }

        [TestMethod]
        public void Successfully_RerollOnceExtra()
        {
            EvaluateRoll("1d20ro<=2", RerollConf, 2, "1d20.rerollOnce(<=2) => 1!* + 2 => 2");
        }

        [TestMethod]
        public void Successfully_RerollOnceExtra_Group()
        {
            EvaluateRoll("2{{2d6}}ro<7", GroupRerollConf, 6, "2{{2d6}}.rerollOnce(<7) => (6!* + 5) + (11!) => 16");
        }

        [TestMethod]
        public void Successfully_RerollOnceFunction_Group()
        {
            EvaluateRoll("2{{2d6}}.rerollOnce(<7)", GroupRerollConf, 6, "2{{2d6}}.rerollOnce(<7) => (6!* + 5) + (11!) => 16");
        }

        [TestMethod]
        public void Successfully_RerollOnceFunction()
        {
            EvaluateRoll("1d20.rerollOnce(<=2)", RerollConf, 2, "1d20.rerollOnce(<=2) => 1!* + 2 => 2");
        }

        [TestMethod]
        public void Successfully_RerollOnceTwoConditionsExtra()
        {
            EvaluateRoll("1d20ro1ro2", RerollConf, 2, "1d20.rerollOnce(=1, =2) => 1!* + 2 => 2");
        }

        [TestMethod]
        public void Successfully_RerollOnceTwoConditionsFunction()
        {
            EvaluateRoll("1d20.rerollOnce(=1,=2)", RerollConf, 2, "1d20.rerollOnce(=1, =2) => 1!* + 2 => 2");
        }

        [TestMethod]
        public void Successfully_RerollNFunction()
        {
            EvaluateRoll("1d20.rerollN(2, <=2)", RerollConf, 3, "1d20.rerollN(2, <=2) => 1!* + 2* + 1! => 1");
        }

        [TestMethod]
        public void Successfully_RerollNFunction_Group()
        {
            EvaluateRoll("2{{2d6}}.rerollN(2, <7)", GroupRerollConf, 8, "2{{2d6}}.rerollN(2, <7) => (6!* + 5* + 4) + (11!) => 15");
        }

        [TestMethod]
        public void Successfully_RerollNTwoConditionsFunction()
        {
            EvaluateRoll("1d20.rerollN(2, =1, =2)", RerollConf, 3, "1d20.rerollN(2, =1, =2) => 1!* + 2* + 1! => 1");
        }

        [TestMethod]
        public void Successfully_TerminateTooManyRerolls()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(Roll9()), MaxRerolls = 2 };
            EvaluateRoll("1d20rr!=2", conf, 3, "1d20.reroll(!=2) => 9* + 9* + 9 => 9");
        }

        [TestMethod]
        public void ThrowTooManyDice_WhenTooManyDice()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(Roll9()), MaxDice = 2 };
            EvaluateRoll("1d20rr<>2", conf, DiceErrorCode.TooManyDice);
        }

        [TestMethod]
        public void ThrowMixedReroll_WhenMixingRerollTypes()
        {
            EvaluateRoll("1d20rr1ro2", Roll9Conf, DiceErrorCode.MixedReroll);
        }

        [TestMethod]
        public void ThrowBadRerollCount_WhenNegativeRerollCount()
        {
            EvaluateRoll("1d20.rerollN(-2, >=1)", Roll9Conf, DiceErrorCode.BadRerollCount);
        }

        [TestMethod]
        public void ThrowBadRerollCount_WhenNonNumberRerollCount()
        {
            EvaluateRoll("1d20.rerollN(1d6, >=1)", Roll9Conf, DiceErrorCode.BadRerollCount);
        }

        [TestMethod]
        public void ThrowParseError_WhenIncompleteRerollExtra()
        {
            EvaluateRoll("3d6rr", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowParseError_WhenIncompleteRerollOnceExtra()
        {
            EvaluateRoll("3d6ro", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowIncorrectArity_WhenEmptyArgsReroll_Basic()
        {
            EvaluateRoll("1d20.reroll()", Roll20Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowIncorrectArity_WhenEmptyArgsReroll_Group()
        {
            EvaluateRoll("{1d20}.reroll()", Roll20Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowIncorrectArity_WhenEmptyArgsRerollN_Basic()
        {
            EvaluateRoll("1d20.rerollN()", Roll20Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowIncorrectArity_WhenOneArgRerollN_Basic()
        {
            EvaluateRoll("1d20.rerollN(2)", Roll20Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowIncorrectArgType_WhenNonComparisonArgRerollN_Basic()
        {
            EvaluateRoll("1d20.rerollN(1, 2)", Roll20Conf, DiceErrorCode.IncorrectArgType);
        }

        [TestMethod]
        public void ThrowIncorrectArity_WhenEmptyArgsRerollN_Group()
        {
            EvaluateRoll("{1d20}.rerollN()", Roll20Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowIncorrectArity_WhenOneArgRerollN_Group()
        {
            EvaluateRoll("{1d20}.rerollN(2)", Roll20Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowIncorrectArgType_WhenNonComparisonArgRerollN_Group()
        {
            EvaluateRoll("{1d20}.rerollN(1, 2)", Roll20Conf, DiceErrorCode.IncorrectArgType);
        }
    }
}
