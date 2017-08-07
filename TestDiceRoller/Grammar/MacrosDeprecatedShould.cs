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
    public class MacrosDeprecatedShould : TestBase
    {
#pragma warning disable CS0618 // Type or member is obsolete -- ensure deprecated version still works
        private RollerConfig DeprecatedConf = new RollerConfig() { ExecuteMacro = ExecuteMacro };
#pragma warning restore CS0618 // Type or member is obsolete

        [TestMethod]
        public void Successfully_RunMacro()
        {
            EvaluateRoll("[one]", DeprecatedConf, 0, "[one] => 1 => 1");
        }

        [TestMethod]
        public void Successfully_RunMacro_Spaces()
        {
            EvaluateRoll("[  one ]", DeprecatedConf, 0, "[one] => 1 => 1");
        }

        [TestMethod]
        public void Successfully_RunMacro_ExtraneousArgs()
        {
            EvaluateRoll("[one::::::::]", DeprecatedConf, 0, "[one::::::::] => 1 => 1");
        }

        [TestMethod]
        public void Successfully_WithArgs()
        {
            EvaluateRoll("[x:1.2345]", DeprecatedConf, 0, "[x:1.2345] => 1.2345 => 1.2345");
        }

        [TestMethod]
        public void Successfully_WithArgs_Spaces()
        {
            EvaluateRoll("[  x  : 1.2345 ]", DeprecatedConf, 0, "[x:1.2345] => 1.2345 => 1.2345");
        }

        [TestMethod]
        public void ThrowParseError_EmptyMacro()
        {
            EvaluateRoll("[]", DeprecatedConf, DiceErrorCode.ParseError);
        }

        [TestMethod]
        public void ThrowInvalidMacro_UnknownMacro()
        {
            EvaluateRoll("[???]", DeprecatedConf, DiceErrorCode.InvalidMacro);
        }

        [TestMethod]
        public void ThrowInvalidMacro_Spaces()
        {
            EvaluateRoll("[ ]", DeprecatedConf, DiceErrorCode.InvalidMacro);
        }

        [TestMethod]
        public void ThrowInvalidMacro_Colons()
        {
            EvaluateRoll("[::::::::::::::::::::::::::]", DeprecatedConf, DiceErrorCode.InvalidMacro);
        }
    }
}
