using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;

namespace TestDiceRoller
{
    [TestClass]
    public class FunctionRegistryShould : TestBase
    {
        [TestMethod]
        public void Successfully_RegisterStaticFunctions()
        {
            var registry = new FunctionRegistry();
            registry.RegisterType(typeof(FunctionContainer));
            var conf = new RollerConfig() { FunctionRegistry = registry, GetRandomBytes = GetRNG(Roll9()) };
            EvaluateRoll("1d20.a()", conf, 1, "1d20.a() => 9 => 10");
        }

        [TestMethod]
        public void Successfully_RegisterAllFunctions()
        {
            var registry = new FunctionRegistry();
            var cont = new FunctionContainer(2);
            registry.RegisterType(cont);
            var conf = new RollerConfig() { FunctionRegistry = registry, GetRandomBytes = GetRNG(Roll9()) };
            EvaluateRoll("1d20.a()", conf, 1, "1d20.a() => 9 => 10");
            EvaluateRoll("1d20.b()", conf, 1, "1d20.b() => 9 => 11");
            EvaluateRoll("b()", conf, 0, "b() => 2 => 2");
        }

        [TestMethod]
        public void Successfully_RegisterFunction()
        {
            var registry = new FunctionRegistry();
            registry.RegisterFunction("c", FunctionContainer.A, FunctionScope.Basic);
            var conf = new RollerConfig() { FunctionRegistry = registry, GetRandomBytes = GetRNG(Roll9()) };
            EvaluateRoll("1d20.c()", conf, 1, "1d20.c() => 9 => 10");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThrowInvalidOperationException_WhenInvalidDiceFunctionAttribute()
        {
            new FunctionRegistry().RegisterType(typeof(Invalid1));
        }

        private class FunctionContainer
        {
            [DiceFunction("a", Scope = FunctionScope.Roll, Timing = FunctionTiming.First)]
            public static void A(FunctionContext context)
            {
                // adds one to the die
                context.Value = context.Expression.Value + 1;
            }

            private int b;

            public FunctionContainer(int amt)
            {
                b = amt;
            }

            [DiceFunction("b", Scope = FunctionScope.All, Timing = FunctionTiming.First)]
            public void B(FunctionContext context)
            {
                // adds b to the die
                context.Value = (context.Expression?.Value ?? 0) + b;
            }
        }

        private class Invalid1
        {
            [DiceFunction("a")]
            public static int A(FunctionContext context)
            {
                return 0;
            }
        }
    }
}
