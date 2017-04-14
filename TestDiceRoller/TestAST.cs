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
        private static Action<byte[]> GetRNG(IEnumerable<uint> values)
        {
            var enumerator = values.GetEnumerator();

            void GetRandomBytes(byte[] arr)
            {
                enumerator.MoveNext();
                BitConverter.GetBytes(enumerator.Current).CopyTo(arr, 0);
            }

            return GetRandomBytes;
        }

        private static IEnumerable<uint> Roll9()
        {
            while (true)
            {
                // this is incremented in RollNode.Roll by 1.
                yield return 8;
            }
        }

        private static IEnumerable<uint> Roll1()
        {
            while (true)
            {
                yield return 0;
            }
        }

        private static IEnumerable<uint> Roll20()
        {
            while (true)
            {
                yield return 19;
            }
        }

        private readonly LiteralNode One = new LiteralNode(1);
        private readonly LiteralNode Two = new LiteralNode(2);
        private readonly LiteralNode Twenty = new LiteralNode(20);
        private readonly RollerConfig Roll9Conf = new RollerConfig() { GetRandomBytes = GetRNG(Roll9()) };
        private readonly RollerConfig Roll1Conf = new RollerConfig() { GetRandomBytes = GetRNG(Roll1()) };
        private readonly RollerConfig Roll20Conf = new RollerConfig() { GetRandomBytes = GetRNG(Roll20()) };

        [TestMethod]
        public void TestBasicRoll()
        {
            var roll = new RollNode(RollType.Normal, One, Twenty);
            var roll2 = new RollNode(RollType.Normal, Two, Twenty);
            RollResult result;

            Assert.AreEqual(1, roll.Evaluate(Roll9Conf, roll, 0));
            result = new RollResult(roll, 1);
            Assert.AreEqual("1d20 => 9 => 9", result.ToString());

            Assert.AreEqual(2, roll2.Evaluate(Roll9Conf, roll, 0));
            result = new RollResult(roll2, 2);
            Assert.AreEqual("2d20 => 9 + 9 => 18", result.ToString());

            roll.Evaluate(Roll20Conf, roll, 0);
            result = new RollResult(roll, 1);
            Assert.AreEqual("1d20 => 20! => 20", result.ToString());
        }

        [TestMethod]
        public void TestFudgeRoll()
        {
            var roll = new RollNode(RollType.Fudge, One, null);
            RollResult result;

            Assert.AreEqual(1, roll.Evaluate(Roll1Conf, roll, 0));
            result = new RollResult(roll, 1);
            Assert.AreEqual("1dF => -1! => -1", result.ToString());

            roll = new RollNode(RollType.Fudge, One, Twenty);
            Assert.AreEqual(1, roll.Evaluate(Roll1Conf, roll, 0));
            result = new RollResult(roll, 1);
            Assert.AreEqual("1dF20 => -20! => -20", result.ToString());
        }

        [TestMethod]
        public void TestInvalidRolls()
        {
            var conf = new RollerConfig() { NormalSidesOnly = true };
            Assert.ThrowsException<ArgumentNullException>(() => new RollNode(RollType.Normal, null, One));
            Assert.ThrowsException<ArgumentNullException>(() => new RollNode(RollType.Normal, One, null));
            Assert.ThrowsException<ArgumentNullException>(() => new RollNode(RollType.Normal, null, null));
            Assert.ThrowsException<ArgumentNullException>(() => new RollNode(RollType.Fudge, null, One));
            Assert.ThrowsException<ArgumentNullException>(() => new RollNode(RollType.Fudge, null, null));
            Assert.ThrowsException<DiceException>(() => new RollNode(RollType.Normal, new LiteralNode(-1), Twenty).Evaluate(Roller.DefaultConfig, null, 0));
            Assert.ThrowsException<DiceException>(() => new RollNode(RollType.Normal, new LiteralNode(100000), Twenty).Evaluate(Roller.DefaultConfig, null, 0));
            Assert.ThrowsException<DiceException>(() => new RollNode(RollType.Normal, One, new LiteralNode(100000)).Evaluate(Roller.DefaultConfig, null, 0));
            Assert.ThrowsException<DiceException>(() => new RollNode(RollType.Normal, One, new LiteralNode(11)).Evaluate(conf, null, 0));
        }

        [TestMethod]
        public void TestExplode()
        {
            var nums = new List<uint> { 19, 59, 5, 0 }; // for d20!e => 20, 20, 6. For d20!p => 20, 6(5), 6(5), 1(0)
            var nums2 = new List<uint> { 99, 19, 5, 1 }; // for d100!p => 100, 20(19), 6(5). The 1 is never rolled.
            var conf = new RollerConfig();
            var roll = new RollNode(RollType.Normal, One, Twenty);
            var roll2 = new RollNode(RollType.Normal, One, new LiteralNode(100));
            var comp = new ComparisonNode(CompareOp.Equals, Twenty);
            RollResult result;

            var explode1 = new ExplodeNode(ExplodeType.Explode, false, null) { Expression = roll };
            var explode2 = new ExplodeNode(ExplodeType.Explode, false, comp) { Expression = roll };
            var compound = new ExplodeNode(ExplodeType.Explode, true, null) { Expression = roll };
            var pen1 = new ExplodeNode(ExplodeType.Penetrate, false, null) { Expression = roll };
            var pen2 = new ExplodeNode(ExplodeType.Penetrate, false, comp) { Expression = roll };
            var pen3 = new ExplodeNode(ExplodeType.Penetrate, false, null) { Expression = roll2 };
            var pencompound = new ExplodeNode(ExplodeType.Penetrate, true, null) { Expression = roll };

            conf.GetRandomBytes = GetRNG(nums);
            Assert.AreEqual(3, explode1.Evaluate(conf, explode1, 0));
            result = new RollResult(explode1, 3);
            Assert.AreEqual("1d20.explode() => 20! + 20! + 6 => 46", result.ToString());

            conf.GetRandomBytes = GetRNG(nums);
            Assert.AreEqual(3, explode2.Evaluate(conf, explode2, 0));
            result = new RollResult(explode2, 3);
            Assert.AreEqual("1d20.explode(=20) => 20! + 20! + 6 => 46", result.ToString());

            conf.GetRandomBytes = GetRNG(nums);
            Assert.AreEqual(3, compound.Evaluate(conf, compound, 0));
            result = new RollResult(compound, 3);
            Assert.AreEqual("1d20.compound() => 46! => 46", result.ToString());

            conf.GetRandomBytes = GetRNG(nums);
            Assert.AreEqual(4, pen1.Evaluate(conf, pen1, 0));
            result = new RollResult(pen1, 4);
            Assert.AreEqual("1d20.penetrate() => 20! + 5! + 5! + 0! => 30", result.ToString());

            conf.GetRandomBytes = GetRNG(nums);
            Assert.AreEqual(3, pen2.Evaluate(conf, pen2, 0));
            result = new RollResult(pen2, 3);
            Assert.AreEqual("1d20.penetrate(=20) => 20! + 19! + 5 => 44", result.ToString());

            conf.GetRandomBytes = GetRNG(nums);
            Assert.AreEqual(4, pencompound.Evaluate(conf, pencompound, 0));
            result = new RollResult(pencompound, 4);
            Assert.AreEqual("1d20.compoundPenetrate() => 30! => 30", result.ToString());

            conf.GetRandomBytes = GetRNG(nums2);
            Assert.AreEqual(3, pen3.Evaluate(conf, pen3, 0));
            result = new RollResult(pen3, 3);
            Assert.AreEqual("1d100.penetrate() => 100! + 19! + 5 => 124", result.ToString());
        }
    }
}
