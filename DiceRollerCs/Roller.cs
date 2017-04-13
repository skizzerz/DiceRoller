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
        /// Rolls dice according to the given dice expression. Please see
        /// the documentation for more details on the formatting for this string.
        /// </summary>
        /// <param name="diceExpr">Dice expression to roll.</param>
        /// <param name="config">Configuration to use. If null, the DefaultConfig is used instead.</param>
        /// <returns>A RollResult containing the details of the roll.</returns>
        public static RollResult Roll(string diceExpr, RollerConfig config = null)
        {
            // parse diceExpr
            var lexer = new DiceGrammarLexer(new AntlrInputStream(diceExpr));
            var parser = new DiceGrammarParser(new CommonTokenStream(lexer));
            var walker = new ParseTreeWalker();
            var listener = new DiceGrammarListener();

            try
            {
                walker.Walk(listener, parser.input());
            }
            catch (RecognitionException e)
            {
                throw new DiceException(DiceErrorCode.ParseError, e.Message, e);
            }

            // evaluate diceExpr
            var root = listener.Root;
            var numRolls = root.Evaluate(config, root, 0);

            return new RollResult(root);
        }
    }
}
