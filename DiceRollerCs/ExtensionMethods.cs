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
    }
}
