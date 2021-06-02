using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;

namespace TestDiceRoller.AST
{
    [TestClass]
    public class SuccessShould : TestBase
    {
        private static RollerConfig SuccessConf => new RollerConfig() { GetRandomBytes = GetRNG(3, 4, 0, 5) };

        private enum SuccessType
        {
            Success,
            Failure
        }

        (RollData, DiceAST) GetSuccess(SuccessType type, RollerConfig config, DiceAST expression, DiceAST argument = null)
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
        public void Successfully_CountSuccesses()
        {
            var (data, node) = GetSuccess(SuccessType.Success, SuccessConf, _4d6, greaterEqual5);
            EvaluateNode(node, data, 4, "4d6.success(>=5) => 4 + $5 + 1 + $6 => 2 successes");
        }

        [TestMethod]
        public void Successfully_CountSuccessesAndFailures()
        {
            var (data, success) = GetSuccess(SuccessType.Success, SuccessConf, _4d6, greaterEqual5);
            var (_, node) = GetSuccess(SuccessType.Failure, SuccessConf, success, equal1);
            EvaluateNode(node, data, 4, "4d6.success(>=5).failure(=1) => 4 + $5 + #1 + $6 => 1 success");
        }

        [TestMethod]
        public void Successfully_CountFailures()
        {
            var (data, node) = GetSuccess(SuccessType.Failure, SuccessConf, _4d6, equal1);
            EvaluateNode(node, data, 4, "4d6.failure(=1) => 4 + 5 + #1 + 6 => -1 successes");
        }

        [TestMethod]
        public void Successfully_IgnoreDroppedDice()
        {
            var data = Data(SuccessConf);
            var drop = new FunctionNode(FunctionScope.Basic, "dropLowest", new List<DiceAST>() { One }, data);
            drop.Context.Expression = _4d6;
            var (_, success) = GetSuccess(SuccessType.Success, SuccessConf, drop, greaterEqual5);
            var (_, node) = GetSuccess(SuccessType.Failure, SuccessConf, success, equal1);
            EvaluateNode(node, data, 4, "4d6.dropLowest(1).success(>=5).failure(=1) => 4 + $5 + 1* + $6 => 2 successes");
        }
    }
}
