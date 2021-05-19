using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using Dice.AST;
using Dice.Grammar;

namespace Dice
{
    /// <summary>
    /// This class exposes the Roll method, which is used to roll dice.
    /// </summary>
    public static class Roller
    {
        /// <summary>
        /// Default configuration for this Roller, such as maximum number of dice, sides, and nesting depth.
        /// </summary>
        public static RollerConfig DefaultConfig { get; set; }

        static Roller()
        {
            DefaultConfig = new RollerConfig();
        }

        /// <summary>
        /// Rolls dice according to the given dice expression and the default configuration. Please see
        /// the documentation for more details on the formatting for this string.
        /// </summary>
        /// <param name="diceExpr">Dice expression to roll.</param>
        /// <returns>A RollResult containing the details of the roll.</returns>
        public static RollResult Roll(string diceExpr)
        {
            return Roll(diceExpr, null, null);
        }

        /// <summary>
        /// Rolls dice according to the given dice expression and configuration. Please see
        /// the documentation for more details on the formatting for this string.
        /// </summary>
        /// <param name="diceExpr">Dice expression to roll.</param>
        /// <param name="config">Configuration to use. If null, the DefaultConfig is used instead.</param>
        /// <returns>A RollResult containing the details of the roll.</returns>
        public static RollResult Roll(string diceExpr, RollerConfig? config)
        {
            return Roll(diceExpr, config, null);
        }

        /// <summary>
        /// Rolls dice according to the given dice expression and configuration. Please see
        /// the documentation for more details on the formatting for this string.
        /// </summary>
        /// <param name="diceExpr">Dice expression to roll.</param>
        /// <param name="config">Configuration to use. If null, the DefaultConfig is used instead.</param>
        /// <param name="data">Additional data that is scoped to this roll.</param>
        /// <returns>A RollResult containing the details of the roll.</returns>
        public static RollResult Roll(string diceExpr, RollerConfig? config, RollData? data)
        {
            if (diceExpr == null)
            {
                throw new ArgumentNullException(nameof(diceExpr));
            }

            if (data == null)
            {
                data = new RollData();
            }

            if (config == null)
            {
                config = DefaultConfig;
            }

            if (config.MaxDice < 1 || config.MaxRecursionDepth < 0 || config.MaxRerolls < 0 || config.MaxSides < 1)
            {
                throw new InvalidOperationException("RollerConfig is invalid, cannot have negative values for maximums.");
            }

            data.Config = config;

            var root = Parse(diceExpr, data);

            return Roll(root, data);
        }

        /// <summary>
        /// Evaluates the root of the tree, returning the RollResult.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static RollResult Roll(DiceAST root, RollData data)
        {
            var numRolls = root.Evaluate(data, root, 0);

            return new RollResult(data, root, (int)numRolls);
        }

        /// <summary>
        /// Evaluates the dice expression using the default configuration,
        /// and fixing all dice to roll their minimum value.
        /// </summary>
        /// <param name="diceExpr">Dice expression to evaluate</param>
        /// <returns>A RollResult containing the details of the roll.</returns>
        public static RollResult Min(string diceExpr)
        {
            return Min(diceExpr, null, null);
        }

        /// <summary>
        /// Evaluates the dice expression using the default configuration,
        /// and fixing all dice to roll their minimum value.
        /// </summary>
        /// <param name="diceExpr">Dice expression to evaluate</param>
        /// <param name="config">Configuration to use. If null, the DefaultConfig is used instead.</param>
        /// <returns>A RollResult containing the details of the roll.</returns>
        public static RollResult Min(string diceExpr, RollerConfig? config)
        {
            return Min(diceExpr, config, null);
        }

        /// <summary>
        /// Evaluates the dice expression using the default configuration,
        /// and fixing all dice to roll their minimum value.
        /// </summary>
        /// <param name="diceExpr">Dice expression to evaluate</param>
        /// <param name="config">Configuration to use. If null, the DefaultConfig is used instead.</param>
        /// <param name="data">Additional data that is scoped to this roll.</param>
        /// <returns>A RollResult containing the details of the roll.</returns>
        public static RollResult Min(string diceExpr, RollerConfig? config, RollData? data)
        {
            if (config == null)
            {
                config = DefaultConfig;
            }

            var savedRollDie = config.RollDie;
            config.RollDie = (min, max) => min;

            try
            {
                return Roll(diceExpr, config, data);
            }
            finally
            {
                config.RollDie = savedRollDie;
            }
        }

        /// <summary>
        /// Evaluates the dice expression using the default configuration,
        /// and fixing all dice to roll their maximum value.
        /// </summary>
        /// <param name="diceExpr">Dice expression to evaluate</param>
        /// <returns>A RollResult containing the details of the roll.</returns>
        public static RollResult Max(string diceExpr)
        {
            return Max(diceExpr, null, null);
        }

        /// <summary>
        /// Evaluates the dice expression using the default configuration,
        /// and fixing all dice to roll their maximum value.
        /// </summary>
        /// <param name="diceExpr">Dice expression to evaluate</param>
        /// <param name="config">Configuration to use. If null, the DefaultConfig is used instead.</param>
        /// <returns>A RollResult containing the details of the roll.</returns>
        public static RollResult Max(string diceExpr, RollerConfig? config)
        {
            return Max(diceExpr, config, null);
        }

        /// <summary>
        /// Evaluates the dice expression using the default configuration,
        /// and fixing all dice to roll their maximum value.
        /// </summary>
        /// <param name="diceExpr">Dice expression to evaluate</param>
        /// <param name="config">Configuration to use. If null, the DefaultConfig is used instead.</param>
        /// <param name="data">Additional data that is scoped to this roll.</param>
        /// <returns>A RollResult containing the details of the roll.</returns>
        public static RollResult Max(string diceExpr, RollerConfig? config, RollData? data)
        {
            if (config == null)
            {
                config = DefaultConfig;
            }

            var savedRollDie = config.RollDie;
            config.RollDie = (min, max) => max;

            try
            {
                return Roll(diceExpr, config, data);
            }
            finally
            {
                config.RollDie = savedRollDie;
            }
        }

        /// <summary>
        /// Evaluates the dice expression using the default configuration,
        /// and fixing all dice to roll their average value.
        /// </summary>
        /// <param name="diceExpr">Dice expression to evaluate</param>
        /// <returns>A RollResult containing the details of the roll.</returns>
        public static RollResult Average(string diceExpr)
        {
            return Average(diceExpr, null, null);
        }

        /// <summary>
        /// Evaluates the dice expression using the default configuration,
        /// and fixing all dice to roll their average value.
        /// </summary>
        /// <param name="diceExpr">Dice expression to evaluate</param>
        /// <param name="config">Configuration to use. If null, the DefaultConfig is used instead.</param>
        /// <returns>A RollResult containing the details of the roll.</returns>
        public static RollResult Average(string diceExpr, RollerConfig? config)
        {
            return Average(diceExpr, config, null);
        }

        /// <summary>
        /// Evaluates the dice expression using the default configuration,
        /// and fixing all dice to roll their average value.
        /// </summary>
        /// <param name="diceExpr">Dice expression to evaluate</param>
        /// <param name="config">Configuration to use. If null, the DefaultConfig is used instead.</param>
        /// <param name="data">Additional data that is scoped to this roll.</param>
        /// <returns>A RollResult containing the details of the roll.</returns>
        public static RollResult Average(string diceExpr, RollerConfig? config, RollData? data)
        {
            if (config == null)
            {
                config = DefaultConfig;
            }

            if (data == null)
            {
                data = new RollData();
            }

            var savedRollDie = config.RollDie;
            config.RollDie = (min, max) => {
                // we round down if we're doing an odd roll and up on an even roll.
                // e.g. in 2d6 the first die is 3 and the second is 4.
                var avg = (min + max) / 2.0;

                if (data.InternalContext.AllRolls.Count % 2 == 0)
                {
                    // we've completed an even number of rolls, which means the current roll is odd. Round down.
                    return (int)Math.Floor(avg);
                }
                else
                {
                    return (int)Math.Ceiling(avg);
                }
            };

            try
            {
                return Roll(diceExpr, config, data);
            }
            finally
            {
                config.RollDie = savedRollDie;
            }
        }

        /// <summary>
        /// Parses the diceExpr into an AST without evaluating it.
        /// </summary>
        /// <param name="diceExpr"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static DiceAST Parse(string diceExpr, RollData data)
        {
            // parse diceExpr
            var inputStream = new AntlrInputStream(diceExpr);
            var lexer = new DiceGrammarLexer(inputStream) { RollData = data };
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new DiceGrammarParser(tokenStream);
            var walker = new ParseTreeWalker();
            var listener = new DiceGrammarListener(data);

            lexer.AddErrorListener(new DiceErrorListener());
            parser.AddErrorListener(new DiceErrorListener());
            var context = parser.input();
            walker.Walk(listener, context);

            return listener.Root;
        }
    }
}
