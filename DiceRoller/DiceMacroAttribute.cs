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
    /// <para>
    /// void DiceMacro(MacroContext context)
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class DiceMacroAttribute : Attribute
    {
        public string Name { get; private set; }

        public DiceMacroAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException("name");
        }
    }
}
