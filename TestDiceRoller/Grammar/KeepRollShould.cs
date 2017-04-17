using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;

namespace TestDiceRoller.Grammar
{
    [TestClass]
    public class KeepRollShould : TestBase
    {
        [TestMethod]
        public void Successfully_DropLowestExtra()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 4, 2) };
            EvaluateRoll("3d6dl1 + 6", conf, 3, "3d6.dropLowest(1) + 6 => 1!* + 5 + 3 + 6 => 14");
        }

        [TestMethod]
        public void Successfully_DropLowestFunction()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 4, 2) };
            EvaluateRoll("3d6.dropLowest(1) + 6", conf, 3, "3d6.dropLowest(1) + 6 => 1!* + 5 + 3 + 6 => 14");
        }

        [TestMethod]
        public void Successfully_DropLowestTwice()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 4, 2) };
            EvaluateRoll("3d6dl1.dropLowest(1) + 6", conf, 3, "3d6.dropLowest(1).dropLowest(1) + 6 => 1!* + 5 + 3* + 6 => 11");
        }

        [TestMethod]
        public void Successfully_DropHighestExtra()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 4, 2) };
            EvaluateRoll("3d6dh1 + 6", conf, 3, "3d6.dropHighest(1) + 6 => 1! + 5* + 3 + 6 => 10");
        }

        [TestMethod]
        public void Successfully_DropHighestFunction()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 4, 2) };
            EvaluateRoll("3d6.dropHighest(1) + 6", conf, 3, "3d6.dropHighest(1) + 6 => 1! + 5* + 3 + 6 => 10");
        }

        [TestMethod]
        public void Successfully_DropHighestTwice()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 4, 2) };
            EvaluateRoll("3d6dh1.dropHighest(1) + 6", conf, 3, "3d6.dropHighest(1).dropHighest(1) + 6 => 1! + 5* + 3* + 6 => 7");
        }

        [TestMethod]
        public void Successfully_KeepLowestExtra()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 4, 2) };
            EvaluateRoll("3d6kl1 + 6", conf, 3, "3d6.keepLowest(1) + 6 => 1! + 5* + 3* + 6 => 7");
        }

        [TestMethod]
        public void Successfully_KeepLowestFunction()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 4, 2) };
            EvaluateRoll("3d6.keepLowest(1) + 6", conf, 3, "3d6.keepLowest(1) + 6 => 1! + 5* + 3* + 6 => 7");
        }

        [TestMethod]
        public void Successfully_KeepLowestTwice()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 4, 2) };
            EvaluateRoll("3d6kl2.keepLowest(1) + 6", conf, 3, "3d6.keepLowest(2).keepLowest(1) + 6 => 1! + 5* + 3* + 6 => 7");
        }

        [TestMethod]
        public void Successfully_KeepHighestExtra()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 4, 2) };
            EvaluateRoll("3d6kh1 + 6", conf, 3, "3d6.keepHighest(1) + 6 => 1!* + 5 + 3* + 6 => 11");
        }

        [TestMethod]
        public void Successfully_KeepHighestFunction()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 4, 2) };
            EvaluateRoll("3d6.keepHighest(1) + 6", conf, 3, "3d6.keepHighest(1) + 6 => 1!* + 5 + 3* + 6 => 11");
        }

        [TestMethod]
        public void Successfully_KeepHighestTwice()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 4, 2) };
            EvaluateRoll("3d6kh2.keepHighest(1) + 6", conf, 3, "3d6.keepHighest(2).keepHighest(1) + 6 => 1!* + 5 + 3* + 6 => 11");
        }

        [TestMethod]
        public void Successfully_AdvantageExtra()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(3, 15) };
            EvaluateRoll("1d20ad", conf, 2, "1d20.advantage() => 4* + 16 => 16");
        }

        [TestMethod]
        public void Successfully_AdvantageFunction()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(3, 15) };
            EvaluateRoll("1d20.advantage()", conf, 2, "1d20.advantage() => 4* + 16 => 16");
        }

        [TestMethod]
        public void Successfully_DisdvantageExtra()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(3, 15) };
            EvaluateRoll("1d20da", conf, 2, "1d20.disadvantage() => 4 + 16* => 4");
        }

        [TestMethod]
        public void Successfully_DisdvantageFunction()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(3, 15) };
            EvaluateRoll("1d20.disadvantage()", conf, 2, "1d20.disadvantage() => 4 + 16* => 4");
        }

        [TestMethod]
        public void Successfully_DropLowestExtra_Fudge()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 1, 2) };
            EvaluateRoll("3dFdl1", conf, 3, "3dF.dropLowest(1) => -1!* + 0 + 1! => 1");
        }

        [TestMethod]
        public void Successfully_DropLowestFunction_Fudge()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 1, 2) };
            EvaluateRoll("3dF.dropLowest(1)", conf, 3, "3dF.dropLowest(1) => -1!* + 0 + 1! => 1");
        }

        [TestMethod]
        public void Successfully_DropHighestExtra_Fudge()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 1, 2) };
            EvaluateRoll("3dFdh1", conf, 3, "3dF.dropHighest(1) => -1! + 0 + 1!* => -1");
        }

        [TestMethod]
        public void Successfully_DropHighestFunction_Fudge()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 1, 2) };
            EvaluateRoll("3dF.dropHighest(1)", conf, 3, "3dF.dropHighest(1) => -1! + 0 + 1!* => -1");
        }

        [TestMethod]
        public void Successfully_KeepLowestExtra_Fudge()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 1, 2) };
            EvaluateRoll("3dFkl1", conf, 3, "3dF.keepLowest(1) => -1! + 0* + 1!* => -1");
        }

        [TestMethod]
        public void Successfully_KeepLowestFunction_Fudge()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 1, 2) };
            EvaluateRoll("3dF.keepLowest(1)", conf, 3, "3dF.keepLowest(1) => -1! + 0* + 1!* => -1");
        }

        [TestMethod]
        public void Successfully_KeepHighestExtra_Fudge()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 1, 2) };
            EvaluateRoll("3dFkh1", conf, 3, "3dF.keepHighest(1) => -1!* + 0* + 1! => 1");
        }

        [TestMethod]
        public void Successfully_KeepHighestFunction_Fudge()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 1, 2) };
            EvaluateRoll("3dF.keepHighest(1)", conf, 3, "3dF.keepHighest(1) => -1!* + 0* + 1! => 1");
        }

        [TestMethod]
        public void Successfully_AdvantageExtra_Fudge()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 1) };
            EvaluateRoll("1dFad", conf, 2, "1dF.advantage() => -1!* + 0 => 0");
        }

        [TestMethod]
        public void Successfully_AdvantageFunction_Fudge()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 1) };
            EvaluateRoll("1dF.advantage()", conf, 2, "1dF.advantage() => -1!* + 0 => 0");
        }

        [TestMethod]
        public void Successfully_DisdvantageExtra_Fudge()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 1) };
            EvaluateRoll("1dFda", conf, 2, "1dF.disadvantage() => -1! + 0* => -1");
        }

        [TestMethod]
        public void Successfully_DisdvantageFunction_Fudge()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 1) };
            EvaluateRoll("1dF.disadvantage()", conf, 2, "1dF.disadvantage() => -1! + 0* => -1");
        }

        [TestMethod]
        public void ThrowAdvantageOnlyOnce_WhenDoingAdvantageTwice()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(3, 15, 7) };
            EvaluateRoll("1d20adda", conf, DiceErrorCode.AdvantageOnlyOnce);
        }

        [TestMethod]
        public void ThrowNoAdvantageKeep_WhenMixingKeepAndAdvantage()
        {
            EvaluateRoll("1d20adkh1", Roll9Conf, DiceErrorCode.NoAdvantageKeep);
        }

        [TestMethod]
        public void Successfully_DropLowestExtra_Group()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 2, 4) };
            EvaluateRoll("{2d6, 1d6}dl1", conf, 3, "{2d6, 1d6}.dropLowest(1) => (4!* + 5) => 5");
        }

        [TestMethod]
        public void Successfully_DropLowestFunction_Group()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 2, 4) };
            EvaluateRoll("{2d6, 1d6}.dropLowest(1)", conf, 3, "{2d6, 1d6}.dropLowest(1) => (4!* + 5) => 5");
        }

        [TestMethod]
        public void Successfully_DropHighestExtra_Group()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 2, 4) };
            EvaluateRoll("{2d6, 1d6}dh1", conf, 3, "{2d6, 1d6}.dropHighest(1) => (4! + 5*) => 4");
        }

        [TestMethod]
        public void Successfully_DropHighestFunction_Group()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 2, 4) };
            EvaluateRoll("{2d6, 1d6}.dropHighest(1)", conf, 3, "{2d6, 1d6}.dropHighest(1) => (4! + 5*) => 4");
        }

        [TestMethod]
        public void Successfully_KeepLowestExtra_Group()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 2, 4) };
            EvaluateRoll("{2d6, 1d6}kl1", conf, 3, "{2d6, 1d6}.keepLowest(1) => (4! + 5*) => 4");
        }

        [TestMethod]
        public void Successfully_KeepLowestFunction_Group()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 2, 4) };
            EvaluateRoll("{2d6, 1d6}.keepLowest(1)", conf, 3, "{2d6, 1d6}.keepLowest(1) => (4! + 5*) => 4");
        }

        [TestMethod]
        public void Successfully_KeepHighestExtra_Group()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 2, 4) };
            EvaluateRoll("{2d6, 1d6}kh1", conf, 3, "{2d6, 1d6}.keepHighest(1) => (4!* + 5) => 5");
        }

        [TestMethod]
        public void Successfully_KeepHighestFunction_Group()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(0, 2, 4) };
            EvaluateRoll("{2d6, 1d6}.keepHighest(1)", conf, 3, "{2d6, 1d6}.keepHighest(1) => (4!* + 5) => 5");
        }

        [TestMethod]
        public void Successfully_AdvantageExtra_Group()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(1, 4, 3, 1) };
            EvaluateRoll("{1d4, 1d6}ad", conf, 4, "{1d4, 1d6}.advantage() => (2 + 5) + (4!* + 2*) => 7");
        }

        [TestMethod]
        public void Successfully_AdvantageFunction_Group()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(1, 4, 3, 1) };
            EvaluateRoll("{1d4, 1d6}.advantage()", conf, 4, "{1d4, 1d6}.advantage() => (2 + 5) + (4!* + 2*) => 7");
        }

        [TestMethod]
        public void Successfully_DisdvantageExtra_Group()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(1, 4, 3, 1) };
            EvaluateRoll("{1d4, 1d6}da", conf, 4, "{1d4, 1d6}.disadvantage() => (2* + 5*) + (4! + 2) => 6");
        }

        [TestMethod]
        public void Successfully_DisdvantageFunction_Group()
        {
            var conf = new RollerConfig() { GetRandomBytes = GetRNG(1, 4, 3, 1) };
            EvaluateRoll("{1d4, 1d6}.disadvantage()", conf, 4, "{1d4, 1d6}.disadvantage() => (2* + 5*) + (4! + 2) => 6");
        }

        [TestMethod]
        public void ThrowAdvantageOnlyOnce_WhenDoingAdvantageTwice_Group()
        {
            EvaluateRoll("{1d4, 1d6}adda", Roll9Conf, DiceErrorCode.AdvantageOnlyOnce);
        }

        [TestMethod]
        public void ThrowNoAdvantageKeep_WhenMixingKeepAndAdvantage_Group()
        {
            EvaluateRoll("{1d4, 1d6}adkh1", Roll9Conf, DiceErrorCode.NoAdvantageKeep);
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
    }
}
