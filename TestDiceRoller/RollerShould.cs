using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;

namespace TestDiceRoller
{
    [TestClass]
    public class RollerShould : TestBase
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowArgumentNullExpression_WhenNullDiceExpr()
        {
            Roller.Roll(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThrowInvalidOperationException_WhenTooFewMaxDice()
        {
            var conf = new RollerConfig() { MaxDice = 0 };
            Roller.Roll("1d20", conf);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThrowInvalidOperationException_WhenTooFewMaxSides()
        {
            var conf = new RollerConfig() { MaxSides = 0 };
            Roller.Roll("1d20", conf);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThrowInvalidOperationException_WhenTooLowMaxRecursionDepth()
        {
            var conf = new RollerConfig() { MaxRecursionDepth = -1 };
            Roller.Roll("1d20", conf);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThrowInvalidOperationException_WhenTooLowMaxRerolls()
        {
            var conf = new RollerConfig() { MaxRerolls = -1 };
            Roller.Roll("1d20", conf);
        }
    }
}
