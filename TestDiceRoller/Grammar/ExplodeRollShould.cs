using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;

namespace TestDiceRoller.Grammar
{
    [TestClass]
    public class ExplodeRollShould : TestBase
    {
        // for d20!e => 20, 20, 6 => 46
        // for d20!p => 20, 6(5), 6(5), 1(0) => 30
        // for d20!p=20 => 20, 20(19), 6(5) => 44
        // the final 9 is never rolled
        private static RollerConfig Explode20Conf => new RollerConfig() { GetRandomBytes = GetRNG(19, 59, 5, 0, 9) };
        // for d100!p => 100, 20(19), 6(5) => 124
        // the final 9 is never rolled
        private static RollerConfig Explode100Conf => new RollerConfig() { GetRandomBytes = GetRNG(99, 19, 5, 9) };

        [TestMethod]
        public void Successfully_ExplodeExtraWithoutCondition()
        {
            EvaluateRoll("1d20!e", Explode20Conf, 3, "1d20.explode() => 20! + 20! + 6 => 46");
        }

        [TestMethod]
        public void Successfully_ExplodeExtraWithCondition()
        {
            EvaluateRoll("1d20!e20", Explode20Conf, 3, "1d20.explode(=20) => 20! + 20! + 6 => 46");
        }

        [TestMethod]
        public void Successfully_ExplodeFunctionWithoutCondition()
        {
            EvaluateRoll("1d20.explode()", Explode20Conf, 3, "1d20.explode() => 20! + 20! + 6 => 46");
        }

        [TestMethod]
        public void Successfully_ExplodeFunctionWithCondition()
        {
            EvaluateRoll("1d20.explode(=20)", Explode20Conf, 3, "1d20.explode(=20) => 20! + 20! + 6 => 46");
        }

        [TestMethod]
        public void Successfully_CompoundExtraWithoutCondition()
        {
            EvaluateRoll("1d20!c", Explode20Conf, 3, "1d20.compound() => 46! => 46");
        }

        [TestMethod]
        public void Successfully_CompoundExtraWithCondition()
        {
            EvaluateRoll("1d20!c20", Explode20Conf, 3, "1d20.compound(=20) => 46! => 46");
        }

        [TestMethod]
        public void Successfully_CompoundFunctionWithoutCondition()
        {
            EvaluateRoll("1d20.compound()", Explode20Conf, 3, "1d20.compound() => 46! => 46");
        }

        [TestMethod]
        public void Successfully_CompoundFunctionWithCondition()
        {
            EvaluateRoll("1d20.compound(=20)", Explode20Conf, 3, "1d20.compound(=20) => 46! => 46");
        }

        [TestMethod]
        public void Successfully_PenetrateExtraWithoutCondition_WithSpeciald20Behavior()
        {
            EvaluateRoll("1d20!p", Explode20Conf, 4, "1d20.penetrate() => 20! + 5! + 5! + 0! => 30");
        }

        [TestMethod]
        public void Successfully_PenetrateExtraWithoutCondition_WithSpeciald100Behavior()
        {
            EvaluateRoll("1d100!p", Explode100Conf, 3, "1d100.penetrate() => 100! + 19! + 5 => 124");
        }

        [TestMethod]
        public void Successfully_PenetrateExtraWithCondition_NoSpecialBehavior()
        {
            EvaluateRoll("1d20!p20", Explode20Conf, 3, "1d20.penetrate(=20) => 20! + 19! + 5 => 44");
        }

        [TestMethod]
        public void Successfully_PenetrateFunctionWithoutCondition_WithSpeciald20Behavior()
        {
            EvaluateRoll("1d20.penetrate()", Explode20Conf, 4, "1d20.penetrate() => 20! + 5! + 5! + 0! => 30");
        }

        [TestMethod]
        public void Successfully_PenetrateFunctionWithoutCondition_WithSpeciald100Behavior()
        {
            EvaluateRoll("1d100.penetrate()", Explode100Conf, 3, "1d100.penetrate() => 100! + 19! + 5 => 124");
        }

        [TestMethod]
        public void Successfully_PenetrateFunctionWithCondition_NoSpecialBehavior()
        {
            EvaluateRoll("1d20.penetrate(=20)", Explode20Conf, 3, "1d20.penetrate(=20) => 20! + 19! + 5 => 44");
        }

        [TestMethod]
        public void Successfully_CompoundPenetrateFunctionWithoutCondition_WithSpeciald20Behavior()
        {
            EvaluateRoll("1d20.compoundPenetrate()", Explode20Conf, 4, "1d20.compoundPenetrate() => 30! => 30");
        }

        [TestMethod]
        public void Successfully_CompoundPenetrateFunctionWithoutCondition_WithSpeciald100Behavior()
        {
            EvaluateRoll("1d100.compoundPenetrate()", Explode100Conf, 3, "1d100.compoundPenetrate() => 124! => 124");
        }

        [TestMethod]
        public void Successfully_CompoundPenetrateFunctionWithCondition_NoSpecialBehavior()
        {
            EvaluateRoll("1d20.compoundPenetrate(=20)", Explode20Conf, 3, "1d20.compoundPenetrate(=20) => 44! => 44");
        }
    }
}
