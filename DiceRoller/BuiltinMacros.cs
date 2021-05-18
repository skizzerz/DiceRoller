using System;
using System.Collections.Generic;
using System.Text;

using Dice.Builtins;

namespace Dice
{
    /// <summary>
    /// Built-in macros which are available in every context.
    /// </summary>
    [Obsolete("If you wish to call built-in functions, please call appropriate methods in the Dice.Builtins namespace")]
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
