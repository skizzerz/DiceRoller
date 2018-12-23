using System;
using System.Collections.Generic;
using System.Text;

namespace Dice
{
    /// <summary>
    /// Built-in macros which are available in every context.
    /// </summary>
    public static class BuiltinMacros
    {
        [DiceMacro("numDice")]
        public static void NumDice(MacroContext context)
        {
            context.Value = context.Data.InternalContext.AllRolls.Count;
            context.ValueType = ResultType.Total;
        }
    }
}
