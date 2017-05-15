using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using Antlr4.Runtime.Misc;

using Dice.AST;

namespace Dice.Grammar
{
    [CLSCompliant(false)]
    public class DiceGrammarListener : DiceGrammarParserBaseListener
    {
        // holds the state of the current parse tree, the bottom of the stack is the root of the AST,
        // which is accessible via the Root property after parsing is complete.
        private Stack<DiceAST> Stack;
        // holds the RollerConfig that's directing this
        private RollerConfig Config;

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

        public DiceGrammarListener(RollerConfig config)
        {
            Config = config ?? throw new ArgumentNullException("config");
        }

        public override void EnterInput([NotNull] DiceGrammarParser.InputContext context)
        {
            Stack = new Stack<DiceAST>();
        }

        public override void ExitMultMult([NotNull] DiceGrammarParser.MultMultContext context)
        {
            var right = Stack.Pop();
            var left = Stack.Pop();
            Stack.Push(new MathNode(MathOp.Multiply, left, right));
        }

        public override void ExitMultDiv([NotNull] DiceGrammarParser.MultDivContext context)
        {
            var right = Stack.Pop();
            var left = Stack.Pop();
            Stack.Push(new MathNode(MathOp.Divide, left, right));
        }

        public override void ExitAddAdd([NotNull] DiceGrammarParser.AddAddContext context)
        {
            var right = Stack.Pop();
            var left = Stack.Pop();
            Stack.Push(new MathNode(MathOp.Add, left, right));
        }

        public override void ExitAddSub([NotNull] DiceGrammarParser.AddSubContext context)
        {
            var right = Stack.Pop();
            var left = Stack.Pop();
            Stack.Push(new MathNode(MathOp.Subtract, left, right));
        }

        public override void ExitUnaryExprMinus([NotNull] DiceGrammarParser.UnaryExprMinusContext context)
        {
            var param = Stack.Pop();
            Stack.Push(new MathNode(MathOp.Negate, null, param));
        }

        public override void ExitUnaryNumberMinus([NotNull] DiceGrammarParser.UnaryNumberMinusContext context)
        {
            var param = Stack.Pop();
            Stack.Push(new MathNode(MathOp.Negate, null, param));
        }

        public override void ExitNumberLiteral([NotNull] DiceGrammarParser.NumberLiteralContext context)
        {
            Stack.Push(new LiteralNode(Convert.ToDecimal(context.GetText())));
        }

        public override void ExitNumberMacro([NotNull] DiceGrammarParser.NumberMacroContext context)
        {
            // T_MACRO includes the surrounding [], so strip those out (via Substring)
            var macro = context.T_MACRO().GetText();
            Stack.Push(new MacroNode(macro.Substring(1, macro.Length - 2)));
        }

        public override void EnterRollGroup([NotNull] DiceGrammarParser.RollGroupContext context)
        {
            // add GroupPartialNode to act as a sentinel and accumulator for all group stuff
            // note that there might be a node after GroupPartialNode on the stack to indicate NumTimes
            // (said node is rolled into the GroupPartialNode upon parsing the inner group contents)
            Stack.Push(new GroupPartialNode());
        }

        public override void ExitRollGroup([NotNull] DiceGrammarParser.RollGroupContext context)
        {
            // our stack looks like a GroupPartialNode followed by all group_extras and group_functions
            List<DiceAST> groupNodes = new List<DiceAST>();
            for (int i = 0; i < context.grouped_extras().Length + context.group_function().Length; i++)
            {
                groupNodes.Add(Stack.Pop());
            }

            groupNodes.Reverse();
            var partial = (GroupPartialNode)Stack.Pop();

            foreach (var node in groupNodes)
            {
                if (node is RerollNode)
                {
                    partial.AddReroll((RerollNode)node);
                }
                else if (node is KeepNode)
                {
                    partial.AddKeep((KeepNode)node);
                }
                else if (node is SuccessNode)
                {
                    partial.AddSuccess((SuccessNode)node);
                }
                else if (node is SortNode)
                {
                    partial.AddSort((SortNode)node);
                }
                else if (node is FunctionNode)
                {
                    partial.AddFunction((FunctionNode)node);
                }
                else
                {
                    throw new InvalidOperationException("Unexpected node type when resolving group node");
                }
            }

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

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Just a big switch, not complex")]
        public override void ExitGroupFunction([NotNull] DiceGrammarParser.GroupFunctionContext context)
        {
            // we will have N function arguments at the top of the stack
            // note that arguments are popped in reverse order, so we'll need to reverse them into normal order
            // before passing them along to our FunctionNode. Certain functions are hardcoded into aliases for
            // other node types, and those are handled here as well (keephighest, keeplowest, drophighest, droplowest,
            // advantage, disadvantage, success, failure, sortasc, and sortdesc)
            List<DiceAST> args = new List<DiceAST>();

            for (int i = 0; i < context.function_arg().Length; i++)
            {
                args.Add(Stack.Pop());
            }

            args.Reverse();

            // check for a built-in function
            var fname = context.T_IDENTIFIER().GetText().ToLower();
            switch (fname)
            {
                case "reroll":
                    if (args.Count == 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (args.OfType<ComparisonNode>().Count() < args.Count)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    Stack.Push(new RerollNode(0, new ComparisonNode(args.Cast<ComparisonNode>())));
                    break;
                case "rerolln":
                    if (args.Count < 2)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (!(args[0] is LiteralNode || args[0] is MacroNode))
                    {
                        throw new DiceException(DiceErrorCode.BadRerollCount, fname);
                    }

                    if (args.Skip(1).OfType<ComparisonNode>().Count() < args.Count - 1)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    Stack.Push(new RerollNode(-1, new ComparisonNode(args.Skip(1).Cast<ComparisonNode>()), args[0]));
                    break;
                case "rerollonce":
                    if (args.Count == 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (args.OfType<ComparisonNode>().Count() < args.Count)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    Stack.Push(new RerollNode(1, new ComparisonNode(args.Cast<ComparisonNode>())));
                    break;
                case "keephighest":
                    if (args.Count != 1)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (args[0] is ComparisonNode)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    Stack.Push(new KeepNode(KeepType.KeepHigh, args[0]));
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

                    Stack.Push(new KeepNode(KeepType.KeepLow, args[0]));
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

                    Stack.Push(new KeepNode(KeepType.DropHigh, args[0]));
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

                    Stack.Push(new KeepNode(KeepType.DropLow, args[0]));
                    break;
                case "advantage":
                    if (args.Count != 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    Stack.Push(new KeepNode(KeepType.Advantage, null));
                    break;
                case "disadvantage":
                    if (args.Count != 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    Stack.Push(new KeepNode(KeepType.Disadvantage, null));
                    break;
                case "success":
                    if (args.Count == 0 || args.Count > 2)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (args[0] is ComparisonNode && (args.Count == 1 || args[1] is ComparisonNode))
                    {
                        Stack.Push(new SuccessNode((ComparisonNode)args[0], args.Count == 1 ? null : (ComparisonNode)args[1]));
                    }
                    else
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

                    Stack.Push(new SuccessNode(null, (ComparisonNode)args[0]));
                    break;
                case "sortasc":
                    if (args.Count != 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    Stack.Push(new SortNode(SortDirection.Ascending));
                    break;
                case "sortdesc":
                    if (args.Count != 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    Stack.Push(new SortNode(SortDirection.Descending));
                    break;
                default:
                    Stack.Push(new FunctionNode(FunctionScope.Group, fname, args, Config));
                    break;
            }
        }

        public override void ExitRollBasic([NotNull] DiceGrammarParser.RollBasicContext context)
        {
            // we'll have 2 + # of extras nodes on the stack, bottom-most 2 nodes are the number of dice and sides
            List<DiceAST> extras = new List<DiceAST>();
            DiceAST numSides, numDice;

            for (int i = 0; i < context.basic_extras().Length + context.basic_function().Length; i++)
            {
                extras.Add(Stack.Pop());
            }

            extras.Reverse();
            numSides = Stack.Pop();
            numDice = Stack.Pop();
            var partial = new RollPartialNode(new RollNode(RollType.Normal, numDice, numSides));

            foreach (var node in extras)
            {
                if (node is RerollNode)
                {
                    partial.AddReroll((RerollNode)node);
                }
                else if (node is ExplodeNode)
                {
                    partial.AddExplode((ExplodeNode)node);
                }
                else if (node is KeepNode)
                {
                    partial.AddKeep((KeepNode)node);
                }
                else if (node is SuccessNode)
                {
                    partial.AddSuccess((SuccessNode)node);
                }
                else if (node is CritNode)
                {
                    partial.AddCritical((CritNode)node);
                }
                else if (node is SortNode)
                {
                    partial.AddSort((SortNode)node);
                }
                else if (node is FunctionNode)
                {
                    partial.AddFunction((FunctionNode)node);
                }
            }

            Stack.Push(partial.CreateRollNode());
        }

        public override void ExitRollFudge([NotNull] DiceGrammarParser.RollFudgeContext context)
        {
            // we'll have 1 or 2 + # of extras nodes on the stack, bottom-most 2 nodes are the number of dice and sides
            List<DiceAST> extras = new List<DiceAST>();
            DiceAST numSides = null, numDice;

            for (int i = 0; i < context.basic_extras().Length + context.basic_function().Length; i++)
            {
                extras.Add(Stack.Pop());
            }

            extras.Reverse();
            if (context.unary_expr().Length > 1)
            {
                numSides = Stack.Pop();
            }

            numDice = Stack.Pop();
            var partial = new RollPartialNode(new RollNode(RollType.Fudge, numDice, numSides));

            foreach (var node in extras)
            {
                if (node is RerollNode)
                {
                    partial.AddReroll((RerollNode)node);
                }
                else if (node is ExplodeNode)
                {
                    partial.AddExplode((ExplodeNode)node);
                }
                else if (node is KeepNode)
                {
                    partial.AddKeep((KeepNode)node);
                }
                else if (node is SuccessNode)
                {
                    partial.AddSuccess((SuccessNode)node);
                }
                else if (node is CritNode)
                {
                    partial.AddCritical((CritNode)node);
                }
                else if (node is SortNode)
                {
                    partial.AddSort((SortNode)node);
                }
                else if (node is FunctionNode)
                {
                    partial.AddFunction((FunctionNode)node);
                }
            }

            Stack.Push(partial.CreateRollNode());
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Just a big switch, not complex")]
        public override void ExitBasicFunction([NotNull] DiceGrammarParser.BasicFunctionContext context)
        {
            // we will have N function arguments at the top of the stack
            // note that arguments are popped in reverse order, so we'll need to reverse them into normal order
            // before passing them along to our FunctionNode. Certain functions are hardcoded into aliases for
            // other node types, and those are handled here as well
            // (reroll, rerolln, rerollonce, explode, compound, penetrate, compoundpenetrate,
            // keephighest, keeplowest, drophighest, droplowest, advantage, disadvantage,
            // success, failure, sortasc, sortdesc, critical, and fumble)
            List<DiceAST> args = new List<DiceAST>();

            for (int i = 0; i < context.function_arg().Length; i++)
            {
                args.Add(Stack.Pop());
            }

            args.Reverse();

            // check for a built-in function
            var fname = context.T_IDENTIFIER().GetText();
            var lname = fname.ToLower();

            if (BuiltinFunctions.ReservedNames.ContainsKey(lname))
            {
                fname = BuiltinFunctions.ReservedNames[lname];
            }

            switch (lname)
            {
                case "reroll":
                    if (args.Count == 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (args.OfType<ComparisonNode>().Count() < args.Count)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    Stack.Push(new RerollNode(0, new ComparisonNode(args.Cast<ComparisonNode>())));
                    break;
                case "rerolln":
                    if (args.Count < 2)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (!(args[0] is LiteralNode || args[0] is MacroNode))
                    {
                        throw new DiceException(DiceErrorCode.BadRerollCount, fname);
                    }

                    if (args.Skip(1).OfType<ComparisonNode>().Count() < args.Count - 1)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    Stack.Push(new RerollNode(-1, new ComparisonNode(args.Skip(1).Cast<ComparisonNode>()), args[0]));
                    break;
                case "rerollonce":
                    if (args.Count == 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (args.OfType<ComparisonNode>().Count() < args.Count)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    Stack.Push(new RerollNode(1, new ComparisonNode(args.Cast<ComparisonNode>())));
                    break;
                case "explode":
                    if (args.Count == 0)
                    {
                        Stack.Push(new ExplodeNode(ExplodeType.Explode, false, null));
                    }
                    else
                    {
                        if (args.OfType<ComparisonNode>().Count() < args.Count)
                        {
                            throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                        }

                        Stack.Push(new ExplodeNode(ExplodeType.Explode, false, new ComparisonNode(args.Cast<ComparisonNode>())));
                    }
                    break;
                case "compound":
                    if (args.Count == 0)
                    {
                        Stack.Push(new ExplodeNode(ExplodeType.Explode, true, null));
                    }
                    else
                    {
                        if (args.OfType<ComparisonNode>().Count() < args.Count)
                        {
                            throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                        }

                        Stack.Push(new ExplodeNode(ExplodeType.Explode, true, new ComparisonNode(args.Cast<ComparisonNode>())));
                    }
                    break;
                case "penetrate":
                    if (args.Count == 0)
                    {
                        Stack.Push(new ExplodeNode(ExplodeType.Penetrate, false, null));
                    }
                    else
                    {
                        if (args.OfType<ComparisonNode>().Count() < args.Count)
                        {
                            throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                        }

                        Stack.Push(new ExplodeNode(ExplodeType.Penetrate, false, new ComparisonNode(args.Cast<ComparisonNode>())));
                    }
                    break;
                case "compoundpenetrate":
                    if (args.Count == 0)
                    {
                        Stack.Push(new ExplodeNode(ExplodeType.Penetrate, true, null));
                    }
                    else
                    {
                        if (args.OfType<ComparisonNode>().Count() < args.Count)
                        {
                            throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                        }

                        Stack.Push(new ExplodeNode(ExplodeType.Penetrate, true, new ComparisonNode(args.Cast<ComparisonNode>())));
                    }
                    break;
                case "keephighest":
                    if (args.Count != 1)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (args[0] is ComparisonNode)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    Stack.Push(new KeepNode(KeepType.KeepHigh, args[0]));
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

                    Stack.Push(new KeepNode(KeepType.KeepLow, args[0]));
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

                    Stack.Push(new KeepNode(KeepType.DropHigh, args[0]));
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

                    Stack.Push(new KeepNode(KeepType.DropLow, args[0]));
                    break;
                case "advantage":
                    if (args.Count != 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    Stack.Push(new KeepNode(KeepType.Advantage, null));
                    break;
                case "disadvantage":
                    if (args.Count != 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    Stack.Push(new KeepNode(KeepType.Disadvantage, null));
                    break;
                case "success":
                    if (args.Count == 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (args.OfType<ComparisonNode>().Count() < args.Count)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    Stack.Push(new SuccessNode(new ComparisonNode(args.Cast<ComparisonNode>()), null));
                    break;
                case "failure":
                    if (args.Count == 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (args.OfType<ComparisonNode>().Count() < args.Count)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    Stack.Push(new SuccessNode(null, new ComparisonNode(args.Cast<ComparisonNode>())));
                    break;
                case "critical":
                    if (args.Count == 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (args.OfType<ComparisonNode>().Count() < args.Count)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    Stack.Push(new CritNode(new ComparisonNode(args.Cast<ComparisonNode>()), null));
                    break;
                case "fumble":
                    if (args.Count == 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    if (args.OfType<ComparisonNode>().Count() < args.Count)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArgType, fname);
                    }

                    Stack.Push(new CritNode(null, new ComparisonNode(args.Cast<ComparisonNode>())));
                    break;
                case "sortasc":
                    if (args.Count != 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    Stack.Push(new SortNode(SortDirection.Ascending));
                    break;
                case "sortdesc":
                    if (args.Count != 0)
                    {
                        throw new DiceException(DiceErrorCode.IncorrectArity, fname);
                    }

                    Stack.Push(new SortNode(SortDirection.Descending));
                    break;
                default:
                    Stack.Push(new FunctionNode(FunctionScope.Basic, fname, args, Config));
                    break;
            }
        }

        public override void ExitGlobalFunction([NotNull] DiceGrammarParser.GlobalFunctionContext context)
        {
            // we will have N function arguments at the top of the stack
            // note that arguments are popped in reverse order, so we'll need to reverse them into normal order
            // before passing them along to our FunctionNode.
            List<DiceAST> args = new List<DiceAST>();

            for (int i = 0; i < context.function_arg().Length; i++)
            {
                args.Add(Stack.Pop());
            }

            args.Reverse();
            var fname = context.T_IDENTIFIER().GetText().ToLower();
            Stack.Push(new FunctionNode(FunctionScope.Global, fname, args, Config));
        }

        public override void ExitKeepHigh([NotNull] DiceGrammarParser.KeepHighContext context)
        {
            var top = Stack.Pop();
            Stack.Push(new KeepNode(KeepType.KeepHigh, top));
        }

        public override void ExitKeepLow([NotNull] DiceGrammarParser.KeepLowContext context)
        {
            var top = Stack.Pop();
            Stack.Push(new KeepNode(KeepType.KeepLow, top));
        }

        public override void ExitDropHigh([NotNull] DiceGrammarParser.DropHighContext context)
        {
            var top = Stack.Pop();
            Stack.Push(new KeepNode(KeepType.DropHigh, top));
        }

        public override void ExitDropLow([NotNull] DiceGrammarParser.DropLowContext context)
        {
            var top = Stack.Pop();
            Stack.Push(new KeepNode(KeepType.DropLow, top));
        }

        public override void ExitAdvantage([NotNull] DiceGrammarParser.AdvantageContext context)
        {
            Stack.Push(new KeepNode(KeepType.Advantage, null));
        }

        public override void ExitDisadvantage([NotNull] DiceGrammarParser.DisadvantageContext context)
        {
            Stack.Push(new KeepNode(KeepType.Disadvantage, null));
        }

        public override void ExitRerollReroll([NotNull] DiceGrammarParser.RerollRerollContext context)
        {
            var top = (ComparisonNode)Stack.Pop();
            Stack.Push(new RerollNode(0, top));
        }

        public override void ExitRerollOnce([NotNull] DiceGrammarParser.RerollOnceContext context)
        {
            var top = (ComparisonNode)Stack.Pop();
            Stack.Push(new RerollNode(1, top));
        }

        public override void ExitExplode([NotNull] DiceGrammarParser.ExplodeContext context)
        {
            ComparisonNode top = null;
            if (context.compare_expr() != null)
            {
                top = (ComparisonNode)Stack.Pop();
            }

            Stack.Push(new ExplodeNode(ExplodeType.Explode, false, top));
        }

        public override void ExitCompound([NotNull] DiceGrammarParser.CompoundContext context)
        {
            ComparisonNode top = null;
            if (context.compare_expr() != null)
            {
                top = (ComparisonNode)Stack.Pop();
            }

            Stack.Push(new ExplodeNode(ExplodeType.Explode, true, top));
        }

        public override void ExitPenetrate([NotNull] DiceGrammarParser.PenetrateContext context)
        {
            ComparisonNode top = null;
            if (context.compare_expr() != null)
            {
                top = (ComparisonNode)Stack.Pop();
            }

            Stack.Push(new ExplodeNode(ExplodeType.Penetrate, false, top));
        }

        public override void ExitSuccessFail([NotNull] DiceGrammarParser.SuccessFailContext context)
        {
            ComparisonNode fail = null;
            if (context.compare_expr() != null)
            {
                fail = (ComparisonNode)Stack.Pop();
            }

            ComparisonNode success = (ComparisonNode)Stack.Pop();
            Stack.Push(new SuccessNode(success, fail));
        }

        public override void ExitCompImplicit([NotNull] DiceGrammarParser.CompImplicitContext context)
        {
            var top = Stack.Pop();
            Stack.Push(new ComparisonNode(CompareOp.Equals, top));
        }

        public override void ExitCompEquals([NotNull] DiceGrammarParser.CompEqualsContext context)
        {
            var top = Stack.Pop();
            Stack.Push(new ComparisonNode(CompareOp.Equals, top));
        }

        public override void ExitCompGreater([NotNull] DiceGrammarParser.CompGreaterContext context)
        {
            var top = Stack.Pop();
            Stack.Push(new ComparisonNode(CompareOp.GreaterThan, top));
        }

        public override void ExitCompGreaterEquals([NotNull] DiceGrammarParser.CompGreaterEqualsContext context)
        {
            var top = Stack.Pop();
            Stack.Push(new ComparisonNode(CompareOp.GreaterEquals, top));
        }

        public override void ExitCompLess([NotNull] DiceGrammarParser.CompLessContext context)
        {
            var top = Stack.Pop();
            Stack.Push(new ComparisonNode(CompareOp.LessThan, top));
        }

        public override void ExitCompLessEquals([NotNull] DiceGrammarParser.CompLessEqualsContext context)
        {
            var top = Stack.Pop();
            Stack.Push(new ComparisonNode(CompareOp.LessEquals, top));
        }

        public override void ExitCompNotEquals([NotNull] DiceGrammarParser.CompNotEqualsContext context)
        {
            var top = Stack.Pop();
            Stack.Push(new ComparisonNode(CompareOp.NotEquals, top));
        }

        public override void ExitSortAsc([NotNull] DiceGrammarParser.SortAscContext context)
        {
            Stack.Push(new SortNode(SortDirection.Ascending));
        }

        public override void ExitSortDesc([NotNull] DiceGrammarParser.SortDescContext context)
        {
            Stack.Push(new SortNode(SortDirection.Descending));
        }

        public override void ExitCritFumble([NotNull] DiceGrammarParser.CritFumbleContext context)
        {
            ComparisonNode fumb = null;
            if (context.compare_expr().Length > 1)
            {
                fumb = (ComparisonNode)Stack.Pop();
            }

            ComparisonNode crit = (ComparisonNode)Stack.Pop();
            Stack.Push(new CritNode(crit, fumb));
        }

        public override void ExitFumbleOnly([NotNull] DiceGrammarParser.FumbleOnlyContext context)
        {
            var top = (ComparisonNode)Stack.Pop();
            Stack.Push(new CritNode(null, top));
        }
    }
}
