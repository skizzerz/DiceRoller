﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.IO.Compression;
using System.Diagnostics.CodeAnalysis;
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
        public void Successfully_AttackRollExpression()
        {
            var post = new RollPost();
            post.AddRoll("1d20", Roll9Conf);
            post.AddRoll("if([roll:-1:critical], =0, 2d10, 2{2d10})+3", Roll9Conf);

            Assert.AreEqual("if([roll:-1:critical], =0, 2d10, 2{2d10}) + 3 => (9 + 9) + 3 => 21", post.Current[1].ToString());
        }

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
        public void AdvantageCriticalBug()
        {
            // Ref: https://www.dndbeyond.com/forums/d-d-beyond-general/play-by-post/2985-rolling-dice?page=81#c1948
            // 2nd set of rolls in that post, crit was rolled on advantage but crit damage was not rolled
            var post = new RollPost();
            var config = new RollerConfig() { GetRandomBytes = GetRNG(18, 5, 5, 7, 9, 19, 3, 3, 0, 0) };
            post.AddRoll("1d20.critical(>=19)", config);
            post.AddRoll("if([roll:-1:critical], =0, 1d8ro<=2, 2{1d8ro<=2}.expand())", config);
            post.AddRoll("1d20.advantage().critical(>=19)", config);
            post.AddRoll("if([roll:-1:critical], =0, 1d8ro<=2, 2{1d8ro<=2}.expand())", config);
            post.Validate();

            Assert.AreEqual("1d20.advantage().critical(>=19) => 10* + 20! => 20", post.Current[2].ToString());
            Assert.AreEqual("if([roll:-1:critical], =0, 1d8.rerollOnce(<=2), 2{1d8.rerollOnce(<=2)}.expand()) => ((4) + (1!* + 1!)) => 5", post.Current[3].ToString());
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
        public void Successfully_PreserveMetadata()
        {
            var metadata = "foobar";
            var stream = new MemoryStream();
            var post = new RollPost();

            post.AddRoll("1d20", null, new RollData() { Metadata = metadata });
            post.Validate();

            post.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post2 = RollPost.Deserialize(stream);
            post2.AddRoll("1d20");
            post2.Validate();

            Assert.AreEqual(metadata, (string)post2.Current[0].Metadata);
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
            EatExceptions(() => post2.AddRoll("q"));
            post2.Validate();

            stream.Dispose();
            stream = new MemoryStream();
            post2.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post3 = RollPost.Deserialize(stream);
            post3.AddRoll("1d20+1");
            EatExceptions(() => post3.AddRoll("q"));

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

        [TestMethod]
        public void Successfully_PostsDontDiverge_LargerFirst()
        {
            var post1 = new RollPost();
            var post2 = new RollPost();

            post1.AddRoll("1d20", Roll9Conf);
            post1.AddRoll("1d20", Roll9Conf);
            post2.AddRoll("1d20", Roll9Conf);

            Assert.IsFalse(post1.HasDivergedFrom(post2));
        }

        [TestMethod]
        public void Successfully_PostsDontDiverge_LargerSecond()
        {
            var post1 = new RollPost();
            var post2 = new RollPost();

            post1.AddRoll("1d20", Roll9Conf);
            post2.AddRoll("1d20", Roll9Conf);
            post2.AddRoll("1d20", Roll9Conf);

            Assert.IsFalse(post1.HasDivergedFrom(post2));
        }

        [TestMethod]
        public void Successfully_PostsDontDiverge_SameSize()
        {
            var post1 = new RollPost();
            var post2 = new RollPost();

            post1.AddRoll("1d20", Roll9Conf);
            post1.AddRoll("1d20", Roll9Conf);
            post2.AddRoll("1d20", Roll9Conf);
            post2.AddRoll("1d20", Roll9Conf);

            Assert.IsFalse(post1.HasDivergedFrom(post2));
        }

        [TestMethod]
        public void Successfully_PostsDontDiverge_Empty()
        {
            var post1 = new RollPost();
            var post2 = new RollPost();

            Assert.IsFalse(post1.HasDivergedFrom(post2));
        }

        [TestMethod]
        public void Successfully_PostsDiverge_LargerFirst()
        {
            var post1 = new RollPost();
            var post2 = new RollPost();

            post1.AddRoll("1d20", Roll1Conf);
            post1.AddRoll("1d20", Roll9Conf);
            post2.AddRoll("1d20", Roll9Conf);

            Assert.IsTrue(post1.HasDivergedFrom(post2));
        }

        [TestMethod]
        public void Successfully_PostsDiverge_LargerSecond()
        {
            var post1 = new RollPost();
            var post2 = new RollPost();

            post1.AddRoll("1d20", Roll1Conf);
            post2.AddRoll("1d20", Roll9Conf);
            post2.AddRoll("1d20", Roll9Conf);

            Assert.IsTrue(post1.HasDivergedFrom(post2));
        }

        [TestMethod]
        public void Successfully_PostsDiverge_SameSize()
        {
            var post1 = new RollPost();
            var post2 = new RollPost();

            post1.AddRoll("1d20", Roll9Conf);
            post1.AddRoll("1d20", Roll9Conf);
            post2.AddRoll("1d20", Roll9Conf);
            post2.AddRoll("1d20", Roll1Conf);

            Assert.IsTrue(post1.HasDivergedFrom(post2));
        }

        [TestMethod]
        public void Successfully_PostsDiverge_Stored()
        {
            var post1 = new RollPost();
            var post2 = new RollPost();
            var stream = new MemoryStream();

            post1.AddRoll("1d20", Roll9Conf);
            post1.AddRoll("1d20", Roll9Conf);
            post2.AddRoll("1d20", Roll9Conf);
            post2.AddRoll("1d20", Roll1Conf);
            post1.Validate();
            post2.Validate();

            post1.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post3 = RollPost.Deserialize(stream);
            stream.Dispose();

            stream = new MemoryStream();
            post2.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post4 = RollPost.Deserialize(stream);
            stream.Dispose();

            Assert.IsTrue(post3.HasDivergedFrom(post4));
        }

        [TestMethod]
        public void ThrowInvalidMacro_WhenNoArgs_RollMacro()
        {
            EvaluatePost("[roll]", DiceErrorCode.InvalidMacro);
        }

        [TestMethod]
        public void ThrowInvalidMacro_WhenNotInt_RollMacro()
        {
            EvaluatePost("[roll:Q]", DiceErrorCode.InvalidMacro);
        }

        [TestMethod]
        public void ThrowInvalidMacro_WhenTooSmall_RollMacro()
        {
            EvaluatePost("[roll:-1]", DiceErrorCode.InvalidMacro);
        }

        [TestMethod]
        public void ThrowInvalidMacro_WhenTooLarge_RollMacro()
        {
            EvaluatePost("[roll:2]", DiceErrorCode.InvalidMacro);
        }

        [TestMethod]
        public void ThrowInvalidMacro_WhenTooSmallDice_RollMacro()
        {
            var post = new RollPost();
            post.AddRoll("1d20");
            EvaluatePost("[roll:1:0]", DiceErrorCode.InvalidMacro, post);
        }

        [TestMethod]
        public void ThrowInvalidMacro_WhenTooLargeDice_RollMacro()
        {
            var post = new RollPost();
            post.AddRoll("1d20");
            EvaluatePost("[roll:1:2]", DiceErrorCode.InvalidMacro, post);
        }

        [TestMethod]
        public void ThrowInvalidMacro_WhenInvalidDice_RollMacro()
        {
            var post = new RollPost();
            post.AddRoll("1d20");
            EvaluatePost("[roll:1:Q]", DiceErrorCode.InvalidMacro, post);
        }

        [TestMethod]
        public void ThrowInvalidMacro_WhenInvalidTag_RollMacro()
        {
            var post = new RollPost();
            post.AddRoll("1d20");
            // technically the same as InvalidDice in terms of implementation
            // adding as separate test just in case that changes in the future
            EvaluatePost("[roll:1:creetical]", DiceErrorCode.InvalidMacro, post);
        }

        [TestMethod]
        public void Successfully_RollMacro_ReadFumbleFlag()
        {
            var post = new RollPost();
            post.AddRoll("2d20", new RollerConfig() { GetRandomBytes = GetRNG(5, 0) });
            post.AddRoll("[roll:1:2:fumble]");

            Assert.AreEqual(1, post.Current[1].Value);
        }

        [TestMethod]
        public void Successfully_RollMacro_ReadSuccessFlag()
        {
            var post = new RollPost();
            post.AddRoll("2d10>5", new RollerConfig() { GetRandomBytes = GetRNG(5, 0) });
            post.AddRoll("[roll:1:2:success]");

            Assert.AreEqual(0, post.Current[1].Value);
        }

        [TestMethod]
        public void Successfully_RollMacro_ReadFailureFlag()
        {
            var post = new RollPost();
            post.AddRoll("2d10>5f1", new RollerConfig() { GetRandomBytes = GetRNG(5, 0) });
            post.AddRoll("[roll:1:failure]");

            Assert.AreEqual(1, post.Current[1].Value);
        }

        private static void EvaluatePost(string roll, DiceErrorCode error, RollPost post = null)
        {
            post = post ?? new RollPost();

            try
            {
                post.AddRoll(roll);
                Assert.Fail("Expected DiceException with error code {0}, but did not receive an exception.", error.ToString());
            }
            catch (DiceException e)
            {
                if (e.ErrorCode != error)
                {
                    Assert.Fail("Expected DiceException with error code {0}, but got error code {1}.", error.ToString(), e.ErrorCode.ToString());
                }
            }
        }
    }
}
