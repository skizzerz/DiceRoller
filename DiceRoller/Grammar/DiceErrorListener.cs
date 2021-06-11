using System;
using System.Globalization;
using System.IO;

using Antlr4.Runtime;

namespace Dice.Grammar
{
    /// <summary>
    /// Simple error listener which throws a DiceException upon encountering errors.
    /// This class should be considered internal and *not* part of the library's public API.
    /// </summary>
    public class DiceErrorListener : IAntlrErrorListener<IToken>, IAntlrErrorListener<int>
    {
        /// <summary>
        /// Throws a parse error when our parser encounters an error.
        /// </summary>
        /// <param name="output">Output.</param>
        /// <param name="recognizer">Recognizer.</param>
        /// <param name="offendingSymbol">Offending symbol.</param>
        /// <param name="line">Line.</param>
        /// <param name="charPositionInLine">Position in line.</param>
        /// <param name="msg">Error message.</param>
        /// <param name="e">Underlying exception.</param>
        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new DiceException(DiceErrorCode.ParseError, String.Format(CultureInfo.InvariantCulture, "{0}; line {1} position {2}", msg, line, charPositionInLine), e);
        }

        /// <summary>
        /// Throws a parse error when our lexer encounters an error.
        /// </summary>
        /// <param name="output">Output.</param>
        /// <param name="recognizer">Recognizer.</param>
        /// <param name="offendingSymbol">Offending symbol.</param>
        /// <param name="line">Line.</param>
        /// <param name="charPositionInLine">Position in line.</param>
        /// <param name="msg">Error message.</param>
        /// <param name="e">Underlying exception.</param>
        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new DiceException(DiceErrorCode.ParseError, String.Format(CultureInfo.InvariantCulture, "{0}; line {1} position {2}", msg, line, charPositionInLine), e);
        }
    }
}
