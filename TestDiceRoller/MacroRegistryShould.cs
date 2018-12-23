using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;

namespace TestDiceRoller
{
    [TestClass]
    public class MacroRegistryShould : TestBase
    {
        [TestMethod]
        public void Successfully_RegisterStaticMacros()
        {
            var registry = new MacroRegistry();
            registry.RegisterType(typeof(MacroContainer));
            var conf = new RollerConfig() { MacroRegistry = registry, GetRandomBytes = GetRNG(Roll9()) };
            EvaluateRoll("1d20+[a]", conf, 1, "1d20 + [a] => 9 + 1 => 10");
        }

        [TestMethod]
        public void Successfully_RegisterAllMacros()
        {
            var registry = new MacroRegistry();
            var cont = new MacroContainer(2);
            registry.RegisterType(cont);
            Assert.IsTrue(registry.Contains("a"));
            Assert.IsTrue(registry.Contains("b"));
            Assert.IsFalse(registry.Contains("c"));
            Assert.IsTrue(registry.Contains("z"));

            var conf = new RollerConfig() { MacroRegistry = registry, GetRandomBytes = GetRNG(Roll9()) };
            EvaluateRoll("1d20 + [a]", conf, 1, "1d20 + [a] => 9 + 1 => 10");
            EvaluateRoll("[b]d20", conf, 2, "2d20 => 9 + 9 => 18");
            EvaluateRoll("[z]d20", conf, 2, "2d20 => 9 + 9 => 18");
        }

        [TestMethod]
        public void Successfully_RegisterMacro()
        {
            var registry = new MacroRegistry();
            registry.RegisterMacro("c", MacroContainer.A);
            var conf = new RollerConfig() { MacroRegistry = registry, GetRandomBytes = GetRNG(Roll9()) };
            EvaluateRoll("[c]d20", conf, 1, "1d20 => 9 => 9");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThrowInvalidOperationException_WhenDuplicatingMacro_Function()
        {
            var registry = new MacroRegistry();
            registry.RegisterMacro("c", MacroContainer.A);
            registry.RegisterMacro("c", MacroContainer.A);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThrowInvalidOperationException_WhenDuplicatingMacro_Type()
        {
            var registry = new MacroRegistry();
            registry.RegisterType(typeof(Invalid2));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThrowInvalidOperationException_WhenInvalidDiceMacroAttribute()
        {
            new MacroRegistry().RegisterType(typeof(Invalid1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowArgumentNullException_WhenRemovingNullMacro()
        {
            var registry = new MacroRegistry();
            registry.Remove(null);
        }

        [TestMethod]
        public void Successfully_RemoveMacro()
        {
            var registry = new MacroRegistry();
            registry.RegisterMacro("a", Invalid2.A);
            registry.RegisterMacro("b", Invalid2.A);

            Assert.IsTrue(registry.Contains("a"));
            Assert.IsTrue(registry.Contains("b"));

            registry.Remove("a");

            Assert.IsFalse(registry.Contains("a"));
            Assert.IsTrue(registry.Contains("b"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowArgumentNullException_WhenNullDiceMacroAttribute()
        {
            var registry = new MacroRegistry();
            registry.RegisterType(typeof(Invalid3));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowArgumentNullException_WhenRegisteringNullMacro()
        {
            var registry = new MacroRegistry();
            registry.RegisterMacro(null, Invalid2.A);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowArgumentException_WhenRegisteringEmptyMacro()
        {
            var registry = new MacroRegistry();
            registry.RegisterMacro("", Invalid2.A);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowArgumentNullException_WhenRegisteringNullGlobalMacro()
        {
            var registry = new MacroRegistry();
            registry.RegisterGlobalMacro(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowArgumentNullException_WhenRemovingNullGlobalMacro()
        {
            var registry = new MacroRegistry();
            registry.RemoveGlobal(null);
        }

        [TestMethod]
        public void Successfully_RegisterGlobalMacro_Config()
        {
            var registry = new MacroRegistry();
            registry.RegisterType(new MacroContainer(3));
            registry.RegisterGlobalMacro(MacroContainer.A);

            var config = new RollerConfig() { MacroRegistry = registry };
            EvaluateRoll("[b]", config, 0, "[b] => 3 => 3");
            EvaluateRoll("[bb]", config, 0, "[bb] => 1 => 1");
        }

        [TestMethod]
        public void Successfully_RegisterGlobalMacro_Data()
        {
            var registry = new MacroRegistry();
            registry.RegisterType(new MacroContainer(3));
            registry.RegisterGlobalMacro(MacroContainer.A);

            var data = new RollData() { MacroRegistry = registry };
            EvaluateRoll("[b]", null, data, 0, "[b] => 3 => 3");
            EvaluateRoll("[bb]", null, data, 0, "[bb] => 1 => 1");
        }

        [TestMethod]
        public void Successfully_ShadowConfigWithData()
        {
            var registry1 = new MacroRegistry();
            var registry2 = new MacroRegistry();
            var cont = new MacroContainer(3);
            registry1.RegisterType(cont);
            registry2.RegisterMacro("a", cont.B);

            var config = new RollerConfig() { MacroRegistry = registry1 };
            var data = new RollData() { MacroRegistry = registry2 };
            EvaluateRoll("[b]", config, data, 0, "[b] => 3 => 3");
            EvaluateRoll("[a]", config, data, 0, "[a] => 3 => 3");
        }

        private class MacroContainer
        {
            [DiceMacro("a")]
            public static void A(MacroContext context)
            {
                if (context.Value == Decimal.MinValue)
                {
                    context.Value = 1;
                }
            }

            private readonly int b;

            public MacroContainer(int amt)
            {
                b = amt;
            }

            [DiceMacro("b"), DiceMacro("z")]
            public void B(MacroContext context)
            {
                context.Value = b;
            }

            [DiceMacro("c")]
            private void C(MacroContext context) { }

            [DiceMacro("d")]
            public static void D(MacroContext context) { }

            [DiceMacro("e")]
            public void E(MacroContext context) { }
        }

        private class Invalid1
        {
            [DiceMacro("a")]
            public static int A(MacroContext context)
            {
                return 0;
            }
        }

        private class Invalid2
        {
            [DiceMacro("a")]
            public static void A(MacroContext context) { }

            [DiceMacro("a")]
            public static void B(MacroContext context) { }
        }

        private class Invalid3
        {
            [DiceMacro(null)]
            public static void A(MacroContext context) { }
        }

        private class Invalid4
        {
            [DiceMacro("a")]
            public void A(MacroContext context) { }

            [DiceMacro("a")]
            public static void B(MacroContext context) { }
        }
    }
}
