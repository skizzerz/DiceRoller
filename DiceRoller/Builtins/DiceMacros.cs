using System;
using System.Collections.Generic;
using System.Text;

namespace Dice.Builtins
{
    /// <summary>
    /// Built-in macros related to the dice rolled so far.
    /// </summary>
    public static class DiceMacros
    {
        /// <summary>
        /// Macro which expands to the number of dice rolled so far.
        /// </summary>
        /// <param name="context">Macro context.</param>
        [DiceMacro("numDice")]
        public static void NumDice(MacroContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Value = context.Data.InternalContext.AllRolls.Count;
            context.ValueType = ResultType.Total;
        }
    }
}
