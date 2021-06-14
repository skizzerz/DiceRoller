using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Dice.AST;

namespace Dice
{
    /// <summary>
    /// Extension methods for types defined in the Dice library.
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Get the Description of a particular dice error code.
        /// </summary>
        /// <param name="code">Error code.</param>
        /// <returns>String containing the error code's description.</returns>
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
        /// Determine if the given DieType is a roll or not.
        /// </summary>
        /// <param name="type">DieType to check.</param>
        /// <returns>true if this DieType represents a roll, false otherwise (e.g. special dice).</returns>
        internal static bool IsRoll(this DieType type)
        {
            return type == DieType.Normal || type == DieType.Fudge || type == DieType.Group;
        }

        /// <summary>
        /// Adds an "Add" special die to the list of DieResults if required to separate multiple
        /// regular dice rolls.
        /// </summary>
        /// <param name="values">Values to add an "Add" die to. A die will be added as long as Values is not empty.</param>
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

        /// <summary>
        /// Determine if the given math operation represents a unary operation.
        /// </summary>
        /// <param name="op">Operation to check.</param>
        /// <returns>true if the operation is unary, false otherwise.</returns>
        internal static bool IsUnary(this MathOp op)
        {
            return op switch
            {
                MathOp.Negate => true,
                _ => false,
            };
        }
    }
}
