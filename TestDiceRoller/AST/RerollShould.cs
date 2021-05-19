using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;

namespace TestDiceRoller.AST
{
    [TestClass]
    public class RerollShould : TestBase
    {
        private static RollerConfig RerollConf => new RollerConfig() { GetRandomBytes = GetRNG(0, 1, 0, 9) };

        private enum RerollType
        {
            Reroll,
            RerollOnce,
            RerollN
        }

        (RollData, DiceAST) GetReroll(RerollType type, RollerConfig config, DiceAST expression, params DiceAST[] arguments)
        {
            var data = Data(config);
            var node = new FunctionNode(FunctionScope.Basic, type.ToString(), arguments, data);
            node.Context.Expression = expression;

            return (data, node);
        }

        [TestMethod]
        public void Successfully_Reroll_WhenConditionMet()
        {
            var (data, node) = GetReroll(RerollType.Reroll, RerollConf, _1d20, less5);
            EvaluateNode(node, data, 4, "1d20.reroll(<5) => 1!* + 2* + 1!* + 10 => 10");
        }

        [TestMethod]
        public void Successfully_NotReroll_WhenConditionNotMet()
        {
            var (data, node) = GetReroll(RerollType.Reroll, Roll9Conf, _1d20, less5);
            EvaluateNode(node, data, 1, "1d20.reroll(<5) => 9 => 9");
        }

        [TestMethod]
        public void Successfully_RerollOnce_WhenConditionMet()
        {
            var (data, node) = GetReroll(RerollType.RerollOnce, RerollConf, _1d20, less5);
            EvaluateNode(node, data, 2, "1d20.rerollOnce(<5) => 1!* + 2 => 2");
        }

        [TestMethod]
        public void Successfully_RerollN_WhenConditionMet()
        {
            var (data, node) = GetReroll(RerollType.RerollN, RerollConf, _1d20, Two, less5);
            EvaluateNode(node, data, 3, "1d20.rerollN(2, <5) => 1!* + 2* + 1! => 1");
        }

        [TestMethod]
        public void ThrowTooManyDice_WhenRerollingMoreThanMaxDice()
        {
            var conf = new RollerConfig() { MaxDice = 10, GetRandomBytes = GetRNG(Roll1()) };
            var (data, node) = GetReroll(RerollType.Reroll, conf, _1d20, equal1);
            EvaluateNode(node, data, DiceErrorCode.TooManyDice);
        }

        [TestMethod]
        public void Successfully_AbortRerollWhenMaxRerollsReached()
        {
            var conf = new RollerConfig() { MaxRerolls = 2, GetRandomBytes = GetRNG(Roll1()) };
            var (data, node) = GetReroll(RerollType.Reroll, conf, _1d20, equal1);
            EvaluateNode(node, data, 3, "1d20.reroll(=1) => 1!* + 1!* + 1! => 1");
        }
    }
}
