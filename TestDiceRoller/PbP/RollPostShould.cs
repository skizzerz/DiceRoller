using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
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

        [TestMethod]
        public void Successfully_Validate_WhenNewPost()
        {
            var post = new RollPost();
            post.AddRoll("1d20");

            Assert.IsTrue(post.Validate());
        }

        [TestMethod]
        public void Successfully_Validate_WhenSavedPostMatches()
        {
            var stream = new MemoryStream();
            var post = new RollPost();

            post.AddRoll("1d20");
            post.Validate();

            post.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post2 = RollPost.Deserialize(stream);
            post2.AddRoll("1d20");

            Assert.IsTrue(post2.Validate());
        }

        [TestMethod]
        public void Successfully_Validate_WhenAddingRolls()
        {
            var stream = new MemoryStream();
            var post = new RollPost();

            post.AddRoll("1d20");
            post.Validate();

            post.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post2 = RollPost.Deserialize(stream);
            post2.AddRoll("1d20");
            post2.AddRoll("2d20");

            Assert.IsTrue(post2.Validate());
        }

        [TestMethod]
        public void Successfully_NotValidate_WhenRemovingRolls()
        {
            var stream = new MemoryStream();
            var post = new RollPost();

            post.AddRoll("1d20");
            post.Validate();

            post.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post2 = RollPost.Deserialize(stream);

            Assert.IsFalse(post2.Validate());
        }

        [TestMethod]
        public void Successfully_NotValidate_WhenChangingRolls()
        {
            var stream = new MemoryStream();
            var post = new RollPost();

            post.AddRoll("1d20");
            post.Validate();

            post.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post2 = RollPost.Deserialize(stream);
            post2.AddRoll("2d20");

            Assert.IsFalse(post2.Validate());
        }

        [TestMethod]
        public void Successfully_NotValidate_WhenChangingRollToInvalid()
        {
            var stream = new MemoryStream();
            var post = new RollPost();

            post.AddRoll("1d20");
            post.Validate();

            post.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post2 = RollPost.Deserialize(stream);
            EatExceptions(DiceErrorCode.ParseError, () => post2.AddRoll("qwerty"));

            Assert.IsFalse(post2.Validate());
            Assert.AreEqual(RollResult.InvalidRoll, post2.Current[0]);
        }

        [TestMethod]
        public void Successfully_Validate_WhenKeepingInvalidRoll()
        {
            var stream = new MemoryStream();
            var post = new RollPost();

            EatExceptions(DiceErrorCode.ParseError, () => post.AddRoll("qwerty"));
            post.Validate();
            Assert.AreEqual(RollResult.InvalidRoll, post.Pristine[0]);

            post.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post2 = RollPost.Deserialize(stream);
            EatExceptions(DiceErrorCode.ParseError, () => post2.AddRoll("qwerty"));

            Assert.IsTrue(post2.Validate());
            Assert.AreEqual(RollResult.InvalidRoll, post2.Current[0]);
        }

        [TestMethod]
        public void Successfully_Validate_WhenChangingInvalidRollToValid()
        {
            var stream = new MemoryStream();
            var post = new RollPost();

            EatExceptions(DiceErrorCode.ParseError, () => post.AddRoll("qwerty"));
            post.AddRoll("1d20");
            post.Validate();

            post.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post2 = RollPost.Deserialize(stream);
            post2.AddRoll("1d20");
            post2.AddRoll("1d20");

            Assert.IsTrue(post2.Validate());
            Assert.AreEqual(0, post2.Diverged);
        }

        [TestMethod]
        public void Successfully_NotValidate_WhenKeepingStoredPost()
        {
            var stream = new MemoryStream();
            var post = new RollPost();

            post.AddRoll("1d20");
            post.Validate();

            post.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post2 = RollPost.Deserialize(stream);
            post2.AddRoll("1d20+1");
            post2.Validate();

            stream.Dispose();
            stream = new MemoryStream();
            post2.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post3 = RollPost.Deserialize(stream);
            post3.AddRoll("1d20+1");

            Assert.IsFalse(post3.Validate());
            Assert.AreEqual(post3.Stored[0], post3.Current[0]);
        }

        [TestMethod]
        public void Successfully_ChangeInvalidToValid_Stored()
        {
            var stream = new MemoryStream();
            var post = new RollPost();

            post.AddRoll("1d20");
            post.AddRoll("1d20");
            post.Validate();

            post.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post2 = RollPost.Deserialize(stream);
            EatExceptions(DiceErrorCode.ParseError, () => post2.AddRoll("qwerty"));
            post2.AddRoll("1d20", Roll9Conf);
            post2.Validate();

            stream.Dispose();
            stream = new MemoryStream();
            post2.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post3 = RollPost.Deserialize(stream);
            post3.AddRoll("1d20+1");
            post3.AddRoll("1d20", Roll20Conf);

            Assert.IsFalse(post3.Validate());
            Assert.AreEqual(post3.Stored[1], post3.Current[1]);
            Assert.AreEqual(1, post3.Diverged);
        }
    }
}
