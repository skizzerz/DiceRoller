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
    }
}
