using System;
using System.Collections.Generic;
using System.Text;

namespace Dice.Builtins
{
    public static class DiceMacros
    {
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
