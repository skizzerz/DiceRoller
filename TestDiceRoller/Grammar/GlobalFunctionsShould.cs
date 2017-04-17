using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;

namespace TestDiceRoller.Grammar
{
    [TestClass]
    public class GlobalFunctionsShould : TestBase
    {
        [TestMethod]
        public void Successfully_Floor()
        {
            EvaluateRoll("floor(1.5)", Roll9Conf, 0, "floor(1.5) => floor(1.5) => 1");
        }

        [TestMethod]
        public void Successfully_FloorMixedCase()
        {
            EvaluateRoll("FLooR(1.5)", Roll9Conf, 0, "floor(1.5) => floor(1.5) => 1");
        }

        [TestMethod]
        public void Successfully_Ceiling()
        {
            EvaluateRoll("ceil(1.5)", Roll9Conf, 0, "ceil(1.5) => ceil(1.5) => 2");
        }

        [TestMethod]
        public void Successfully_RoundBelowPoint5()
        {
            EvaluateRoll("round(1.4)", Roll9Conf, 0, "round(1.4) => round(1.4) => 1");
        }

        [TestMethod]
        public void Successfully_RoundAbovePoint5()
        {
            EvaluateRoll("round(1.6)", Roll9Conf, 0, "round(1.6) => round(1.6) => 2");
        }

        [TestMethod]
        public void Successfully_RoundAtPoint5()
        {
            EvaluateRoll("round(1.5)", Roll9Conf, 0, "round(1.5) => round(1.5) => 2");
        }

        [TestMethod]
        public void Successfully_Abs()
        {
            EvaluateRoll("abs(-0.5)", Roll9Conf, 0, "abs(-0.5) => abs(-0.5) => 0.5");
        }

        [TestMethod]
        public void Successfully_Max()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(4, 9) };
            EvaluateRoll("max(1d20, 1d20)", conf, 2, "max(1d20, 1d20) => max(5*, 10) => 10");
        }

        [TestMethod]
        public void Successfully_Min()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(4, 9) };
            EvaluateRoll("min(1d20, 1d20)", conf, 2, "min(1d20, 1d20) => min(5, 10*) => 5");
        }

        [TestMethod]
        public void ThrowsIncorrectArity_When0ArgFloor()
        {
            EvaluateRoll("floor()", Roll9Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowsIncorrectArity_When2ArgFloor()
        {
            EvaluateRoll("floor(1, 2)", Roll9Conf, DiceErrorCode.IncorrectArity);
        }
    }
}
