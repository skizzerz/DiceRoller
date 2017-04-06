using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Dice.AST;

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

        public override void EnterGroupGroup([NotNull] DiceGrammarParser.GroupGroupContext context)
        {
            // add GroupPartialNode to act as a sentinel and accumulator for all group stuff
            // note that there might be a node after GroupPartialNode on the stack to indicate NumTimes
            // (said node is rolled into the GroupPartialNode upon parsing the inner group contents)
            Stack.Push(new GroupPartialNode());
        }

        public override void ExitGroupGroup([NotNull] DiceGrammarParser.GroupGroupContext context)
        {
            var partial = (GroupPartialNode)Stack.Pop();
            Stack.Push(partial.CreateGroupNode());
        }

        public override void ExitGroupInit([NotNull] DiceGrammarParser.GroupInitContext context)
        {
            var top = Stack.Pop();
            var partial = Stack.Pop();

            if (partial is GroupPartialNode)
            {
                // no NumTimes was specified
                ((GroupPartialNode)partial).AddExpression(top);
                Stack.Push(partial);
            }
            else
            {
                // NumTimes specified
                var partial2 = (GroupPartialNode)Stack.Pop();
                partial2.NumTimes = partial;
                partial2.AddExpression(top);
                Stack.Push(partial2);
            }
        }

        public override void ExitGroupExtra([NotNull] DiceGrammarParser.GroupExtraContext context)
        {
            var top = Stack.Pop();
            var partial = (GroupPartialNode)Stack.Pop();
            partial.AddExpression(top);
            Stack.Push(partial);
        }

        public override void ExitGroupKeep([NotNull] DiceGrammarParser.GroupKeepContext context)
        {
            var keep = (KeepNode)Stack.Pop();
            var partial = (GroupPartialNode)keep.Expression;
            partial.AddKeep(keep);
            Stack.Push(partial);
        }

        public override void ExitGroupSuccess([NotNull] DiceGrammarParser.GroupSuccessContext context)
        {
            var success = (SuccessNode)Stack.Pop();
            var partial = (GroupPartialNode)success.Expression;
            partial.AddSuccess(success.Success);
            partial.AddFailure(success.Failure);
            Stack.Push(partial);
        }

        public override void ExitGroupSort([NotNull] DiceGrammarParser.GroupSortContext context)
        {
            var sort = (SortNode)Stack.Pop();
            var partial = (GroupPartialNode)sort.Expression;
            partial.AddSort(sort);
            Stack.Push(partial);
        }

        public override void ExitGroupFunction([NotNull] DiceGrammarParser.GroupFunctionContext context)
        {
            // we will have N function arguments at the top of the stack, followed by the GroupPartialNode
            // note that arguments are popped in reverse order, so we'll need to reverse them into normal order
            // before passing them along to our FunctionNode. Certain functions are hardcoded into aliases for
            // other node types, and those are handled here as well (keephighest, keeplowest, drophighest, droplowest,
            // advantage, disadvantage, success, failure, sortasc, and sortdesc)
            List<DiceAST> args = new List<DiceAST>();
            DiceAST top = Stack.Pop();

            while (!(top is GroupPartialNode))
            {
                args.Add(top);
                top = Stack.Pop();
            }

            // top is our Partial node, and has already been popped off the stack
            args.Reverse();
            var partial = (GroupPartialNode)top;

            // check for a built-in function
            var fname = context.T_ALPHA_STRING().GetText().ToLower();
            switch (fname)
            {
                case "keephighest":
                    if (args.Count != 1)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (args[0] is ComparisonNode)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    partial.AddKeep(new KeepNode(KeepType.KeepHigh, args[0], partial));
                    break;
                case "keeplowest":
                    if (args.Count != 1)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (args[0] is ComparisonNode)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    partial.AddKeep(new KeepNode(KeepType.KeepLow, args[0], partial));
                    break;
                case "drophighest":
                    if (args.Count != 1)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (args[0] is ComparisonNode)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    partial.AddKeep(new KeepNode(KeepType.DropHigh, args[0], partial));
                    break;
                case "droplowest":
                    if (args.Count != 1)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (args[0] is ComparisonNode)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    partial.AddKeep(new KeepNode(KeepType.DropLow, args[0], partial));
                    break;
                case "advantage":
                    if (args.Count != 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    partial.AddKeep(new KeepNode(KeepType.Advantage, null, partial));
                    break;
                case "disadvantage":
                    if (args.Count != 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    partial.AddKeep(new KeepNode(KeepType.Disadvantage, null, partial));
                    break;
                case "success":
                    if (args.Count == 0 || args.Count > 2)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (args[0] is ComparisonNode)
                    {
                        partial.AddSuccess((ComparisonNode)args[0]);
                    }
                    else
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    if (args.Count == 2 && args[1] is ComparisonNode)
                    {
                        partial.AddFailure((ComparisonNode)args[1]);
                    }
                    else if (args.Count == 2)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    break;
                case "failure":
                    if (args.Count != 1)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (!(args[0] is ComparisonNode))
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    partial.AddFailure((ComparisonNode)args[0]);
                    break;
                case "sortasc":
                    if (args.Count != 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    partial.AddSort(new SortNode(SortDirection.Ascending, partial));
                    break;
                case "sortdesc":
                    if (args.Count != 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    partial.AddSort(new SortNode(SortDirection.Descending, partial));
                    break;
                default:
                    partial.AddFunction(new FunctionNode(FunctionScope.Group, fname, args, partial));
                    break;
            }

            Stack.Push(partial);
        }
    }
}
