using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.PbP;

namespace TestDiceRoller.PbP
{
    [TestClass]
    public class RollPostShould : TestBase
    {
        [TestMethod]
        public void Successfully_RollMacro_XPositive()
        {
            var post = new RollPost();
            post.AddRoll("1d20", Roll9Conf);
            post.AddRoll("[roll:1] + 1");

            Assert.AreEqual("[roll:1] + 1 => 9 + 1 => 10", post.Current[1].ToString());
        }

        [TestMethod]
        public void Successfully_RollMacro_XNegative()
        {
            var post = new RollPost();
            post.AddRoll("1d20", Roll9Conf);
            post.AddRoll("[roll:-1] + 1");

            Assert.AreEqual("[roll:-1] + 1 => 9 + 1 => 10", post.Current[1].ToString());
        }

        [TestMethod]
        public void Successfully_RollMacro_XY()
        {
            var post = new RollPost();
            post.AddRoll("2d20", new RollerConfig() { GetRandomBytes = GetRNG(8, 9) });
            post.AddRoll("[roll:1:1] + 1");

            Assert.AreEqual("[roll:1:1] + 1 => 9 + 1 => 10", post.Current[1].ToString());
        }

        [TestMethod]
        public void Successfully_RollMacro_X_Critical()
        {
            var post = new RollPost();
            post.AddRoll("2d20", Roll20Conf);
            post.AddRoll("[roll:1:critical]");

            Assert.AreEqual("[roll:1:critical] => 2 => 2", post.Current[1].ToString());
        }

        [TestMethod]
        public void Successfully_RollMacro_XY_Critical()
        {
            var post = new RollPost();
            post.AddRoll("2d20", Roll20Conf);
            post.AddRoll("[roll:1:1:critical]");

            Assert.AreEqual("[roll:1:1:critical] => 1 => 1", post.Current[1].ToString());
        }
    }
}
