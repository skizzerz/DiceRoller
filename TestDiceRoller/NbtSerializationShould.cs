using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.PbP;
using Dice.Serialization;

namespace TestDiceRoller
{
    [TestClass]
    public class NbtSerializationShould : TestBase
    {
        [TestMethod]
        public void Successfully_RoundtripDieResult()
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
            var stream = new MemoryStream();
            var post = new RollPost();

            post.AddRoll("1d20+4");
            post.AddRoll("2d6+3");
            post.Validate();

            post.Serialize(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var post2 = RollPost.Deserialize(stream);

            // when using NBT serialization, Current roundtrips to Stored
            CollectionAssert.AreEqual(post.Pristine.ToList(), post2.Pristine.ToList());
            CollectionAssert.AreEqual(post.Current.ToList(), post2.Stored.ToList());
            Assert.AreEqual(0, post2.Current.Count);
            Assert.AreEqual(0, post2.Diverged);
        }

        [TestMethod]
        public void Successfully_BeSmallerThanBinaryFormatter()
        {
            var binStream = new MemoryStream();
            var nbtStream = new MemoryStream();
            var binFmt = new BinaryFormatter();
            var post = new RollPost();

            post.AddRoll("20d6+4d8");
            post.AddRoll("(1d8)d6");
            post.AddRoll("[roll:2] + 4");

            post.Serialize(nbtStream);
            binFmt.Serialize(binStream, post);

            Assert.IsTrue(nbtStream.Length * 10 <= binStream.Length, "Binary stream is not significantly larger than NBT stream");
        }
    }
}
