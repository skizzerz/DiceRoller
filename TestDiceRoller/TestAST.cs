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
            var expected = new DieResult()
            {
                DieType = DieType.Normal,
                NumSides = 20,
                Value = 9,
                Flags = 0
            };

            var plus = new DieResult()
            {
                DieType = DieType.Special,
                NumSides = 0,
                Value = (decimal)SpecialDie.Add,
                Flags = 0
            };

            Assert.AreEqual(1, roll.Evaluate(Roll9Conf, roll, 0));
            Assert.AreEqual(9, roll.Value);
            Assert.AreEqual(ResultType.Total, roll.ValueType);
            Assert.AreEqual(1, roll.Values.Count);
            Assert.AreEqual(expected, roll.Values[0]);
            Assert.AreEqual("1d20", roll.ToString());

            Assert.AreEqual(2, roll2.Evaluate(Roll9Conf, roll, 0));
            Assert.AreEqual(18, roll2.Value);
            Assert.AreEqual(3, roll2.Values.Count);
            Assert.AreEqual(expected, roll2.Values[0]);
            Assert.AreEqual(plus, roll2.Values[1]);
            Assert.AreEqual(expected, roll2.Values[2]);
            Assert.AreEqual("2d20", roll2.ToString());

            expected = new DieResult()
            {
                DieType = DieType.Normal,
                NumSides = 20,
                Value = 20,
                Flags = DieFlags.Critical
            };

            roll.Evaluate(Roll20Conf, roll, 0);
            Assert.AreEqual(20, roll.Value);
            Assert.AreEqual(expected, roll.Values[0]);
        }

        [TestMethod]
        public void TestFudgeRoll()
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
            Assert.AreEqual("1dF", roll.ToString());

            roll = new RollNode(RollType.Fudge, One, Twenty);
            expected = new DieResult()
            {
                DieType = DieType.Fudge,
                NumSides = 20,
                Value = -20,
                Flags = DieFlags.Fumble
            };

            Assert.AreEqual(1, roll.Evaluate(Roll1Conf, roll, 0));
            Assert.AreEqual(-20, roll.Value);
            Assert.AreEqual(ResultType.Total, roll.ValueType);
            Assert.AreEqual(1, roll.Values.Count);
            Assert.AreEqual(expected, roll.Values[0]);
            Assert.AreEqual("1dF20", roll.ToString());
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
            var conf = new RollerConfig();
            var roll = new RollNode(RollType.Normal, One, Twenty);
            var comp = new ComparisonNode(CompareOp.Equals, Twenty);

            var e20d20f = new DieResult()
            {
                DieType = DieType.Normal,
                NumSides = 20,
                Value = 20,
                Flags = DieFlags.Critical
            };
            var e46d20 = new DieResult()
            {
                DieType = DieType.Normal,
                NumSides = 20,
                Value = 46,
                Flags = DieFlags.Critical
            };
            var e30d20 = new DieResult()
            {
                DieType = DieType.Normal,
                NumSides = 20,
                Value = 30,
                Flags = DieFlags.Critical
            };
            var e20d20 = new DieResult()
            {
                DieType = DieType.Normal,
                NumSides = 20,
                Value = 20,
                Flags = DieFlags.Critical | DieFlags.Extra
            };
            var e19d20 = new DieResult()
            {
                DieType = DieType.Normal,
                NumSides = 20,
                Value = 19,
                Flags = DieFlags.Critical | DieFlags.Extra
            };
            var e6d20 = new DieResult()
            {
                DieType = DieType.Normal,
                NumSides = 20,
                Value = 6,
                Flags = DieFlags.Extra
            };
            var e5d20 = new DieResult()
            {
                DieType = DieType.Normal,
                NumSides = 20,
                Value = 5,
                Flags = DieFlags.Extra
            };
            var e5d6 = new DieResult()
            {
                DieType = DieType.Normal,
                NumSides = 6,
                Value = 5,
                Flags = DieFlags.Critical | DieFlags.Extra
            };
            var e0d6 = new DieResult()
            {
                DieType = DieType.Normal,
                NumSides = 6,
                Value = 0,
                Flags = DieFlags.Fumble | DieFlags.Extra
            };
            var plus = new DieResult()
            {
                DieType = DieType.Special,
                NumSides = 0,
                Value = (decimal)SpecialDie.Add,
                Flags = 0
            };

            var explode1 = new ExplodeNode(ExplodeType.Explode, false, null) { Expression = roll };
            var explode2 = new ExplodeNode(ExplodeType.Explode, false, comp) { Expression = roll };
            var compound = new ExplodeNode(ExplodeType.Explode, true, null) { Expression = roll };
            var pen1 = new ExplodeNode(ExplodeType.Penetrate, false, null) { Expression = roll };
            var pen2 = new ExplodeNode(ExplodeType.Penetrate, false, comp) { Expression = roll };
            var pencompound = new ExplodeNode(ExplodeType.Penetrate, true, null) { Expression = roll };

            conf.GetRandomBytes = GetRNG(nums);
            Assert.AreEqual(3, explode1.Evaluate(conf, explode1, 0));
            Assert.AreEqual(46, explode1.Value);
            Assert.AreEqual(ResultType.Total, explode1.ValueType);
            Assert.AreEqual(5, explode1.Values.Count);
            Assert.AreEqual(e20d20f, explode1.Values[0]);
            Assert.AreEqual(plus, explode1.Values[1]);
            Assert.AreEqual(e20d20, explode1.Values[2]);
            Assert.AreEqual(plus, explode1.Values[3]);
            Assert.AreEqual(e6d20, explode1.Values[4]);
            Assert.AreEqual("1d20.explode()", explode1.ToString());

            conf.GetRandomBytes = GetRNG(nums);
            Assert.AreEqual(3, explode2.Evaluate(conf, explode2, 0));
            Assert.AreEqual(46, explode2.Value);
            Assert.AreEqual(ResultType.Total, explode2.ValueType);
            Assert.AreEqual(5, explode2.Values.Count);
            Assert.AreEqual(e20d20f, explode2.Values[0]);
            Assert.AreEqual(plus, explode2.Values[1]);
            Assert.AreEqual(e20d20, explode2.Values[2]);
            Assert.AreEqual(plus, explode2.Values[3]);
            Assert.AreEqual(e6d20, explode2.Values[4]);
            Assert.AreEqual("1d20.explode(=20)", explode2.ToString());

            conf.GetRandomBytes = GetRNG(nums);
            Assert.AreEqual(3, compound.Evaluate(conf, compound, 0));
            Assert.AreEqual(46, compound.Value);
            Assert.AreEqual(ResultType.Total, compound.ValueType);
            Assert.AreEqual(1, compound.Values.Count);
            Assert.AreEqual(e46d20, compound.Values[0]);
            Assert.AreEqual("1d20.compound()", compound.ToString());

            conf.GetRandomBytes = GetRNG(nums);
            Assert.AreEqual(4, pen1.Evaluate(conf, pen1, 0));
            Assert.AreEqual(30, pen1.Value);
            Assert.AreEqual(ResultType.Total, pen1.ValueType);
            Assert.AreEqual(7, pen1.Values.Count);
            Assert.AreEqual(e20d20f, pen1.Values[0]);
            Assert.AreEqual(plus, pen1.Values[1]);
            Assert.AreEqual(e5d6, pen1.Values[2]);
            Assert.AreEqual(plus, pen1.Values[3]);
            Assert.AreEqual(e5d6, pen1.Values[4]);
            Assert.AreEqual(plus, pen1.Values[5]);
            Assert.AreEqual(e0d6, pen1.Values[6]);
            Assert.AreEqual("1d20.penetrate()", pen1.ToString());

            conf.GetRandomBytes = GetRNG(nums);
            Assert.AreEqual(3, pen2.Evaluate(conf, pen2, 0));
            Assert.AreEqual(44, pen2.Value);
            Assert.AreEqual(ResultType.Total, pen2.ValueType);
            Assert.AreEqual(5, pen2.Values.Count);
            Assert.AreEqual(e20d20f, pen2.Values[0]);
            Assert.AreEqual(plus, pen2.Values[1]);
            Assert.AreEqual(e19d20, pen2.Values[2]);
            Assert.AreEqual(plus, pen2.Values[3]);
            Assert.AreEqual(e5d20, pen2.Values[4]);
            Assert.AreEqual("1d20.penetrate(=20)", pen2.ToString());

            conf.GetRandomBytes = GetRNG(nums);
            Assert.AreEqual(4, pencompound.Evaluate(conf, pencompound, 0));
            Assert.AreEqual(30, pencompound.Value);
            Assert.AreEqual(ResultType.Total, pencompound.ValueType);
            Assert.AreEqual(1, pencompound.Values.Count);
            Assert.AreEqual(e30d20, pencompound.Values[0]);
            Assert.AreEqual("1d20.compoundPenetrate()", pencompound.ToString());
        }
    }
}
