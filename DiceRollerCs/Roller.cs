using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

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
            return Roll(diceExpr, null);
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
            // validate config
            if (config == null)
            {
                config = DefaultConfig;
            }

            if (config.MaxDice < 1 || config.MaxRecursionDepth < 0 || config.MaxRerolls < 0 || config.MaxSides < 1)
            {
                throw new InvalidOperationException("RollerConfig is invalid, cannot have negative values for maximums.");
            }

            // parse diceExpr
            //diceExpr = DiceGrammarPreprocessor.Preprocess(diceExpr);
            //var lexer = new DiceExpressionLexer(new AntlrInputStream(diceExpr));
            var inputStream = new AntlrInputStream(diceExpr);
            var lexer = new DiceGrammarLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new DiceGrammarParser(tokenStream);
            var walker = new ParseTreeWalker();
            var listener = new DiceGrammarListener(config);

            parser.AddErrorListener(new DiceErrorListener());
            var context = parser.input();
            walker.Walk(listener, context);

            // evaluate diceExpr
            var root = listener.Root;
            var numRolls = root.Evaluate(config, root, 0);

            return new RollResult(root, (int)numRolls);
        }
    }
}
