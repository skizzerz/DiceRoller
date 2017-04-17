using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;

namespace TestDiceRoller.Grammar
{
    [TestClass]
    public class SuccessRollShould : TestBase
    {
        private static RollerConfig SuccessConf => new RollerConfig() { GetRandomBytes = GetRNG(3, 4, 0, 5) };
        private static RollerConfig FudgeConf => new RollerConfig() { GetRandomBytes = GetRNG(0, 1, 2, 2) };

        [TestMethod]
        public void Successfully_SuccessExtra()
        {
            EvaluateRoll("4d6>=5", SuccessConf, 4, "4d6.success(>=5) => 4 + $5 + 1! + $6! => 2 successes");
        }

        [TestMethod]
        public void Successfully_SuccessFunction()
        {
            EvaluateRoll("4d6.success(>=5)", SuccessConf, 4, "4d6.success(>=5) => 4 + $5 + 1! + $6! => 2 successes");
        }

        [TestMethod]
        public void Successfully_SuccessExtra_Fudge()
        {
            EvaluateRoll("4dF=1", FudgeConf, 4, "4dF.success(=1) => -1! + 0 + $1! + $1! => 2 successes");
        }

        [TestMethod]
        public void Successfully_SuccessFunction_Fudge()
        {
            EvaluateRoll("4dF.success(=1)", FudgeConf, 4, "4dF.success(=1) => -1! + 0 + $1! + $1! => 2 successes");
        }

        [TestMethod]
        public void Successfully_SuccessFailureExtra()
        {
            EvaluateRoll("4d6>=5f1", SuccessConf, 4, "4d6.success(>=5).failure(=1) => 4 + $5 + #1! + $6! => 1 success");
        }

        [TestMethod]
        public void Successfully_SuccessFailureFunction()
        {
            EvaluateRoll("4d6.success(>=5).failure(=1)", SuccessConf, 4, "4d6.success(>=5).failure(=1) => 4 + $5 + #1! + $6! => 1 success");
        }

        [TestMethod]
        public void Successfully_SuccessFailureExtra_Fudge()
        {
            EvaluateRoll("4dF=1f-1", FudgeConf, 4, "4dF.success(=1).failure(=-1) => #-1! + 0 + $1! + $1! => 1 success");
        }

        [TestMethod]
        public void Successfully_SuccessFailureFunction_Fudge()
        {
            EvaluateRoll("4dF.success(=1).failure(=-1)", FudgeConf, 4, "4dF.success(=1).failure(=-1) => #-1! + 0 + $1! + $1! => 1 success");
        }

        [TestMethod]
        public void Successfully_IgnoreDroppedDice()
        {
            EvaluateRoll("4d6dl1>=5f1", SuccessConf, 4, "4d6.dropLowest(1).success(>=5).failure(=1) => 4 + $5 + 1!* + $6! => 2 successes");
        }

        [TestMethod]
        public void Successfully_CombineExtraFunction()
        {
            EvaluateRoll("4d6=5f1.success(=6)", SuccessConf, 4, "4d6.success(=5, =6).failure(=1) => 4 + $5 + #1! + $6! => 1 success");
        }

        [TestMethod]
        public void Successfully_AddSuccessToConstant_ResultSuccess()
        {
            EvaluateRoll("4d6>=5 + 2", SuccessConf, 4, "4d6.success(>=5) + 2 => 4 + $5 + 1! + $6! + 2 => 4 successes");
        }

        [TestMethod]
        public void Successfully_AddSuccessToSuccess_ResultSuccess()
        {
            EvaluateRoll("2d6>=5 + 2d6>=5", SuccessConf, 4, "2d6.success(>=5) + 2d6.success(>=5) => 4 + $5 + 1! + $6! => 2 successes");
        }

        [TestMethod]
        public void Successfully_AddSuccessToRoll_ResultTotal()
        {
            EvaluateRoll("2d6>=5 + 2d6", SuccessConf, 4, "2d6.success(>=5) + 2d6 => 4 + $5 + 1! + 6! => 8");
        }
    }
}
