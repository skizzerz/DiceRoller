using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;

namespace TestDiceRoller.AST
{
    [TestClass]
    public class ExplodeShould : TestBase
    {
        // for d20!e => 20, 20, 6 => 46
        // for d20!p => 20, 6(5), 6(5), 1(0) => 30
        // for d20!p=20 => 20, 20(19), 6(5) => 44
        // the final 9 is never rolled
        private static RollerConfig Explode20Conf => new RollerConfig() { GetRandomBytes = GetRNG(19, 59, 5, 0, 9) };
        // for d100!p => 100, 20(19), 6(5) => 124
        // the final 9 is never rolled
        private static RollerConfig Explode100Conf => new RollerConfig() { GetRandomBytes = GetRNG(99, 19, 5, 9) };

        private enum ExplodeType
        {
            Explode,
            Compound,
            Penetrate,
            CompoundPenetrate
        }

        (RollData, DiceAST) GetExplode(ExplodeType type, RollerConfig config, DiceAST expression, ComparisonNode comparison)
        {
            var arguments = new List<DiceAST>();
            if (comparison != null)
            {
                arguments.Add(comparison);
            }

            var data = Data(config);
            var node = new FunctionNode(FunctionScope.Basic, type.ToString(), arguments, data);
            node.Context.Expression = expression;

            return (data, node);
        }

        [TestMethod]
        public void Successfully_ExplodeWithoutCondition_IfMaxRoll()
        {
            var (data, node) = GetExplode(ExplodeType.Explode, Explode20Conf, _1d20, null);
            EvaluateNode(node, data, 3, "1d20.explode() => 20! + 20! + 6 => 46");
        }

        [TestMethod]
        public void Successfully_NotExplodeWithoutCondition_IfNotMaxRoll()
        {
            var (data, node) = GetExplode(ExplodeType.Explode, Roll9Conf, _1d20, null);
            EvaluateNode(node, data, 1, "1d20.explode() => 9 => 9");
        }

        [TestMethod]
        public void Successfully_ExplodeWithCondition_IfConditionMet()
        {
            var (data, node) = GetExplode(ExplodeType.Explode, Explode20Conf, _1d20, equal20);
            EvaluateNode(node, data, 3, "1d20.explode(=20) => 20! + 20! + 6 => 46");
        }

        [TestMethod]
        public void Successfully_NotExplodeWithCondition_IfConditionNotMet()
        {
            var (data, node) = GetExplode(ExplodeType.Explode, Roll9Conf, _1d20, equal20);
            EvaluateNode(node, data, 1, "1d20.explode(=20) => 9 => 9");
        }

        [TestMethod]
        public void Successfully_CompoundWithoutCondition_IfMaxRoll()
        {
            var (data, node) = GetExplode(ExplodeType.Compound, Explode20Conf, _1d20, null);
            EvaluateNode(node, data, 3, "1d20.compound() => 46! => 46");
        }

        [TestMethod]
        public void Successfully_NotCompoundWithoutCondition_IfNotMaxRoll()
        {
            var (data, node) = GetExplode(ExplodeType.Compound, Roll9Conf, _1d20, null);
            EvaluateNode(node, data, 1, "1d20.compound() => 9 => 9");
        }

        [TestMethod]
        public void Successfully_CompoundWithCondition_IfConditionMet()
        {
            var (data, node) = GetExplode(ExplodeType.Compound, Explode20Conf, _1d20, equal20);
            EvaluateNode(node, data, 3, "1d20.compound(=20) => 46! => 46");
        }

        [TestMethod]
        public void Successfully_NotCompoundWithCondition_IfConditionNotMet()
        {
            var (data, node) = GetExplode(ExplodeType.Compound, Roll9Conf, _1d20, equal20);
            EvaluateNode(node, data, 1, "1d20.compound(=20) => 9 => 9");
        }

        [TestMethod]
        public void Successfully_PenetrateWithoutCondition_IfMaxRoll_WithSpeciald20Behavior()
        {
            var (data, node) = GetExplode(ExplodeType.Penetrate, Explode20Conf, _1d20, null);
            EvaluateNode(node, data, 4, "1d20.penetrate() => 20! + 5! + 5! + 0! => 30");
        }

        [TestMethod]
        public void Successfully_PenetrateWithoutCondition_IfMaxRoll_WithSpeciald100Behavior()
        {
            var (data, node) = GetExplode(ExplodeType.Penetrate, Explode100Conf, _1d100, null);
            EvaluateNode(node, data, 3, "1d100.penetrate() => 100! + 19! + 5 => 124");
        }

        [TestMethod]
        public void Successfully_NotPenetrateWithoutCondition_IfNotMaxRoll()
        {
            var (data, node) = GetExplode(ExplodeType.Penetrate, Roll9Conf, _1d20, null);
            EvaluateNode(node, data, 1, "1d20.penetrate() => 9 => 9");
        }

        [TestMethod]
        public void Successfully_PenetrateWithCondition_IfConditionMet_NoSpecialBehavior()
        {
            var (data, node) = GetExplode(ExplodeType.Penetrate, Explode20Conf, _1d20, equal20);
            EvaluateNode(node, data, 3, "1d20.penetrate(=20) => 20! + 19! + 5 => 44");
        }

        [TestMethod]
        public void Successfully_NotPenetrateWithCondition_IfConditionNotMet()
        {
            var (data, node) = GetExplode(ExplodeType.Penetrate, Roll9Conf, _1d20, equal20);
            EvaluateNode(node, data, 1, "1d20.penetrate(=20) => 9 => 9");
        }

        [TestMethod]
        public void Successfully_CompoundPenetrateWithoutCondition_IfMaxRoll_WithSpeciald20Behavior()
        {
            var (data, node) = GetExplode(ExplodeType.CompoundPenetrate, Explode20Conf, _1d20, null);
            EvaluateNode(node, data, 4, "1d20.compoundPenetrate() => 30! => 30");
        }

        [TestMethod]
        public void Successfully_CompoundPenetrateWithoutCondition_IfMaxRoll_WithSpeciald100Behavior()
        {
            var (data, node) = GetExplode(ExplodeType.CompoundPenetrate, Explode100Conf, _1d100, null);
            EvaluateNode(node, data, 3, "1d100.compoundPenetrate() => 124! => 124");
        }

        [TestMethod]
        public void Successfully_NotCompoundPenetrateWithoutCondition_IfNotMaxRoll()
        {
            var (data, node) = GetExplode(ExplodeType.CompoundPenetrate, Roll9Conf, _1d20, null);
            EvaluateNode(node, data, 1, "1d20.compoundPenetrate() => 9 => 9");
        }

        [TestMethod]
        public void Successfully_CompoundPenetrateWithCondition_IfConditionMet_NoSpecialBehavior()
        {
            var (data, node) = GetExplode(ExplodeType.CompoundPenetrate, Explode20Conf, _1d20, equal20);
            EvaluateNode(node, data, 3, "1d20.compoundPenetrate(=20) => 44! => 44");
        }

        [TestMethod]
        public void Successfully_NotCompoundPenetrateWithCondition_IfConditionNotMet()
        {
            var (data, node) = GetExplode(ExplodeType.CompoundPenetrate, Roll9Conf, _1d20, equal20);
            EvaluateNode(node, data, 1, "1d20.compoundPenetrate(=20) => 9 => 9");
        }
    }
}
