using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;

namespace TestDiceRoller.Grammar
{
    [TestClass]
    public class InvalidExpressionsShould : TestBase
    {
        [TestMethod]
        public void ThrowParseError_WhenIncompleteBasicRoll()
        {
            EvaluateRoll("3d", Roll9Conf, DiceErrorCode.ParseError);
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
        public void ThrowParseError_WhenIncompleteKeepHighestExtra()
        {
            EvaluateRoll("3d6kh", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowParseError_WhenIncompleteKeepLowestExtra()
        {
            EvaluateRoll("3d6kl", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowParseError_WhenIncompleteDropHighestExtra()
        {
            EvaluateRoll("3d6dh", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowParseError_WhenIncompleteDropLowestExtra()
        {
            EvaluateRoll("3d6dl", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowParseError_WhenIncompleteCriticalExtra()
        {
            EvaluateRoll("3d6cs", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowParseError_WhenIncompleteFumbleExtra()
        {
            EvaluateRoll("3d6cf", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowParseError_WhenIncompleteCriticalFumbleExtra()
        {
            EvaluateRoll("3d6cs>=5f", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowParseError_WhenIncompleteFunction1()
        {
            EvaluateRoll("3d6.fun", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowParseError_WhenIncompleteFunction2()
        {
            EvaluateRoll("3d6.fun(", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowParseError_WhenIncompleteFunction3()
        {
            EvaluateRoll("fun", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowParseError_WhenIncompleteFunction4()
        {
            EvaluateRoll("fun(", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowParseError_WhenIncompleteGroup()
        {
            EvaluateRoll("{3d6, 4+2", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowParseError_WhenExtraAfterFunction()
        {
            EvaluateRoll("1d20.reroll()sa", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowNoAdvantageKeep_WhenMixingKeepAndAdvantage()
        {
            EvaluateRoll("1d20adkh1", Roll9Conf, DiceErrorCode.NoAdvantageKeep);
        }

        [TestMethod]
        public void ThrowNegativeDice_WhenRollingNegativeDice()
        {
            EvaluateRoll("(1d8-2)d6", Roll1Conf, DiceErrorCode.NegativeDice);
        }
    }
}
