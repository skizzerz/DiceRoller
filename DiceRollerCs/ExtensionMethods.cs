using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Dice
{
    internal static class ExtensionMethods
    {
        internal static string GetDescriptionString(this DiceErrorCode code)
        {
            return typeof(DiceErrorCode)
                .GetMember(code.ToString())[0]
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .Cast<DescriptionAttribute>()
                .Single()
                .Description;
        }

        /// <summary>
        /// Determine if the given DieType is a roll or not
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsRoll(this DieType type)
        {
            return type == DieType.Normal || type == DieType.Fudge || type == DieType.Group;
        }

        internal static void MaybeAddPlus(this List<DieResult> values)
        {
            if (values.Count == 0)
            {
                return;
            }
            
            var last = values[values.Count - 1];
            if (last.DieType != DieType.Special || last.SpecialDie != SpecialDie.OpenParen)
            {
                values.Add(new DieResult(SpecialDie.Add));
            }
        }
    }
}
