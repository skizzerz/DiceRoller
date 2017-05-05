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
            var conf = new RollerConfig();
            conf.FunctionRegistry.RegisterFunction("foo", Foo, FunctionScope.Roll, FunctionTiming.BeforeCrit);
            var partial = new RollPartialNode(new RollNode(RollType.Normal, One, Twenty));
            partial.AddCritical(new CritNode(equal20, equal1));
            partial.AddFunction(new FunctionNode(FunctionScope.Basic, "foo", new List<DiceAST>(), conf));
            partial.AddReroll(new RerollNode(0, equal1));
            Assert.AreEqual("RPARTIAL<<1d20.reroll(=1).critical(=20).fumble(=1).foo()>>", partial.ToString());
        }

        [TestMethod]
        public void Successfully_ToAST()
        {
            var conf = new RollerConfig();
            conf.FunctionRegistry.RegisterFunction("foo", Foo, FunctionScope.Roll, FunctionTiming.BeforeCrit);
            conf.GetRandomBytes = GetRNG(0, 1);
            var partial = new RollPartialNode(new RollNode(RollType.Normal, One, Twenty));
            partial.AddCritical(new CritNode(equal20, equal1));
            partial.AddFunction(new FunctionNode(FunctionScope.Basic, "foo", new List<DiceAST>(), conf));
            partial.AddReroll(new RerollNode(0, equal1));

            EvaluateNode(partial.CreateRollNode(), conf, 2, "1d20.reroll(=1).foo().critical(=20).fumble(=1) => 1!* + 2 => 2");
        }

        private void Foo(FunctionContext context)
        {
            context.Value = context.Expression.Value;
        }
    }
}
