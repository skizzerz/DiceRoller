using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;

namespace TestDiceRoller.Grammar
{
    [TestClass]
    public class MacrosShould : TestBase
    {
        private RollerConfig Conf = new RollerConfig() { ExecuteMacro = ExecuteMacro };

        [TestMethod]
        public void Successfully_RunMacro()
        {
            EvaluateRoll("[one]", Conf, 0, "[one] => 1 => 1");
        }

        [TestMethod]
        public void Successfully_RunMacro_Spaces()
        {
            EvaluateRoll("[  one ]", Conf, 0, "[one] => 1 => 1");
        }

        [TestMethod]
        public void Successfully_RunMacro_ExtraneousArgs()
        {
            EvaluateRoll("[one::::::::]", Conf, 0, "[one::::::::] => 1 => 1");
        }

        [TestMethod]
        public void Successfully_WithArgs()
        {
            EvaluateRoll("[x:1.2345]", Conf, 0, "[x:1.2345] => 1.2345 => 1.2345");
        }

        [TestMethod]
        public void Successfully_WithArgs_Spaces()
        {
            EvaluateRoll("[  x  : 1.2345 ]", Conf, 0, "[x:1.2345] => 1.2345 => 1.2345");
        }

        [TestMethod]
        public void ThrowParseError_EmptyMacro()
        {
            EvaluateRoll("[]", Conf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowInvalidMacro_UnknownMacro()
        {
            EvaluateRoll("[???]", Conf, DiceErrorCode.InvalidMacro);
        }

        [TestMethod]
        public void ThrowInvalidMacro_Spaces()
        {
            EvaluateRoll("[ ]", Conf, DiceErrorCode.InvalidMacro);
        }

        [TestMethod]
        public void ThrowInvalidMacro_Colons()
        {
            EvaluateRoll("[::::::::::::::::::::::::::]", Conf, DiceErrorCode.InvalidMacro);
        }
    }
}
