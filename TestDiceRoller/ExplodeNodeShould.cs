using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;

namespace TestDiceRoller
{
    [TestClass]
    public class ExplodeNodeShould : TestBase
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
        public void Successfully_ExplodeWithoutCondition_IfMaxRoll()
        {
            var node = new ExplodeNode(ExplodeType.Explode, false, null) { Expression = _1d20 };
            EvaluateNode(node, Explode20Conf, 3, "1d20.explode() => 20! + 20! + 6 => 46");
        }

        [TestMethod]
        public void Successfully_NotExplodeWithoutCondition_IfNotMaxRoll()
        {
            var node = new ExplodeNode(ExplodeType.Explode, false, null) { Expression = _1d20 };
            EvaluateNode(node, Roll9Conf, 1, "1d20.explode() => 9 => 9");
        }

        [TestMethod]
        public void Successfully_ExplodeWithCondition_IfConditionMet()
        {
            var node = new ExplodeNode(ExplodeType.Explode, false, equal20) { Expression = _1d20 };
            EvaluateNode(node, Explode20Conf, 3, "1d20.explode(=20) => 20! + 20! + 6 => 46");
        }

        [TestMethod]
        public void Successfully_NotExplodeWithCondition_IfConditionNotMet()
        {
            var node = new ExplodeNode(ExplodeType.Explode, false, equal20) { Expression = _1d20 };
            EvaluateNode(node, Roll9Conf, 1, "1d20.explode(=20) => 9 => 9");
        }

        [TestMethod]
        public void Successfully_CompoundWithoutCondition_IfMaxRoll()
        {
            var node = new ExplodeNode(ExplodeType.Explode, true, null) { Expression = _1d20 };
            EvaluateNode(node, Explode20Conf, 3, "1d20.compound() => 46! => 46");
        }

        [TestMethod]
        public void Successfully_NotCompoundWithoutCondition_IfNotMaxRoll()
        {
            var node = new ExplodeNode(ExplodeType.Explode, true, null) { Expression = _1d20 };
            EvaluateNode(node, Roll9Conf, 1, "1d20.compound() => 9 => 9");
        }

        [TestMethod]
        public void Successfully_CompoundWithCondition_IfConditionMet()
        {
            var node = new ExplodeNode(ExplodeType.Explode, true, equal20) { Expression = _1d20 };
            EvaluateNode(node, Explode20Conf, 3, "1d20.compound(=20) => 46! => 46");
        }

        [TestMethod]
        public void Successfully_NotCompoundWithCondition_IfConditionNotMet()
        {
            var node = new ExplodeNode(ExplodeType.Explode, true, equal20) { Expression = _1d20 };
            EvaluateNode(node, Roll9Conf, 1, "1d20.compound(=20) => 9 => 9");
        }

        [TestMethod]
        public void Successfully_PenetrateWithoutCondition_IfMaxRoll_WithSpeciald20Behavior()
        {
            var node = new ExplodeNode(ExplodeType.Penetrate, false, null) { Expression = _1d20 };
            EvaluateNode(node, Explode20Conf, 4, "1d20.penetrate() => 20! + 5! + 5! + 0! => 30");
        }

        [TestMethod]
        public void Successfully_PenetrateWithoutCondition_IfMaxRoll_WithSpeciald100Behavior()
        {
            var node = new ExplodeNode(ExplodeType.Penetrate, false, null) { Expression = _1d100 };
            EvaluateNode(node, Explode100Conf, 3, "1d100.penetrate() => 100! + 19! + 5 => 124");
        }

        [TestMethod]
        public void Successfully_NotPenetrateWithoutCondition_IfNotMaxRoll()
        {
            var node = new ExplodeNode(ExplodeType.Penetrate, false, null) { Expression = _1d20 };
            EvaluateNode(node, Roll9Conf, 1, "1d20.penetrate() => 9 => 9");
        }

        [TestMethod]
        public void Successfully_PenetrateWithCondition_IfConditionMet_NoSpecialBehavior()
        {
            var node = new ExplodeNode(ExplodeType.Penetrate, false, equal20) { Expression = _1d20 };
            EvaluateNode(node, Explode20Conf, 3, "1d20.penetrate(=20) => 20! + 19! + 5 => 44");
        }

        [TestMethod]
        public void Successfully_NotPenetrateWithCondition_IfConditionNotMet()
        {
            var node = new ExplodeNode(ExplodeType.Penetrate, false, equal20) { Expression = _1d20 };
            EvaluateNode(node, Roll9Conf, 1, "1d20.penetrate(=20) => 9 => 9");
        }

        [TestMethod]
        public void Successfully_CompoundPenetrateWithoutCondition_IfMaxRoll_WithSpeciald20Behavior()
        {
            var node = new ExplodeNode(ExplodeType.Penetrate, true, null) { Expression = _1d20 };
            EvaluateNode(node, Explode20Conf, 4, "1d20.compoundPenetrate() => 30! => 30");
        }

        [TestMethod]
        public void Successfully_CompoundPenetrateWithoutCondition_IfMaxRoll_WithSpeciald100Behavior()
        {
            var node = new ExplodeNode(ExplodeType.Penetrate, true, null) { Expression = _1d100 };
            EvaluateNode(node, Explode100Conf, 3, "1d100.compoundPenetrate() => 124! => 124");
        }

        [TestMethod]
        public void Successfully_NotCompoundPenetrateWithoutCondition_IfNotMaxRoll()
        {
            var node = new ExplodeNode(ExplodeType.Penetrate, true, null) { Expression = _1d20 };
            EvaluateNode(node, Roll9Conf, 1, "1d20.compoundPenetrate() => 9 => 9");
        }

        [TestMethod]
        public void Successfully_CompoundPenetrateWithCondition_IfConditionMet_NoSpecialBehavior()
        {
            var node = new ExplodeNode(ExplodeType.Penetrate, true, equal20) { Expression = _1d20 };
            EvaluateNode(node, Explode20Conf, 3, "1d20.compoundPenetrate(=20) => 44! => 44");
        }

        [TestMethod]
        public void Successfully_NotCompoundPenetrateWithCondition_IfConditionNotMet()
        {
            var node = new ExplodeNode(ExplodeType.Penetrate, true, equal20) { Expression = _1d20 };
            EvaluateNode(node, Roll9Conf, 1, "1d20.compoundPenetrate(=20) => 9 => 9");
        }
    }
}
