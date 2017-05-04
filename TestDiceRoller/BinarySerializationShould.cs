using System;
using System.Linq;
using System.IO;
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

            Assert.IsTrue(post.Pristine.SequenceEqual(post2.Pristine), "Pristine did not roundtrip");
            Assert.IsTrue(post.Stored.SequenceEqual(post2.Stored), "Stored did not roundtrip");
            Assert.IsTrue(post.Current.SequenceEqual(post2.Current), "Current did not roundtrip");
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

            Assert.IsTrue(post.Pristine.SequenceEqual(post2.Pristine), "Pristine did not roundtrip");
            Assert.IsTrue(post.Stored.SequenceEqual(post2.Stored), "Stored did not roundtrip");
            Assert.IsTrue(post.Current.SequenceEqual(post2.Current), "Current did not roundtrip");
            Assert.AreEqual(post.Diverged, post2.Diverged);
        }
    }
}
