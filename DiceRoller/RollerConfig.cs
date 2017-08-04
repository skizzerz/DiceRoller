using System;

namespace Dice
{
    /// <summary>
    /// Configuration for a Roller, controlling the maximum number of
    /// dice, sides, and nesting depth, as well as which grammar is used.
    /// </summary>
    public class RollerConfig
    {
        /// <summary>
        /// The maximum number of dice that may be rolled, including dice
        /// rolled due to rerolls or exploding dice. Exceeding this limit
        /// will result in a DiceException being thrown.
        /// Default: 1,000
        /// </summary>
        public int MaxDice { get; set; }

        /// <summary>
        /// The maximum number of sides that any individual die can have.
        /// Exceeding this limit will result in a DiceException being thrown.
        /// Default: 10,000
        /// </summary>
        public int MaxSides { get; set; }

        /// <summary>
        /// The maximum amount of nesting that can happen in dice expressions.
        /// Exceeding this limit will result in a DiceException being thrown.
        /// Default: 20
        /// </summary>
        public short MaxRecursionDepth { get; set; }

        /// <summary>
        /// The maximum amount of times a die can be rerolled, either due to
        /// rerolls, exploding dice, or anything else that rerolls dice.
        /// Once a die is rerolled this many times, the final value is fixed
        /// (no exception is thrown).
        /// Default: 100
        /// </summary>
        public int MaxRerolls { get; set; }

        /// <summary>
        /// If true, only standard dice sizes (d2, d3, d4, d6, d8, d10, d12, d20, d100, d1,000, d10,000)
        /// may be rolled. If false, any die size from 1 to MaxSides, inclusive, can be rolled.
        /// Default: false
        /// </summary>
        public bool NormalSidesOnly { get; set; }

        /// <summary>
        /// If set, this function will be called whenever a random number is needed. The function
        /// should fill the passed-in byte array with a number, which will be used as the die roll.
        /// The BitConverter.GetBytes() method may be useful here, if the random number generator
        /// generates an integer rather than a byte array natively.
        /// If unset, a cryptographically strong random number generator is used to generate die rolls.
        /// Leaving this unset is preferred, unless repeatable results are needed (such as in unit tests).
        /// </summary>
        public Action<byte[]> GetRandomBytes { get; set; }

        /// <summary>
        /// If set, this is the function used to execute macros specified by the roll. If unset, attempting
        /// to use a macro in the roll will result in a DiceException being thrown.
        /// </summary>
        /// <remarks>
        /// The ExecuteMacro property is deprecated and will be removed in a future version. Use the
        /// MacroRegistry property instead.
        /// </remarks>
        [Obsolete("The ExecuteMacro property is deprecated and will be removed in a future version. Use the MacroRegistry property instead.")]
        public MacroCallback ExecuteMacro
        {
            get { return MacroRegistry.GlobalCallbacks; }
            set { MacroRegistry.GlobalCallbacks = value; }
        }

        /// <summary>
        /// Contains the MacroRegistry that maps all known macros to their callbacks.
        /// </summary>
        public MacroRegistry MacroRegistry { get; set; }

        /// <summary>
        /// Contains the FunctionRegistry that maps all known functions to their callbacks.
        /// </summary>
        public FunctionRegistry FunctionRegistry { get; set; }

        /// <summary>
        /// Opaque contextual information used when evaluating dice expressions
        /// </summary>
        internal InternalContext InternalContext { get; set; }

        public RollerConfig()
        {
            MaxDice = 1000;
            MaxSides = 10000;
            MaxRecursionDepth = 20;
            MaxRerolls = 100;
            NormalSidesOnly = false;
            GetRandomBytes = null;
            FunctionRegistry = new FunctionRegistry();
            FunctionRegistry.RegisterType(typeof(BuiltinFunctions));
            InternalContext = new InternalContext();
        }
    }
}
