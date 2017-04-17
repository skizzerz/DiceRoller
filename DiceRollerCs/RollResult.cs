using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dice.AST;

namespace Dice
{
    /// <summary>
    /// Holds the result of a roll, allowing easy access to the total result
    /// as well as the individual die results of the roll.
    /// </summary>
    public class RollResult
    {
        /// <summary>
        /// The result of the roll. This will either be the total or the number of successes.
        /// ResultType can be used to determine which it is.
        /// </summary>
        public decimal Value { get; private set; }

        /// <summary>
        /// The values of the individual dice rolled. This is not necessarily all dice rolled,
        /// just the ones that are exposed to the user. For example, in (1d8)d6, 1d8 is rolled,
        /// and that many d6s are rolled. Values will only contain the results of the d6s.
        /// Inspecting the d8 requires walking the AST beginning at RollRoot.
        /// </summary>
        public IReadOnlyList<DieResult> Values { get; private set; }

        /// <summary>
        /// Whether or not Value represents the roll total or the number of successes.
        /// </summary>
        public ResultType ResultType { get; private set; }

        /// <summary>
        /// The root of the AST for this roll. Accessing this is usually not required,
        /// but is exposed if deeper introspection into the roll is desired.
        /// </summary>
        public DiceAST RollRoot { get; private set; }

        /// <summary>
        /// The number of dice rolls that were needed to fully evaluate this expression.
        /// </summary>
        public int NumRolls { get; private set; }

        internal RollResult(DiceAST rollRoot, int numRolls)
        {
            RollRoot = rollRoot ?? throw new ArgumentNullException("rollRoot");
            // cache some commonly-referenced information directly in this class instead of requiring
            // the user to drill down into RollRoot for everything
            ResultType = rollRoot.ValueType;
            Value = rollRoot.Value;
            Values = rollRoot.Values;
            NumRolls = numRolls;
        }

        /// <summary>
        /// Display a representation of the roll
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(RollRoot.ToString());

            sb.Append(" => ");
            foreach (var die in Values)
            {
                if (die.DieType == DieType.Special)
                {
                    switch ((SpecialDie)die.Value)
                    {
                        case SpecialDie.Add:
                            sb.Append(" + ");
                            break;
                        case SpecialDie.Subtract:
                            sb.Append(" - ");
                            break;
                        case SpecialDie.Multiply:
                            sb.Append(" * ");
                            break;
                        case SpecialDie.Divide:
                            sb.Append(" / ");
                            break;
                        case SpecialDie.OpenParen:
                            sb.Append("(");
                            break;
                        case SpecialDie.CloseParen:
                            sb.Append(")");
                            break;
                        case SpecialDie.Negate:
                            sb.Append("-");
                            break;
                        default:
                            sb.Append("<<UNKOWN SPECIAL>>");
                            break;
                    }
                }
                else
                {
                    if (die.Flags.HasFlag(DieFlags.Success))
                    {
                        sb.Append("$");
                    }
                    else if (die.Flags.HasFlag(DieFlags.Failure))
                    {
                        sb.Append("#");
                    }

                    sb.Append(die.Value);

                    if (die.Flags.HasFlag(DieFlags.Critical) || die.Flags.HasFlag(DieFlags.Fumble))
                    {
                        sb.Append("!");
                    }

                    if (die.Flags.HasFlag(DieFlags.Dropped))
                    {
                        sb.Append("*");
                    }
                }
            }

            sb.Append(" => ");
            sb.Append(Value);

            if (ResultType == ResultType.Successes)
            {
                if (Value == 1)
                {
                    sb.Append(" success");
                }
                else
                {
                    sb.Append(" successes");
                }
            }

            return sb.ToString();
        }
    }
}
