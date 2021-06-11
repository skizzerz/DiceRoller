using System;
using System.Collections.Generic;

using Dice.Builtins;

// class is deprecated, so suppress some warnings about doc comments
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

namespace Dice
{
    /// <summary>
    /// Built-in functions.
    /// </summary>
    [Obsolete("If you wish to call built-in functions, please call appropriate methods in the Dice.Builtins namespace")]
    public static class BuiltinFunctions
    {
        // these are reserved in every scope
        internal static readonly Dictionary<string, string> ReservedNames = new Dictionary<string, string>
        {
            // key = lowercased name, value = properly-cased name
            { "keephighest", "keepHighest" },
            { "keeplowest", "keepLowest" },
            { "drophighest", "dropHighest" },
            { "droplowest", "dropLowest" },
            { "advantage", "advantage" },
            { "disadvantage", "disadvantage" },
            { "success", "success" },
            { "failure", "failure" },
            { "critical", "critical" },
            { "fumble", "fumble" },
            { "sortasc", "sortAsc" },
            { "sortdesc", "sortDesc" }
        };

        [DiceFunction("floor", Scope = FunctionScope.Global)]
        public static void Floor(FunctionContext context)
        {
            MathFunctions.Floor(context);
        }

        [DiceFunction("ceil", Scope = FunctionScope.Global)]
        public static void Ceiling(FunctionContext context)
        {
            MathFunctions.Ceiling(context);
        }

        [DiceFunction("round", Scope = FunctionScope.Global)]
        public static void Round(FunctionContext context)
        {
            MathFunctions.Round(context);
        }

        [DiceFunction("abs", Scope = FunctionScope.Global)]
        public static void Abs(FunctionContext context)
        {
            MathFunctions.Abs(context);
        }

        [DiceFunction("max", Scope = FunctionScope.Global)]
        public static void Max(FunctionContext context)
        {
            MathFunctions.Max(context);
        }

        [DiceFunction("min", Scope = FunctionScope.Global)]
        public static void Min(FunctionContext context)
        {
            MathFunctions.Min(context);
        }

        [DiceFunction("if", Scope = FunctionScope.Global)]
        public static void If(FunctionContext context)
        {
            ConditionalFunctions.If(context);
        }

        [DiceFunction("expand", Scope = FunctionScope.Group, Timing = FunctionTiming.BeforeSort)]
        public static void Expand(FunctionContext context)
        {
            OutputFunctions.Expand(context);
        }

        [DiceMacro("numDice")]
        public static void NumDice(MacroContext context)
        {
            BuiltinMacros.NumDice(context);
        }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
