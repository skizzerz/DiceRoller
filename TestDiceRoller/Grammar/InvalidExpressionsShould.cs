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
        public void ThrowNegativeDice_WhenRollingNegativeDice()
        {
            EvaluateRoll("(1d8-2)d6", Roll1Conf, DiceErrorCode.NegativeDice);
        }

        [TestMethod]
        public void ThrowDivideByZero_WhenDividingByZero()
        {
            EvaluateRoll("2/(1d6-1)*3", Roll1Conf, DiceErrorCode.DivideByZero);
        }
    }
}
