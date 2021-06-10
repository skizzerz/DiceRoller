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
        private static RollerConfig ImplodeConf => new RollerConfig() { GetRandomBytes = GetRNG(0, 0, 1, 9) };

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

        [TestMethod]
        public void ThrowMixedExplodeType_WhenMixingExplodeTypes_ExtraExtra()
        {
            EvaluateRoll("1d20!e!p", Explode20Conf, DiceErrorCode.MixedExplodeType);
        }

        [TestMethod]
        public void ThrowMixedExplodeType_WhenMixingExplodeTypes_ExtraFunction()
        {
            EvaluateRoll("1d20!c.compoundPenetrate()", Explode20Conf, DiceErrorCode.MixedExplodeType);
        }

        [TestMethod]
        public void ThrowMixedExplodeType_WhenMixingExplodeTypes_FunctionFunction()
        {
            EvaluateRoll("1d20.explode().compound()", Explode20Conf, DiceErrorCode.MixedExplodeType);
        }

        [TestMethod]
        public void ThrowMixedExplodeComp_WhenMixingExplodeComparisons_ExtraExtra()
        {
            EvaluateRoll("1d20!e!e=20", Explode20Conf, DiceErrorCode.MixedExplodeComp);
        }

        [TestMethod]
        public void ThrowMixedExplodeComp_WhenMixingExplodeComparisons_ExtraFunction()
        {
            EvaluateRoll("1d20!e.explode(=20)", Explode20Conf, DiceErrorCode.MixedExplodeComp);
        }

        [TestMethod]
        public void ThrowMixedExplodeComp_WhenMixingExplodeComparisons_FunctionFunction()
        {
            EvaluateRoll("1d20.explode().explode(=20)", Explode20Conf, DiceErrorCode.MixedExplodeComp);
        }

        [TestMethod]
        public void Successfully_CombineConditions_Function()
        {
            EvaluateRoll("1d20.explode(=6, =20)", Explode20Conf, 4, "1d20.explode(=6, =20) => 20! + 20! + 6 + 1! => 47");
        }

        [TestMethod]
        public void Successfully_CombineConditions_ExtraExtra()
        {
            EvaluateRoll("1d20!e6!e20", Explode20Conf, 4, "1d20.explode(=6, =20) => 20! + 20! + 6 + 1! => 47");
        }

        [TestMethod]
        public void Successfully_CombineConditions_ExtraFunction()
        {
            EvaluateRoll("1d20!e6.explode(=20)", Explode20Conf, 4, "1d20.explode(=6, =20) => 20! + 20! + 6 + 1! => 47");
        }

        [TestMethod]
        public void Successfully_CombineConditions_FunctionFunction()
        {
            EvaluateRoll("1d20.explode(=6).explode(=20)", Explode20Conf, 4, "1d20.explode(=6, =20) => 20! + 20! + 6 + 1! => 47");
        }

        [DataTestMethod]
        [DataRow("1d20!eo")]
        [DataRow("1d20.explodeOnce()")]
        public void Successfully_ExplodeOnce_WithoutCondition(string roll)
        {
            EvaluateRoll(roll, Explode20Conf, 2, "1d20.explodeOnce() => 20! + 20! => 40");
        }

        [DataTestMethod]
        [DataRow("1d20!eo20")]
        [DataRow("1d20!eo=20")]
        [DataRow("1d20.explodeOnce(=20)")]
        public void Successfully_ExplodeOnce_WithCondition(string roll)
        {
            EvaluateRoll(roll, Explode20Conf, 2, "1d20.explodeOnce(=20) => 20! + 20! => 40");
        }

        [DataTestMethod]
        [DataRow("1d20!i")]
        [DataRow("1d20.implode()")]
        public void Successfully_Implode_WithoutCondition(string roll)
        {
            EvaluateRoll(roll, ImplodeConf, 3, "1d20.implode() => 1! - 1! - 2 => -2");
        }

        [DataTestMethod]
        [DataRow("1d20!i<=2")]
        [DataRow("1d20.implode(<=2)")]
        public void Successfully_Implode_WithCondition(string roll)
        {
            EvaluateRoll(roll, ImplodeConf, 4, "1d20.implode(<=2) => 1! - 1! - 2 - 10 => -12");
        }

        [DataTestMethod]
        [DataRow("1d20!io")]
        [DataRow("1d20.implodeOnce()")]
        public void Successfully_ImplodeOnce_WithoutCondition(string roll)
        {
            EvaluateRoll(roll, ImplodeConf, 2, "1d20.implodeOnce() => 1! - 1! => 0");
        }

        [DataTestMethod]
        [DataRow("1d20!io<=2")]
        [DataRow("1d20.implodeOnce(<=2)")]
        public void Successfully_ImplodeOnce_WithCondition(string roll)
        {
            EvaluateRoll(roll, ImplodeConf, 2, "1d20.implodeOnce(<=2) => 1! - 1! => 0");
        }

        [TestMethod]
        public void Successfully_CompoundImplode_WithoutCondition()
        {
            EvaluateRoll("1d20.compoundImplode()", ImplodeConf, 3, "1d20.compoundImplode() => -2! => -2");
        }

        [TestMethod]
        public void Successfully_CompoundImplode_WithCondition()
        {
            EvaluateRoll("1d20.compoundImplode(<=2)", ImplodeConf, 4, "1d20.compoundImplode(<=2) => -12! => -12");
        }
    }
}
