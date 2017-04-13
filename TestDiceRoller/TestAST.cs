using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;
using Dice.Grammar;

namespace TestDiceRoller
{
    [TestClass]
    public class TestAST
    {
        private static Action<byte[]> GetRNG(IEnumerator<uint> values)
        {
            void GetRandomBytes(byte[] arr)
            {
                values.MoveNext();
                BitConverter.GetBytes(values.Current).CopyTo(arr, 0);
            }

            return GetRandomBytes;
        }

        private static IEnumerator<uint> Roll9()
        {
            while (true)
            {
                // this is incremented in RollNode.Roll by 1.
                yield return 8;
            }
        }

        private static IEnumerator<uint> Roll1()
        {
            while (true)
            {
                yield return 0;
            }
        }

        private readonly LiteralNode One = new LiteralNode(1);
        private readonly LiteralNode Twenty = new LiteralNode(20);
        private readonly RollerConfig Roll9Conf = new RollerConfig() { GetRandomBytes = GetRNG(Roll9()) };
        private readonly RollerConfig Roll1Conf = new RollerConfig() { GetRandomBytes = GetRNG(Roll1()) };

        [TestMethod]
        public void TestBasicRoll()
        {
            var roll = new RollNode(RollType.Normal, One, Twenty);
            var expected = new DieResult()
            {
                DieType = DieType.Normal,
                NumSides = 20,
                Value = 9,
                Flags = 0
            };

            Assert.AreEqual(1, roll.Evaluate(Roll9Conf, roll, 0));
            Assert.AreEqual(9, roll.Value);
            Assert.AreEqual(ResultType.Total, roll.ValueType);
            Assert.AreEqual(1, roll.Values.Count);
            Assert.AreEqual(expected, roll.Values[0]);
        }

        [TestMethod]
        public void TestStandardFudgeRoll()
        {
            var roll = new RollNode(RollType.Fudge, One, null);
            var expected = new DieResult()
            {
                DieType = DieType.Fudge,
                NumSides = 1,
                Value = -1,
                Flags = DieFlags.Fumble
            };

            Assert.AreEqual(1, roll.Evaluate(Roll1Conf, roll, 0));
            Assert.AreEqual(-1, roll.Value);
            Assert.AreEqual(ResultType.Total, roll.ValueType);
            Assert.AreEqual(1, roll.Values.Count);
            Assert.AreEqual(expected, roll.Values[0]);
        }

        [TestMethod]
        public void TestInvalidRolls()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new RollNode(RollType.Normal, null, One));
            Assert.ThrowsException<ArgumentNullException>(() => new RollNode(RollType.Normal, One, null));
            Assert.ThrowsException<ArgumentNullException>(() => new RollNode(RollType.Normal, null, null));
            Assert.ThrowsException<ArgumentNullException>(() => new RollNode(RollType.Fudge, null, One));
            Assert.ThrowsException<ArgumentNullException>(() => new RollNode(RollType.Fudge, null, null));
        }
    }
}
