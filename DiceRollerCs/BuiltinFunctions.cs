using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Dice
{
    /// <summary>
    /// Built-in functions.
    /// </summary>
    public static class BuiltinFunctions
    {
        // these are reserved in every scope
        internal static readonly string[] ReservedNames = {
            "reroll", "rerolln", "rerollonce", "explode", "compound", "penetrate",
            "compoundpenetrate", "keephighest", "keeplowest", "drophighest", "droplowest",
            "advantage", "disadvantage", "success", "failure", "critical", "fumble",
            "sortasc", "sortdesc"
        };


    }
}
