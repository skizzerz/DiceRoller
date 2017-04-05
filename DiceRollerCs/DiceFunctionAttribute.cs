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
        public string Name { get; private set; }
        public FunctionScope Scope { get; set; } = FunctionScope.Global;
        public FunctionTiming Timing { get; set; } = FunctionTiming.Last;

        public DiceFunctionAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException("name");
        }
    }
}
