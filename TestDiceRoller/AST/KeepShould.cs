using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;

namespace TestDiceRoller.AST
{
    [TestClass]
    public class KeepShould : TestBase
    {
        private static RollerConfig StatConf => new RollerConfig() { GetRandomBytes = GetRNG(4, 2, 5, 0) };
        private static RollerConfig AdvantageConf => new RollerConfig() { GetRandomBytes = GetRNG(1, 18) };

        private enum KeepType
        {
            KeepHighest,
            KeepLowest,
            DropHighest,
            DropLowest,
            Advantage,
            Disadvantage
        }

        (RollData, DiceAST) GetKeep(KeepType type, RollerConfig config, DiceAST expression, DiceAST argument = null)
        {
            var arguments = new List<DiceAST>();
            if (argument != null)
            {
                arguments.Add(argument);
            }

            var data = Data(config);
            var node = new FunctionNode(FunctionScope.Basic, type.ToString(), arguments, data);
            node.Context.Expression = expression;

            return (data, node);
        }

        [TestMethod]
        public void Successfully_DropHighest()
        {
            var (data, node) = GetKeep(KeepType.DropHighest, StatConf, _4d6, One);
            EvaluateNode(node,data, 4, "4d6.dropHighest(1) => 5 + 3 + 6!* + 1! => 9");
        }

        [TestMethod]
        public void Successfully_DropLowest()
        {
            var (data, node) = GetKeep(KeepType.DropLowest, StatConf, _4d6, One);
            EvaluateNode(node, data, 4, "4d6.dropLowest(1) => 5 + 3 + 6! + 1!* => 14");
        }

        [TestMethod]
        public void Successfully_KeepHighest()
        {
            var (data, node) = GetKeep(KeepType.KeepHighest, StatConf, _4d6, One);
            EvaluateNode(node, data, 4, "4d6.keepHighest(1) => 5* + 3* + 6! + 1!* => 6");
        }

        [TestMethod]
        public void Successfully_KeepLowest()
        {
            var (data, node) = GetKeep(KeepType.KeepLowest, StatConf, _4d6, One);
            EvaluateNode(node, data, 4, "4d6.keepLowest(1) => 5* + 3* + 6!* + 1! => 1");
        }

        [TestMethod]
        public void Successfully_ApplyAdvantage()
        {
            var (data, node) = GetKeep(KeepType.Advantage, AdvantageConf, _1d20);
            EvaluateNode(node, data, 2, "1d20.advantage() => 2* + 19 => 19");
        }

        [TestMethod]
        public void Successfully_ApplyDisadvantage()
        {
            var (data, node) = GetKeep(KeepType.Disadvantage, AdvantageConf, _1d20);
            EvaluateNode(node, data, 2, "1d20.disadvantage() => 2 + 19* => 2");
        }

        [TestMethod]
        public void ThrowNegativeDice_WhenKeepingNegativeDice()
        {
            var (data, node) = GetKeep(KeepType.KeepHighest, StatConf, _4d6, MinusOne);
            EvaluateNode(node, data, DiceErrorCode.NegativeDice);
        }

        [TestMethod]
        public void CountSuccesses_WhenKeepingSuccessDice()
        {
            var success = new FunctionNode(FunctionScope.Basic, "success", new List<DiceAST>() { greaterEqual5 }, Data(StatConf));
            success.Context.Expression = _4d6;
            var failure = new FunctionNode(FunctionScope.Basic, "failure", new List<DiceAST>() { equal1 }, Data(StatConf));
            failure.Context.Expression = success;

            var (data, node) = GetKeep(KeepType.DropLowest, StatConf, failure, One);
            EvaluateNode(node, data, 4, "4d6.success(>=5).failure(=1).dropLowest(1) => $5 + 3 + $6 + 1* => 2 successes");
        }

        [TestMethod]
        public void Successfully_AllowDroppingMoreDiceThanExist()
        {
            var (data, node) = GetKeep(KeepType.DropHighest, StatConf, _4d6, Five);
            EvaluateNode(node, data, 4, "4d6.dropHighest(5) => 5* + 3* + 6!* + 1!* => 0");
        }

        [TestMethod]
        public void Successfully_AllowKeepingZeroDice()
        {
            var (data, node) = GetKeep(KeepType.KeepHighest, StatConf, _4d6, Zero);
            EvaluateNode(node, data, 4, "4d6.keepHighest(0) => 5* + 3* + 6!* + 1!* => 0");
        }

        [TestMethod]
        public void ThrowIncorrectArity_WhenAmountIsNullAndNotAdvantage()
        {
            var (data, node) = GetKeep(KeepType.KeepHighest, StatConf, _4d6);
            EvaluateNode(node, data, DiceErrorCode.IncorrectArity);
        }

        [TestMethod]
        public void ThrowIncorrectArity_WhenAmountNotNullAndIsAdvantage()
        {
            var (data, node) = GetKeep(KeepType.Advantage, StatConf, _4d6, One);
            EvaluateNode(node, data, DiceErrorCode.IncorrectArity);
        }
    }
}
