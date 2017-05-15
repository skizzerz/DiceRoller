using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice
{
    /// <summary>
    /// Represents a callback for a function
    /// </summary>
    /// <param name="context">Function context, to be filled in by the delegate</param>
    public delegate void FunctionCallback(FunctionContext context);

    /// <summary>
    /// Represents a callback for a macro
    /// </summary>
    /// <param name="context">Macro context, to be filled in by the delegate</param>
    public delegate void MacroCallback(MacroContext context);
}
