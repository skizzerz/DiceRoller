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

        internal RollResult(ResultType resultType, decimal value, IReadOnlyList<DieResult> values, DiceAST rollRoot)
        {
            ResultType = resultType;
            Value = value;
            Values = values ?? throw new ArgumentNullException("values");
            RollRoot = rollRoot ?? throw new ArgumentNullException("rollRoot");
        }
    }
}
