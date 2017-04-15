using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dice;
using Dice.AST;
using Dice.Grammar;

namespace TestDiceRoller.AST
{
    [TestClass]
    public class RollNodeShould : TestBase
    {
        [TestMethod]
        public void Successfully_RollOneDie()
        {
            EvaluateNode(_1d20, Roll9Conf, 1, "1d20 => 9 => 9");
        }

        [TestMethod]
        public void Successfully_RollTwoDice()
        {
            EvaluateNode(_2d20, Roll9Conf, 2, "2d20 => 9 + 9 => 18");
        }

        [TestMethod]
        public void Successfully_RollZeroDice()
        {
            EvaluateNode(new RollNode(RollType.Normal, Zero, Twenty), Roll1Conf, 0, "0d20 => 0 => 0");
        }

        [TestMethod]
        public void FlagCriticals_IfMaxRoll()
        {
            EvaluateNode(_1d20, Roll20Conf, 1, "1d20 => 20! => 20");
        }

        [TestMethod]
        public void FlagFumbles_IfMinRoll()
        {
            EvaluateNode(_1d20, Roll1Conf, 1, "1d20 => 1! => 1");
        }

        [TestMethod]
        public void Successfully_RollStandardFudgeDice()
        {
            EvaluateNode(_1dF, Roll1Conf, 1, "1dF => -1! => -1");
        }

        [TestMethod]
        public void Successfully_RollWackyFudgeDice()
        {
            EvaluateNode(_1dF20, Roll20Conf, 1, "1dF20 => -1 => -1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowArgumentNullException_IfNumDiceIsNullAndRollTypeIsNormal()
        {
            new RollNode(RollType.Normal, null, One);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowArgumentNullException_IfNumDiceIsNullAndRollTypeIsFudge()
        {
            new RollNode(RollType.Fudge, null, One);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowArgumentNullException_IfNumSidesIsNullAndRollTypeIsNormal()
        {
            new RollNode(RollType.Normal, One, null);
        }

        // no test for Successfully_RollFudgeDice_IfNumSidesIsNull since base._1dF already passes null numSides
        // if the test framework doesn't fall over when the class is constructed, we know that case succeeds

        [TestMethod]
        public void ThrowsNegativeDice_IfNumDiceIsNegativeAndRollTypeIsNormal()
        {
            EvaluateNode(new RollNode(RollType.Normal, MinusOne, Twenty), Roll9Conf, DiceErrorCode.NegativeDice);
        }

        [TestMethod]
        public void ThrowsNegativeDice_IfNumDiceIsNegativeAndRollTypeIsFudge()
        {
            EvaluateNode(new RollNode(RollType.Fudge, MinusOne, null), Roll9Conf, DiceErrorCode.NegativeDice);
        }

        [TestMethod]
        public void ThrowsTooManyDice_IfNumDiceIsLargeAndRollTypeIsNormal()
        {
            EvaluateNode(new RollNode(RollType.Normal, OneMillion, Twenty), Roll9Conf, DiceErrorCode.TooManyDice);
        }

        [TestMethod]
        public void ThrowsTooManyDice_IfNumDiceIsLargeAndRollTypeIsFudge()
        {
            EvaluateNode(new RollNode(RollType.Fudge, OneMillion, null), Roll9Conf, DiceErrorCode.TooManyDice);
        }

        [TestMethod]
        public void ThrowsBadSides_IfNumSidesIsZeroAndRollTypeIsNormal()
        {
            EvaluateNode(new RollNode(RollType.Normal, One, Zero), Roll9Conf, DiceErrorCode.BadSides);
        }

        [TestMethod]
        public void ThrowsBadSides_IfNumSidesIsZeroAndRollTypeIsFudge()
        {
            EvaluateNode(new RollNode(RollType.Fudge, One, Zero), Roll9Conf, DiceErrorCode.BadSides);
        }

        [TestMethod]
        public void ThrowsBadSides_IfNumSidesIsNegativeAndRollTypeIsNormal()
        {
            EvaluateNode(new RollNode(RollType.Normal, One, MinusOne), Roll9Conf, DiceErrorCode.BadSides);
        }

        [TestMethod]
        public void ThrowsBadSides_IfNumSidesIsNegativeAndRollTypeIsFudge()
        {
            EvaluateNode(new RollNode(RollType.Fudge, One, MinusOne), Roll9Conf, DiceErrorCode.BadSides);
        }

        [TestMethod]
        public void ThrowsBadSides_IfNumSidesIsLargeAndRollTypeIsNormal()
        {
            EvaluateNode(new RollNode(RollType.Normal, One, OneMillion), Roll9Conf, DiceErrorCode.BadSides);
        }

        [TestMethod]
        public void ThrowsBadSides_IfNumSidesIsLargeAndRollTypeIsFudge()
        {
            EvaluateNode(new RollNode(RollType.Fudge, One, OneMillion), Roll9Conf, DiceErrorCode.BadSides);
        }

        [TestMethod]
        public void ThrowsWrongSides_IfNormalSidesOnlyAndNumSidesIsNonstandardAndRollTypeIsNormal()
        {
            EvaluateNode(new RollNode(RollType.Normal, One, Five), NormalOnlyConf, DiceErrorCode.WrongSides);
        }

        [TestMethod]
        public void ThrowsWrongSides_IfNormalSidesOnlyAndNumSidesIsNonstandardAndRollTypeIsFudge()
        {
            EvaluateNode(new RollNode(RollType.Fudge, One, Five), NormalOnlyConf, DiceErrorCode.WrongSides);
        }

        [TestMethod]
        public void Successfully_RollNormalDice_IfNormalSidesOnly()
        {
            EvaluateNode(_1d20, NormalOnlyConf, 1, "1d20 => 1! => 1");
        }

        [TestMethod]
        public void Successfully_RollStandardFudgeDice_IfNormalSidesOnly()
        {
            EvaluateNode(_1dF, NormalOnlyConf, 1, "1dF => -1! => -1");
        }

        [TestMethod]
        public void Successfully_RollWackyFudgeDice_IfNormalSidesOnly()
        {
            EvaluateNode(_1dF20, NormalOnlyConf, 1, "1dF20 => -20! => -20");
        }

        [TestMethod]
        public async Task BeSufficientlyRandom_WhenUsingDefaultRNG()
        {
            await Task.Run(() =>
            {
                Dictionary<int, int> rollCounts = new Dictionary<int, int>();
                for (int i = 1; i <= 20; i++)
                {
                    rollCounts[i] = 0;
                }

                DieResult die;
                RollerConfig conf = new RollerConfig();
                var numTrials = 10000000;
                var perRoll = numTrials / 20;
                var tolerance = perRoll * 0.005;

                for (int i = 0; i < numTrials; i++)
                {
                    die = RollNode.DoRoll(conf, RollType.Normal, 20);
                    rollCounts[(int)die.Value]++;
                }

                for (int i = 1; i <= 20; i++)
                {
                    int off = Math.Abs(rollCounts[i] - perRoll);
                    if (off > tolerance)
                    {
                        Assert.Inconclusive("Default RNG fell outside of allowed tolerance of 0.5% ({0}/{1})", off, tolerance);
                    }
                }
            });
        }
    }
}
