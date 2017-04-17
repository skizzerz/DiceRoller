using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;

namespace TestDiceRoller
{
    public class TestBase
    {
        protected static Action<byte[]> GetRNG(params uint[] values)
        {
            return GetRNG((IEnumerable<uint>)values);
        }

        protected static Action<byte[]> GetRNG(IEnumerable<uint> values)
        {
            var enumerator = values.GetEnumerator();

            void GetRandomBytes(byte[] arr)
            {
                enumerator.MoveNext();
                BitConverter.GetBytes(enumerator.Current).CopyTo(arr, 0);
            }

            return GetRandomBytes;
        }

        protected static IEnumerable<uint> Roll9()
        {
            while (true)
            {
                // this is incremented in RollNode.Roll by 1.
                yield return 8;
            }
        }

        protected static IEnumerable<uint> Roll1()
        {
            while (true)
            {
                yield return 0;
            }
        }

        protected static IEnumerable<uint> Roll20()
        {
            while (true)
            {
                yield return 19;
            }
        }

        protected static readonly LiteralNode MinusOne = new LiteralNode(-1);
        protected static readonly LiteralNode Zero = new LiteralNode(0);
        protected static readonly LiteralNode One = new LiteralNode(1);
        protected static readonly LiteralNode Two = new LiteralNode(2);
        protected static readonly LiteralNode Three = new LiteralNode(3);
        protected static readonly LiteralNode Four = new LiteralNode(4);
        protected static readonly LiteralNode Five = new LiteralNode(5);
        protected static readonly LiteralNode Six = new LiteralNode(6);
        protected static readonly LiteralNode Eight = new LiteralNode(8);
        protected static readonly LiteralNode Nineteen = new LiteralNode(19);
        protected static readonly LiteralNode Twenty = new LiteralNode(20);
        protected static readonly LiteralNode OneHundred = new LiteralNode(100);
        protected static readonly LiteralNode OneMillion = new LiteralNode(1000000);

        protected static readonly RollerConfig Roll9Conf = new RollerConfig() { GetRandomBytes = GetRNG(Roll9()) };
        protected static readonly RollerConfig Roll1Conf = new RollerConfig() { GetRandomBytes = GetRNG(Roll1()) };
        protected static readonly RollerConfig Roll20Conf = new RollerConfig() { GetRandomBytes = GetRNG(Roll20()) };
        protected static readonly RollerConfig NormalOnlyConf = new RollerConfig() { NormalSidesOnly = true, GetRandomBytes = GetRNG(Roll1()) };

        protected static readonly RollNode _1d8 = new RollNode(RollType.Normal, One, Eight);
        protected static readonly RollNode _1d20 = new RollNode(RollType.Normal, One, Twenty);
        protected static readonly RollNode _2d20 = new RollNode(RollType.Normal, Two, Twenty);
        protected static readonly RollNode _4d6 = new RollNode(RollType.Normal, Four, Six);
        protected static readonly RollNode _4d20 = new RollNode(RollType.Normal, Four, Twenty);
        protected static readonly RollNode _1d100 = new RollNode(RollType.Normal, One, OneHundred);
        protected static readonly RollNode _1dF = new RollNode(RollType.Fudge, One, null);
        protected static readonly RollNode _1dF20 = new RollNode(RollType.Fudge, One, Twenty);

        protected static readonly ComparisonNode equal20 = new ComparisonNode(CompareOp.Equals, Twenty);
        protected static readonly ComparisonNode equal19 = new ComparisonNode(CompareOp.Equals, Nineteen);
        protected static readonly ComparisonNode equal1 = new ComparisonNode(CompareOp.Equals, One);
        protected static readonly ComparisonNode equal2 = new ComparisonNode(CompareOp.Equals, Two);
        protected static readonly ComparisonNode less5 = new ComparisonNode(CompareOp.LessThan, Five);
        protected static readonly ComparisonNode greaterEqual5 = new ComparisonNode(CompareOp.GreaterEquals, Five);

        /// <summary>
        /// Evaluates the node and asserts that the number of rolls and result match what is expected.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="conf"></param>
        /// <param name="expectedRolls"></param>
        /// <param name="expectedResult"></param>
        protected static void EvaluateNode(DiceAST node, RollerConfig conf, long expectedRolls, string expectedResult)
        {
            long actualRolls = node.Evaluate(conf, node, 0);
            Assert.AreEqual(expectedRolls, actualRolls);

            RollResult result = new RollResult(node, (int)actualRolls);
            Assert.AreEqual(expectedResult, result.ToString());
        }

        /// <summary>
        /// Evaluates the node and asserts that a DiceException was thrown with the specified error code.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="conf"></param>
        /// <param name="error"></param>
        protected static void EvaluateNode(DiceAST node, RollerConfig conf, DiceErrorCode error)
        {
            try
            {
                node.Evaluate(conf, node, 0);
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

        protected static void EvaluateRoll(string diceExpr, RollerConfig conf, int expectedRolls, string expectedResult)
        {
            var result = Roller.Roll(diceExpr, conf);
            Assert.AreEqual(expectedRolls, result.NumRolls);
            Assert.AreEqual(expectedResult, result.ToString());
        }

        protected static void EvaluateRoll(string diceExpr, RollerConfig conf, DiceErrorCode error)
        {
            try
            {
                Roller.Roll(diceExpr, conf);
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

        public static void ExecuteMacro(MacroContext context)
        {
            switch (context.Param)
            {
                case "one":
                    context.Value = 1;
                    break;
                case "two":
                    context.Value = 2;
                    break;
                case "twenty":
                    context.Value = 20;
                    break;
            }
        }
    }
}
