using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;
using Dice.Grammar;

namespace TestDiceRoller.AST
{
    [TestClass]
    public class RollPartialNodeShould : TestBase
    {
        [TestMethod]
        public void Successfully_ToString()
        {
            RollData data = new RollData()
            {
                Config = new RollerConfig()
            };
            data.Config.FunctionRegistry.RegisterFunction("foo", Foo, FunctionScope.Roll, FunctionTiming.BeforeCrit);
            var partial = new RollPartialNode(new RollNode(RollType.Normal, One, Twenty));
            partial.AddFunction(new FunctionNode(FunctionScope.Basic, "critical", new List<DiceAST>() { equal20 }, data));
            partial.AddFunction(new FunctionNode(FunctionScope.Basic, "fumble", new List<DiceAST>() { equal1 }, data));
            partial.AddFunction(new FunctionNode(FunctionScope.Basic, "foo", new List<DiceAST>(), data));
            partial.AddFunction(new FunctionNode(FunctionScope.Basic, "reroll", new List<DiceAST>() { equal1 }, data));
            Assert.AreEqual("RPARTIAL<<1d20.reroll(=1).critical(=20).fumble(=1).foo()>>", partial.ToString());
        }

        [TestMethod]
        public void Successfully_ToAST()
        {
            RollData data = new RollData()
            {
                Config = new RollerConfig()
            };
            data.Config.FunctionRegistry.RegisterFunction("foo", Foo, FunctionScope.Roll, FunctionTiming.BeforeCrit);
            data.Config.GetRandomBytes = GetRNG(0, 1);
            var partial = new RollPartialNode(new RollNode(RollType.Normal, One, Twenty));
            partial.AddFunction(new FunctionNode(FunctionScope.Basic, "critical", new List<DiceAST>() { equal20 }, data));
            partial.AddFunction(new FunctionNode(FunctionScope.Basic, "fumble", new List<DiceAST>() { equal1 }, data));
            partial.AddFunction(new FunctionNode(FunctionScope.Basic, "foo", new List<DiceAST>(), data));
            partial.AddFunction(new FunctionNode(FunctionScope.Basic, "reroll", new List<DiceAST>() { equal1 }, data));

            EvaluateNode(partial.CreateRollNode(), data, 2, "1d20.reroll(=1).foo().critical(=20).fumble(=1) => 1!* + 2 => 2");
        }

        private void Foo(FunctionContext context)
        {
            context.Value = context.Expression.Value;
        }
    }
}
