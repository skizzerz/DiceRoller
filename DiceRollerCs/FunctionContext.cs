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
        /// The dice expression this function is attached to, or null
        /// if this is a global function call.
        /// </summary>
        public DiceAST Expression { get; internal set; }

        /// <summary>
        /// The result of this function call.
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// If dice rolls need to be exposed, set Values to them.
        /// If null, the underlying dice rolls of Expression (if any) are used.
        /// </summary>
        public List<DieResult> Values { get; set; }
        
        internal FunctionContext(FunctionScope scope, string name, IReadOnlyList<DiceAST> arguments)
        {
            Scope = scope;
            Name = name ?? throw new ArgumentNullException("name");
            Arguments = arguments ?? throw new ArgumentNullException("arguments");
            Expression = null;
            Values = null;

            if (Arguments.Any(d => d == null))
            {
                throw new ArgumentException("Function argument list cannot contain null arguments");
            }
        }
    }
}
