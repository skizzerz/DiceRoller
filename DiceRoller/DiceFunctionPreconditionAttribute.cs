using System;
using System.Collections.Generic;
using System.Text;

namespace Dice
{
    /// <summary>
    /// An attribute that can be applied to a method to validate all functions attached
    /// to the roll for a particular timing. The method signature must be as follows:
    /// <para>
    /// void DiceFunctionPrecondition(IReadOnlyList&lt;FunctionContext&gt; contexts)
    /// </para>
    /// <para>
    /// The method may throw a DiceException if it detects an invalid combination of functions
    /// Treat all passed-in FunctionContext objects as read-only
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class DiceFunctionPreconditionAttribute : Attribute
    {
        public FunctionTiming Timing { get; private set; }

        public DiceFunctionPreconditionAttribute(FunctionTiming timing)
        {
            Timing = timing;
        }
    }
}
