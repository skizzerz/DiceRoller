using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice
{
    /// <summary>
    /// An attribute that can be applied to a callback to mark it as a dice macro.
    /// Dice macros should have the following signature:
    /// <code>
    /// void DiceMacro(MacroContext context)
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class DiceMacroAttribute : Attribute
    {
        /// <summary>
        /// Macro name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiceMacroAttribute"/> class
        /// with the given macro name.
        /// </summary>
        /// <param name="name">Macro name to register.</param>
        public DiceMacroAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
