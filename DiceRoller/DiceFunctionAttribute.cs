using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice
{
    /// <summary>
    /// An attribute that can be applied to a callback to mark it as a dice function.
    /// Dice functions should have the following signature:
    /// <para>
    /// void DiceFunction(FunctionContext context)
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class DiceFunctionAttribute : Attribute
    {
        /// <summary>
        /// Function name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// "Extra" name, may be null if this function is only available in long
        /// form (.functionName) and not as a shorthand attached to a roll.
        /// Ignored for global functions.
        /// </summary>
        public string? Extra { get; private set; }

        /// <summary>
        /// Function scope
        /// </summary>
        public FunctionScope Scope { get; set; } = FunctionScope.Global;

        /// <summary>
        /// When the function executes in relation to other functions.
        /// Ignored for global functions.
        /// </summary>
        public FunctionTiming Timing { get; set; } = FunctionTiming.Last;

        /// <summary>
        /// Declare this method as a dice function
        /// </summary>
        /// <param name="name">Function name</param>
        public DiceFunctionAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Declare this method as a dice function
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="extra">Extra name</param>
        public DiceFunctionAttribute(string name, string extra)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Extra = extra ?? throw new ArgumentNullException(nameof(extra));
        }
    }
}
