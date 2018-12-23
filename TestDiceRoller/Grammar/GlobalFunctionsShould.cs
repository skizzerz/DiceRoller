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
        public void Successfully_Max_BiggerRight()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(4, 9) };
            EvaluateRoll("max(1d20, 1d20)", conf, 2, "max(1d20, 1d20) => max(5*, 10) => 10");
        }

        [TestMethod]
        public void Successfully_Max_BiggerLeft()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(9, 4) };
            EvaluateRoll("max(1d20, 1d20)", conf, 2, "max(1d20, 1d20) => max(10, 5*) => 10");
        }

        [TestMethod]
        public void Successfully_Max_Successes()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(9, 4, 5, 5) };
            EvaluateRoll("max(2d10>5, 2d10>5)", conf, 4, "max(2d10.success(>5), 2d10.success(>5)) => max(10* + 5*, $6 + $6) => 2 successes");
        }

        [TestMethod]
        public void Successfully_Min_BiggerRight()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(4, 9) };
            EvaluateRoll("min(1d20, 1d20)", conf, 2, "min(1d20, 1d20) => min(5, 10*) => 5");
        }

        [TestMethod]
        public void Successfully_Min_BiggerLeft()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(9, 4) };
            EvaluateRoll("min(1d20, 1d20)", conf, 2, "min(1d20, 1d20) => min(10*, 5) => 5");
        }

        [TestMethod]
        public void Successfully_Min_Successes()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 7, 5, 5) };
            EvaluateRoll("min(2d10>5f1, 2d10>5f1)", conf, 4, "min(2d10.success(>5).failure(=1), 2d10.success(>5).failure(=1)) => min(#1 + $8, 6* + 6*) => 0 successes");
        }

        [TestMethod]
        public void Successfully_MinMax_BiggerLeft()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(17) };
            EvaluateRoll("min(max(1d20, 10), 9)", conf, 1, "min(max(1d20, 10), 9) => min(max(18*, 10*), 9) => 9");
        }

        [TestMethod]
        public void Successfully_If_Success_NoElse()
        {
            EvaluateRoll("if(1, =1, 2)", Roll9Conf, 0, "if(1, =1, 2) => (2) => 2");
        }

        [TestMethod]
        public void Successfully_If_Failure_NoElse()
        {
            EvaluateRoll("if(2, =1, 2)", Roll9Conf, 0, "if(2, =1, 2) => (0) => 0");
        }

        [TestMethod]
        public void Successfully_If_Success_WithElse()
        {
            EvaluateRoll("if(1, =1, 2, 3)", Roll9Conf, 0, "if(1, =1, 2, 3) => (2) => 2");
        }

        [TestMethod]
        public void Successfully_If_Failure_WithElse()
        {
            EvaluateRoll("if(2, =1, 2, 3)", Roll9Conf, 0, "if(2, =1, 2, 3) => (3) => 3");
        }

        [TestMethod]
        public void Successfully_ParticipateInMath()
        {
            EvaluateRoll("-max(1, 2) + 6", Roll9Conf, 0, "-max(1, 2) + 6 => -max(1*, 2) + 6 => 4");
        }

        [TestMethod]
        public void Successfully_ParticipateInMath_Nested()
        {
            EvaluateRoll("{-max(1, 2)} + 6", Roll9Conf, 0, "{-max(1, 2)} + 6 => (-2) + 6 => 4");
        }

        [TestMethod]
        public void ThrowsParseError_WhenInRoll_NumDice()
        {
            EvaluateRoll("max(1, 2)d20", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowsParseError_WhenInRoll_NumSides()
        {
            EvaluateRoll("1dmax(10, 20)", Roll9Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void Successfully_NestedFunction_WhenInRoll_NumDice()
        {
            EvaluateRoll("(max(1, 2))d20", Roll9Conf, 2, "2d20 => 9 + 9 => 18");
        }

        [TestMethod]
        public void Successfully_NestedFunction_WhenInRoll_NumSides()
        {
            EvaluateRoll("1d(max(5, 20))", Roll9Conf, 1, "1d20 => 9 => 9");
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

        [TestMethod]
        public void ThrowsIncorrectArgType_WhenComparisonArgFloor()
        {
            EvaluateRoll("floor(=1)", Roll9Conf, DiceErrorCode.IncorrectArgType);
        }

        [TestMethod]
        public void ThrowsIncorrectArity_When0ArgCeiling()
        {
            EvaluateRoll("ceil()", Roll9Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowsIncorrectArity_When2ArgCeiling()
        {
            EvaluateRoll("ceil(1, 2)", Roll9Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowsIncorrectArgType_WhenComparisonArgCeiling()
        {
            EvaluateRoll("ceil(=1)", Roll9Conf, DiceErrorCode.IncorrectArgType);
        }

        [TestMethod]
        public void ThrowsIncorrectArity_When0ArgRound()
        {
            EvaluateRoll("round()", Roll9Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowsIncorrectArity_When2ArgRound()
        {
            EvaluateRoll("round(1, 2)", Roll9Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowsIncorrectArgType_WhenComparisonArgRound()
        {
            EvaluateRoll("round(=1)", Roll9Conf, DiceErrorCode.IncorrectArgType);
        }

        [TestMethod]
        public void ThrowsIncorrectArity_When0ArgAbs()
        {
            EvaluateRoll("abs()", Roll9Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowsIncorrectArity_When2ArgAbs()
        {
            EvaluateRoll("abs(1, 2)", Roll9Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowsIncorrectArgType_WhenComparisonArgAbs()
        {
            EvaluateRoll("abs(=1)", Roll9Conf, DiceErrorCode.IncorrectArgType);
        }

        [TestMethod]
        public void ThrowsIncorrectArity_When1ArgMax()
        {
            EvaluateRoll("max(1d20)", Roll9Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowsIncorrectArity_When3ArgMax()
        {
            EvaluateRoll("max(1d20, 1d20, 1d20)", Roll9Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowsIncorrectArgType_WhenComparisonArg1Max()
        {
            EvaluateRoll("max(=1, 1d20)", Roll9Conf, DiceErrorCode.IncorrectArgType);
        }

        [TestMethod]
        public void ThrowsIncorrectArgType_WhenComparisonArg2Max()
        {
            EvaluateRoll("max(1d20, =1)", Roll9Conf, DiceErrorCode.IncorrectArgType);
        }

        [TestMethod]
        public void ThrowsIncorrectArity_When1ArgMin()
        {
            EvaluateRoll("min(1d20)", Roll9Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowsIncorrectArity_When3ArgMin()
        {
            EvaluateRoll("min(1d20, 1d20, 1d20)", Roll9Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowsIncorrectArgType_WhenComparisonArg1Min()
        {
            EvaluateRoll("min(=1, 1d20)", Roll9Conf, DiceErrorCode.IncorrectArgType);
        }

        [TestMethod]
        public void ThrowsIncorrectArgType_WhenComparisonArg2Min()
        {
            EvaluateRoll("min(1d20, =1)", Roll9Conf, DiceErrorCode.IncorrectArgType);
        }

        [TestMethod]
        public void ThrowsIncorrectArity_When2ArgIf()
        {
            EvaluateRoll("if(1d20, =9)", Roll9Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowsIncorrectArity_When5ArgMin()
        {
            EvaluateRoll("if(1d20, =9, 5, 6, 7)", Roll9Conf, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowsIncorrectArgType_WhenComparisonArg1If()
        {
            EvaluateRoll("if(=1, =2, 3)", Roll9Conf, DiceErrorCode.IncorrectArgType);
        }

        [TestMethod]
        public void ThrowsIncorrectArgType_WhenNotComparisonArg2If()
        {
            EvaluateRoll("if(1d20, 2, 3)", Roll9Conf, DiceErrorCode.IncorrectArgType);
        }

        [TestMethod]
        public void ThrowsIncorrectArgType_WhenComparisonArg3If()
        {
            EvaluateRoll("if(1d20, =2, =3)", Roll9Conf, DiceErrorCode.IncorrectArgType);
        }

        [TestMethod]
        public void ThrowsIncorrectArgType_WhenComparisonArg4If()
        {
            EvaluateRoll("if(1d20, =2, 3, =4)", Roll9Conf, DiceErrorCode.IncorrectArgType);
        }
    }
}
