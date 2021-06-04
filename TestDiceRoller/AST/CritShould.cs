using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;

namespace TestDiceRoller.AST
{
    [TestClass]
    public class CritShould : TestBase
    {
        private static RollerConfig CritConf => new RollerConfig() { GetRandomBytes = GetRNG(0, 1, 18, 19) };

        private enum CritType
        {
            Critical,
            Fumble
        }

        static (RollData, DiceAST) GetCrit(CritType type, RollerConfig config, DiceAST expression, DiceAST argument = null)
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
        public void Successfully_MarkCritsAndFumbles()
        {
            var (data, crit) = GetCrit(CritType.Critical, CritConf, _4d20, equal19);
            var (_, node) = GetCrit(CritType.Fumble, CritConf, crit, equal2);
            EvaluateNode(node, data, 4, "4d20.critical(=19).fumble(=2) => 1 + 2! + 19! + 20 => 42");
        }

        [TestMethod]
        public void Successfully_MarkCrits_AndKeepFumblesSame()
        {
            var (data, node) = GetCrit(CritType.Critical, CritConf, _4d20, equal19);
            EvaluateNode(node, data, 4, "4d20.critical(=19) => 1! + 2 + 19! + 20 => 42");
        }

        [TestMethod]
        public void Successfully_MarkFumbles_AndKeepCritsSame()
        {
            var (data, node) = GetCrit(CritType.Fumble, CritConf, _4d20, equal2);
            EvaluateNode(node, data, 4, "4d20.fumble(=2) => 1 + 2! + 19 + 20! => 42");
        }
    }
}
