using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace Dice.Grammar
{
    /// <summary>
    /// Simple error listener which throws a DiceException upon encountering errors
    /// </summary>
    public class DiceErrorListener : IAntlrErrorListener<IToken>, IAntlrErrorListener<int>
    {
        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new DiceException(DiceErrorCode.ParseError, String.Format(CultureInfo.InvariantCulture, "{0}; line {1} position {2}", msg, line, charPositionInLine), e);
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new DiceException(DiceErrorCode.ParseError, String.Format(CultureInfo.InvariantCulture, "{0}; line {1} position {2}", msg, line, charPositionInLine), e);
        }
    }
}
