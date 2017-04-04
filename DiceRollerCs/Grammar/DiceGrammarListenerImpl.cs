using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Dice.AST;
using Dice.Exceptions;

// DiceGrammarListener.cs is taken by the auto-generated IDiceGrammarListener interface
// so we needed a different filename here

namespace Dice.Grammar
{
    public class DiceGrammarListener : DiceGrammarBaseListener
    {
        // holds the state of the current parse tree, the bottom of the stack is the root of the AST,
        // which is accessible via the Root property after parsing is complete.
        private Stack<DiceAST> Stack;

        /// <summary>
        /// Accesses the root of the parsed AST. This is only valid if parsing is complete. If this
        /// listener is used for multiple parses, this value is overridden on each subsequent parse.
        /// </summary>
        public DiceAST Root
        {
            get
            {
                if (Stack.Count != 1)
                {
                    throw new InvalidOperationException("Parse error, stack does not contain exactly one element");
                }

                return Stack.Peek();
            }
        }

        public override void EnterInput([NotNull] DiceGrammarParser.InputContext context)
        {
            Stack = new Stack<DiceAST>();
        }

        public override void ExitMultMult([NotNull] DiceGrammarParser.MultMultContext context)
        {
            var left = Stack.Pop();
            var right = Stack.Pop();
            Stack.Push(new MathNode(MathOp.Multiply, left, right));
        }

        public override void ExitMultDiv([NotNull] DiceGrammarParser.MultDivContext context)
        {
            var left = Stack.Pop();
            var right = Stack.Pop();
            Stack.Push(new MathNode(MathOp.Divide, left, right));
        }

        public override void ExitAddAdd([NotNull] DiceGrammarParser.AddAddContext context)
        {
            var left = Stack.Pop();
            var right = Stack.Pop();
            Stack.Push(new MathNode(MathOp.Add, left, right));
        }

        public override void ExitAddSub([NotNull] DiceGrammarParser.AddSubContext context)
        {
            var left = Stack.Pop();
            var right = Stack.Pop();
            Stack.Push(new MathNode(MathOp.Subtract, left, right));
        }

        public override void ExitNumberLiteral([NotNull] DiceGrammarParser.NumberLiteralContext context)
        {
            Stack.Push(new LiteralNode(Convert.ToDecimal(context.GetText())));
        }

        public override void ExitNumberMacro([NotNull] DiceGrammarParser.NumberMacroContext context)
        {
            Stack.Push(new MacroNode(context.T_STRING().GetText()));
        }


    }
}
