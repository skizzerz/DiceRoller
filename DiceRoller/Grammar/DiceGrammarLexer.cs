using System;
using System.Text.RegularExpressions;

namespace Dice.Grammar
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case",
        Justification = "Generated code")]
    public partial class DiceGrammarLexer
    {
        public RollData? RollData { get; set; }

        private bool IsLikelyDiceExpression()
        {
            if (RollData == null)
            {
                throw new InvalidOperationException("This dice expression requires a RollData instance passed to the lexer");
            }

            // check whether the matched text starts with d followed by F, [, (, or a number
            // or is just "d" by itself
            var regex = new Regex(@"^d[f[(0-9]", RegexOptions.IgnoreCase);
            if (Text != "d" && !regex.IsMatch(Text))
            {
                // doesn't start with the expected beginnings of a dice expression
                return false;
            }

            // check whether this is a global function or roll function
            if (InputStream.Index - Text.Length > 0 && InputStream.LA(-Text.Length) == '.')
            {
                // this is a roll function; can't be a dice expression
                return false;
            }

            // check whether a global function with the given name exists in our scopes
            if (FunctionRegistry.FunctionExists(RollData, Text, FunctionScope.Global))
            {
                // global function exists with this name; prefer calling that instead of doing a dice expression
                return false;
            }

            // if we get here, this is probably a dice expression and not a function call
            return true;
        }
    }
}