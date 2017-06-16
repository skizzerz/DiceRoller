using System;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.PbP;

namespace TestDiceRoller
{
    [TestClass]
    public class BinarySerializationShould : TestBase
    {
        [TestMethod]
        public void Successfully_RoundtripDieResult()
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            var die = new DieResult()
            {
                DieType = DieType.Fudge,
                NumSides = 3,
                Value = -2,
                Flags = DieFlags.Extra | DieFlags.Failure,
                Data = "Some Data"
            };

            formatter.Serialize(stream, die);
            stream.Seek(0, SeekOrigin.Begin);
            var die2 = (DieResult)formatter.Deserialize(stream);
            Assert.AreEqual(die, die2);
        }

        [TestMethod]
        public void Successfully_RoundtripDieResult_ForPersistence()
        {
            var stream = new MemoryStream();
            var die = new DieResult()
            {
                DieType = DieType.Fudge,
                NumSides = 3,
                Value = -2,
                Flags = DieFlags.Extra | DieFlags.Failure,
                Data = "Some Data"
            };

            die.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var die2 = DieResult.Deserialize(stream);
            Assert.AreEqual(die, die2);
        }

        [TestMethod]
        public void Successfully_RoundtripRollResult()
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            var result = Roller.Roll("1d20+4");

            formatter.Serialize(stream, result);
            stream.Seek(0, SeekOrigin.Begin);
            var result2 = (RollResult)formatter.Deserialize(stream);
            Assert.AreEqual(result, result2);
        }

        [TestMethod]
        public void Successfully_RoundtripRollResult_ForPersistence()
        {
            var stream = new MemoryStream();
            var result = Roller.Roll("1d20+4");

            result.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var result2 = RollResult.Deserialize(stream);
            Assert.AreEqual(result, result2);
        }

        [TestMethod]
        public void Successfully_RoundtripRollPost()
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            var post = new RollPost();

            post.AddRoll("1d20+4");
            post.AddRoll("2d6+3");
            post.Validate();

            formatter.Serialize(stream, post);
            stream.Seek(0, SeekOrigin.Begin);
            var post2 = (RollPost)formatter.Deserialize(stream);

            CollectionAssert.AreEqual(post.Pristine.ToList(), post2.Pristine.ToList());
            CollectionAssert.AreEqual(post.Stored.ToList(), post2.Stored.ToList());
            CollectionAssert.AreEqual(post.Current.ToList(), post2.Current.ToList());
            Assert.AreEqual(post.Diverged, post2.Diverged);
        }

        [TestMethod]
        public void Successfully_RoundtripRollPost_NoValidate()
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            var post = new RollPost();

            post.AddRoll("1d20+4");
            post.AddRoll("2d6+3");

            formatter.Serialize(stream, post);
            stream.Seek(0, SeekOrigin.Begin);
            var post2 = (RollPost)formatter.Deserialize(stream);

            CollectionAssert.AreEqual(post.Pristine.ToList(), post2.Pristine.ToList());
            CollectionAssert.AreEqual(post.Stored.ToList(), post2.Stored.ToList());
            CollectionAssert.AreEqual(post.Current.ToList(), post2.Current.ToList());
            Assert.AreEqual(post.Diverged, post2.Diverged);
        }

        [TestMethod]
        public void Successfully_RoundtripRollPost_ForPersistence()
        {
            var stream = new MemoryStream();
            var post = new RollPost();

            post.AddRoll("20d20+4");
            post.AddRoll("2d6+3");
            post.AddRoll("[roll:2]-2");
            post.Validate();

            post.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post2 = RollPost.Deserialize(stream);

            CollectionAssert.AreEqual(post.Pristine.ToList(), post2.Pristine.ToList());
            CollectionAssert.AreEqual(post.Current.ToList(), post2.Stored.ToList());
            CollectionAssert.AreEqual(new List<RollResult>(), post2.Current.ToList());
            Assert.AreEqual(0, post2.Diverged);

            post2.AddRoll("10d20+4");
            Assert.AreEqual(2, post2.Diverged);
        }

        [TestMethod]
        public void TestCompression()
        {
            var ustream = new MemoryStream();
            var cstream = new MemoryStream();
            var post = new RollPost();

            post.AddRoll("20d20+100d6");
            post.AddRoll("(4d8)d6+3");
            post.AddRoll("[roll:2]-2");
            post.AddRoll("1000d10000");

            post.Serialize(ustream);

            using (var dstream = new DeflateStream(cstream, CompressionLevel.Optimal, true))
            {
                post.Serialize(dstream);
            }

            Assert.IsTrue(cstream.Length < ustream.Length * 0.5, "Compression isn't significantly smaller");
        }
    }
}
