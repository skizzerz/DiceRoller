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
        public static RollResult Roll(string diceExpr)
        {
            if (diceExpr == null)
            {
                throw new ArgumentNullException("diceExpr");
            }

            return Roll(diceExpr, null, null);
        }

        /// <summary>
        /// Rolls dice according to the given dice expression and configuration. Please see
        /// the documentation for more details on the formatting for this string.
        /// </summary>
        /// <param name="diceExpr">Dice expression to roll.</param>
        /// <param name="config">Configuration to use. If null, the DefaultConfig is used instead.</param>
        /// <returns>A RollResult containing the details of the roll.</returns>
        public static RollResult Roll(string diceExpr, RollerConfig config)
        {
            if (diceExpr == null)
            {
                throw new ArgumentNullException("diceExpr");
            }

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
        public static RollResult Roll(string diceExpr, RollerConfig config, RollData data)
        {
            if (diceExpr == null)
            {
                throw new ArgumentNullException("diceExpr");
            }

            if (data == null)
            {
                data = new RollData();
            }

            if (config == null)
            {
                config = DefaultConfig;
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
            if (data.Config.MaxDice < 1 || data.Config.MaxRecursionDepth < 0 || data.Config.MaxRerolls < 0 || data.Config.MaxSides < 1)
            {
                throw new InvalidOperationException("RollerConfig is invalid, cannot have negative values for maximums.");
            }

            var numRolls = root.Evaluate(data, root, 0);

            return new RollResult(data, root, (int)numRolls);
        }

        /// <summary>
        /// Parses the diceExpr into an AST without evaluating it.
        /// </summary>
        /// <param name="diceExpr"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static DiceAST Parse(string diceExpr, RollData data)
        {
            if (data.Config.MaxDice < 1 || data.Config.MaxRecursionDepth < 0 || data.Config.MaxRerolls < 0 || data.Config.MaxSides < 1)
            {
                throw new InvalidOperationException("RollerConfig is invalid, cannot have negative values for maximums.");
            }

            // parse diceExpr
            var inputStream = new AntlrInputStream(diceExpr);
            var lexer = new DiceGrammarLexer(inputStream);
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
