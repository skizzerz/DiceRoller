using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;
using Dice.Grammar;

// TODO: Split into multiple tests instead of asserting multiple things per test, it makes live testing not very useful
// Make each node its own test class, and inherit from a base class that implements common stuff (like all the private static stuff, and some predefined rolls too)

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
        private readonly LiteralNode Four = new LiteralNode(4);
        private readonly LiteralNode Six = new LiteralNode(6);
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
            var nums3 = new List<uint> { 9 };
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

            conf.GetRandomBytes = GetRNG(nums3);
            explode1.Evaluate(conf, explode1, 0);
            result = new RollResult(explode1, 1);
            Assert.AreEqual("1d20.explode() => 10 => 10", result.ToString());

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

        [TestMethod]
        public void TestReroll()
        {
            var nums = new List<uint> { 0, 1, 0, 9 }; // 1 2 1 10
            var roll = new RollNode(RollType.Normal, One, Twenty);
            var conf = new RollerConfig();
            var comp = new ComparisonNode(CompareOp.LessEquals, Two);
            RollResult result;

            var reroll1 = new RerollNode(0, comp) { Expression = roll };
            var reroll2 = new RerollNode(1, comp) { Expression = roll };
            var reroll3 = new RerollNode(2, comp, Two) { Expression = roll };

            conf.GetRandomBytes = GetRNG(nums);
            Assert.AreEqual(4, reroll1.Evaluate(conf, reroll1, 0));
            result = new RollResult(reroll1, 4);
            Assert.AreEqual("1d20.reroll(<=2) => 1!* + 2* + 1!* + 10 => 10", result.ToString());

            Assert.AreEqual(1, reroll1.Evaluate(Roll9Conf, reroll1, 0));
            result = new RollResult(reroll1, 4);
            Assert.AreEqual("1d20.reroll(<=2) => 9 => 9", result.ToString());

            conf.GetRandomBytes = GetRNG(nums);
            Assert.AreEqual(2, reroll2.Evaluate(conf, reroll2, 0));
            result = new RollResult(reroll2, 2);
            Assert.AreEqual("1d20.rerollOnce(<=2) => 1!* + 2 => 2", result.ToString());

            conf.GetRandomBytes = GetRNG(nums);
            Assert.AreEqual(3, reroll3.Evaluate(conf, reroll3, 0));
            result = new RollResult(reroll3, 3);
            Assert.AreEqual("1d20.rerollN(2, <=2) => 1!* + 2* + 1! => 1", result.ToString());
        }

        [TestMethod]
        public void TestKeep()
        {
            var nums1 = new List<uint> { 4, 2, 5, 0 }; // 1 3 5 6
            var nums2 = new List<uint> { 1, 18 }; // 2 19
            var roll1 = new RollNode(RollType.Normal, Four, Six);
            var roll2 = new RollNode(RollType.Normal, One, Twenty);
            var conf = new RollerConfig();
            RollResult result;

            var dh1 = new KeepNode(KeepType.DropHigh, One) { Expression = roll1 };
            var dl1 = new KeepNode(KeepType.DropLow, One) { Expression = roll1 };
            var kh1 = new KeepNode(KeepType.KeepHigh, One) { Expression = roll1 };
            var kl1 = new KeepNode(KeepType.KeepLow, One) { Expression = roll1 };
            var ad = new KeepNode(KeepType.Advantage, null) { Expression = roll2 };
            var da = new KeepNode(KeepType.Disadvantage, null) { Expression = roll2 };

            conf.GetRandomBytes = GetRNG(nums1);
            Assert.AreEqual(4, dh1.Evaluate(conf, dh1, 0));
            result = new RollResult(dh1, 4);
            Assert.AreEqual("4d6.dropHighest(1) => 5 + 3 + 6!* + 1! => 9", result.ToString());

            conf.GetRandomBytes = GetRNG(nums1);
            dl1.Evaluate(conf, dl1, 0);
            result = new RollResult(dl1, 4);
            Assert.AreEqual("4d6.dropLowest(1) => 5 + 3 + 6! + 1!* => 14", result.ToString());

            conf.GetRandomBytes = GetRNG(nums1);
            kh1.Evaluate(conf, kh1, 0);
            result = new RollResult(kh1, 4);
            Assert.AreEqual("4d6.keepHighest(1) => 5* + 3* + 6! + 1!* => 6", result.ToString());

            conf.GetRandomBytes = GetRNG(nums1);
            kl1.Evaluate(conf, kl1, 0);
            result = new RollResult(kl1, 4);
            Assert.AreEqual("4d6.keepLowest(1) => 5* + 3* + 6!* + 1! => 1", result.ToString());

            conf.GetRandomBytes = GetRNG(nums2);
            Assert.AreEqual(2, ad.Evaluate(conf, ad, 0));
            result = new RollResult(ad, 2);
            Assert.AreEqual("1d20.advantage() => 2* + 19 => 19", result.ToString());

            conf.GetRandomBytes = GetRNG(nums2);
            Assert.AreEqual(2, da.Evaluate(conf, da, 0));
            result = new RollResult(da, 2);
            Assert.AreEqual("1d20.disadvantage() => 2 + 19* => 2", result.ToString());
        }
    }
}
