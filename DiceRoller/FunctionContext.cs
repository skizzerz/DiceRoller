using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dice.AST;

namespace Dice
{
    /// <summary>
    /// Represents the context of a function call, such as the parameters
    /// and what die expression it is attached to.
    /// </summary>
    public class FunctionContext
    {
        /// <summary>
        /// The scope of this function call.
        /// </summary>
        public FunctionScope Scope { get; private set; }

        /// <summary>
        /// The function name, converted to lowercase.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The arguments to the function.
        /// </summary>
        public IReadOnlyList<DiceAST> Arguments { get; private set; }

        /// <summary>
        /// RollData attached to this function execution
        /// </summary>
        public RollData Data { get; private set; }

        /// <summary>
        /// The dice expression this function is attached to, or null
        /// if this is a global function call.
        /// </summary>
        public DiceAST Expression { get; internal set; }

        /// <summary>
        /// The result of this function call.
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// What sort of value the function call returns.
        /// </summary>
        public ResultType ValueType { get; set; }

        /// <summary>
        /// If dice rolls need to be exposed, set Values to them.
        /// If null, the underlying dice rolls of Expression (if any) are used.
        /// </summary>
        public IEnumerable<DieResult>? Values { get; set; }

        /// <summary>
        /// Number of extra rolls performed during function execution
        /// </summary>
        internal long NumRolls { get; set; }
        
        internal FunctionContext(FunctionScope scope, string name, IReadOnlyList<DiceAST> arguments, RollData data)
        {
            Scope = scope;
            Name = name;
            Arguments = arguments;
            Expression = null!;
            Value = Decimal.MinValue;
            ValueType = ResultType.Total;
            Values = null;
            Data = data;
            NumRolls = 0;
        }

        /// <summary>
        /// Roll an extra die as part of the function execution.
        /// </summary>
        /// <param name="rollType">Type of roll to perform</param>
        /// <param name="numSides">Number of sides to roll</param>
        /// <returns></returns>
        public DieResult RollExtra(RollType rollType, int numSides)
        {
            NumRolls++;
            return RollNode.DoRoll(Data, rollType, numSides, DieFlags.Extra);
        }
    }
}
