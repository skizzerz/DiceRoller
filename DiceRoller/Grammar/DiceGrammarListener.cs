using Antlr4.Runtime.Misc;

using Dice.AST;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Dice.Grammar
{
    [CLSCompliant(false)]
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods",
        Justification = "Class is considered internal (for public API purposes) despite being marked public to align with antlr-generated code visibility")]
    public class DiceGrammarListener : DiceGrammarParserBaseListener
    {
        // holds the state of the current parse tree, the bottom of the stack is the root of the AST,
        // which is accessible via the Root property after parsing is complete.
        private readonly Stack<DiceAST> Stack;
        // holds the RollData that's directing this
        private readonly RollData Data;

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

        public DiceGrammarListener(RollData data)
        {
            Stack = new Stack<DiceAST>();
            Data = data;
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

        public override void ExitFuncMinus([NotNull] DiceGrammarParser.FuncMinusContext context)
        {
            var param = Stack.Pop();
            Stack.Push(new MathNode(MathOp.Negate, null, param));
        }

        public override void ExitUnaryExprMinus([NotNull] DiceGrammarParser.UnaryExprMinusContext context)
        {
            var param = Stack.Pop();
            Stack.Push(new MathNode(MathOp.Negate, null, param));
        }

        public override void ExitNumberLiteral([NotNull] DiceGrammarParser.NumberLiteralContext context)
        {
            Stack.Push(new LiteralNode(Convert.ToDecimal(context.GetText(), CultureInfo.InvariantCulture)));
        }

        public override void ExitNumberMacro([NotNull] DiceGrammarParser.NumberMacroContext context)
        {
            // T_MACRO includes the surrounding [], so strip those out (via Substring)
            var macro = context.T_MACRO().GetText();
            Stack.Push(new MacroNode(macro.Substring(1, macro.Length - 2), Data));
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
            List<DiceAST> groupNodes = new List<DiceAST>();

            // our stack looks like a GroupPartialNode followed by all group_extras and group_functions
            for (int i = 0; i < context.group_function().Length; i++)
            {
                groupNodes.Add(Stack.Pop());
            }

            for (int i = 0; i < context.grouped_extras().Length; i++)
            {
                while (true)
                {
                    var node = Stack.Pop();
                    if (node is SentinelNode s)
                    {
                        if (s.Marker == "GroupExtra")
                        {
                            // reached the end of this particular grouped_extra term
                            break;
                        }
                        else if (s.Marker == "MultipartExtra")
                        {
                            // ignore these nodes now that all extras are resolved
                            continue;
                        }
                    }

                    groupNodes.Add(node);
                }
            }

            groupNodes.Reverse();
            var partial = (GroupPartialNode)Stack.Pop();

            foreach (var node in groupNodes)
            {
                if (node is FunctionNode f)
                {
                    partial.AddFunction(f);
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

            if (partial is GroupPartialNode group)
            {
                // no NumTimes was specified
                group.AddExpression(top);
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

        public override void ExitGroupAdditional([NotNull] DiceGrammarParser.GroupAdditionalContext context)
        {
            var top = Stack.Pop();
            var partial = (GroupPartialNode)Stack.Pop();
            partial.AddExpression(top);
            Stack.Push(partial);
        }

        public override void ExitGroupExtra([NotNull] DiceGrammarParser.GroupExtraContext context)
        {
            // we might have a comparison on the stack
            List<DiceAST> args = new List<DiceAST>();
            if (context.compare_expr() != null)
            {
                args.Add(Stack.Pop());
            }

            // we may have a sentinel on the stack from finishing up a previous extra
            FunctionExtra? currentExtra = null;
            var top = Stack.Peek();
            if (top is SentinelNode sentinel && sentinel.Marker == "MultipartExtra")
            {
                currentExtra = (FunctionExtra?)sentinel.Data;
            }

            // get the extra's name. This may in fact be multiple extras smooshed together without arguments,
            // so check for that as well. If that is the case, arg above is only the argument for the final extra.
            var fname = context.T_EXTRAS().GetText();
            var extras = FunctionRegistry.GetAllExtras(Data, FunctionScope.Group);
            Stack.Push(new SentinelNode("GroupExtra"));

            do
            {
                var startLength = fname.Length;
                var lname = fname.ToLowerInvariant();
                foreach (var extra in extras)
                {
                    if (currentExtra != null)
                    {
                        // check for multipart extras
                        foreach (var follower in currentExtra.MultipartFollowers)
                        {
                            if (follower.Key.Length > 0 && lname.StartsWith(follower.Key, StringComparison.InvariantCulture))
                            {
                                fname = fname.Substring(follower.Key.Length);
                                if (fname.Length == 0)
                                {
                                    Stack.Push(new FunctionNode(FunctionScope.Group, follower.Value, args, Data));
                                }
                                else
                                {
                                    Stack.Push(new FunctionNode(FunctionScope.Group, follower.Value, new List<DiceAST>(), Data));
                                }

                                goto extraProcessed;
                            }
                        }
                    }

                    // if no multipart extras match (or if we're just starting a string of extras),
                    // check for a globally-registered extra
                    if (extra.ExtraName.Length > 0 && lname.StartsWith(extra.ExtraName, StringComparison.InvariantCulture))
                    {
                        fname = fname.Substring(extra.ExtraName.Length);
                        var resolved = FunctionRegistry.GetExtraSlot(Data, extra.ExtraName, FunctionScope.Group);
                        currentExtra = extra;

                        if (fname.Length == 0)
                        {
                            Stack.Push(new FunctionNode(FunctionScope.Group, resolved.Name, args, Data));
                        }
                        else
                        {
                            Stack.Push(new FunctionNode(FunctionScope.Group, resolved.Name, new List<DiceAST>(), Data));
                        }

                        goto extraProcessed;
                    }
                }

            extraProcessed:
                if (fname.Length == startLength)
                {
                    // no extra found
                    throw new DiceException(DiceErrorCode.NoSuchExtra, lname);
                }
            } while (fname.Length > 0);

            // let future invocations know that there may be a continuation from this call
            Stack.Push(new SentinelNode("MultipartExtra", currentExtra));
        }

        public override void ExitGroupEmptyExtra([NotNull] DiceGrammarParser.GroupEmptyExtraContext context)
        {
            if (!FunctionRegistry.ExtraExists(Data, String.Empty, FunctionScope.Group))
            {
                throw new DiceException(DiceErrorCode.NoSuchExtra, String.Empty);
            }

            var resolved = FunctionRegistry.GetExtraSlot(Data, String.Empty, FunctionScope.Group);
            var args = new List<DiceAST>() { Stack.Pop() };

            Stack.Push(new SentinelNode("GroupExtra"));
            Stack.Push(new FunctionNode(FunctionScope.Group, resolved.Name, args, Data));
            Stack.Push(new SentinelNode("MultipartExtra", FunctionRegistry.GetExtraData(Data, String.Empty)));
        }

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

            var fname = context.T_FUNCTION().GetText();
            Stack.Push(new FunctionNode(FunctionScope.Group, fname, args, Data));
        }

        public override void ExitRollBasic([NotNull] DiceGrammarParser.RollBasicContext context)
        {
            List<DiceAST> extras = new List<DiceAST>();
            DiceAST numSides, numDice;

            for (int i = 0; i < context.basic_function().Length; i++)
            {
                extras.Add(Stack.Pop());
            }

            for (int i = 0; i < context.basic_extras().Length; i++)
            {
                while (true)
                {
                    var node = Stack.Pop();
                    if (node is SentinelNode s)
                    {
                        if (s.Marker == "BasicExtra")
                        {
                            // reached the end of this particular grouped_extra term
                            break;
                        }
                        else if (s.Marker == "MultipartExtra")
                        {
                            // ignore these nodes now that all extras are resolved
                            continue;
                        }
                    }

                    extras.Add(node);
                }
            }

            extras.Reverse();
            numSides = Stack.Pop();

            if (context.unary_expr() == null)
            {
                numDice = new LiteralNode(1);
            }
            else
            {
                numDice = Stack.Pop();
            }

            var partial = new RollPartialNode(new RollNode(RollType.Normal, numDice, numSides));

            foreach (var node in extras)
            {
                if (node is FunctionNode f)
                {
                    partial.AddFunction(f);
                }
                else
                {
                    throw new InvalidOperationException("Unexpected node type when resolving basic node");
                }
            }

            Stack.Push(partial.CreateRollNode());
        }

        public override void ExitRollFudge([NotNull] DiceGrammarParser.RollFudgeContext context)
        {
            // we'll have 1 or 2 + # of extras nodes on the stack, bottom-most 2 nodes are the number of dice and sides
            var extras = new List<DiceAST>();
            DiceAST? numSides = null;
            DiceAST numDice;

            for (int i = 0; i < context.basic_function().Length; i++)
            {
                extras.Add(Stack.Pop());
            }

            for (int i = 0; i < context.basic_extras().Length; i++)
            {
                while (true)
                {
                    var node = Stack.Pop();
                    if (node is SentinelNode s)
                    {
                        if (s.Marker == "BasicExtra")
                        {
                            // reached the end of this particular grouped_extra term
                            break;
                        }
                        else if (s.Marker == "MultipartExtra")
                        {
                            // ignore these nodes now that all extras are resolved
                            continue;
                        }
                    }

                    extras.Add(node);
                }
            }

            extras.Reverse();
            if (context.number_expr() != null)
            {
                numSides = Stack.Pop();
            }

            if (context.unary_expr() == null)
            {
                numDice = new LiteralNode(1);
            }
            else
            {
                numDice = Stack.Pop();
            }

            var partial = new RollPartialNode(new RollNode(RollType.Fudge, numDice, numSides));

            foreach (var node in extras)
            {
                if (node is FunctionNode f)
                {
                    partial.AddFunction(f);
                }
                else
                {
                    throw new InvalidOperationException("Unexpected node type when resolving fudge node");
                }
            }

            Stack.Push(partial.CreateRollNode());
        }

        public override void ExitBasicExtra([NotNull] DiceGrammarParser.BasicExtraContext context)
        {
            // we might have a comparison on the stack
            List<DiceAST> args = new List<DiceAST>();
            if (context.compare_expr() != null)
            {
                args.Add(Stack.Pop());
            }

            // we may have a sentinel on the stack from finishing up a previous extra
            FunctionExtra? currentExtra = null;
            var top = Stack.Peek();
            if (top is SentinelNode sentinel && sentinel.Marker == "MultipartExtra")
            {
                currentExtra = (FunctionExtra?)sentinel.Data;
            }

            // get the extra's name. This may in fact be multiple extras smooshed together without arguments,
            // so check for that as well. If that is the case, arg above is only the argument for the final extra.
            var fname = context.T_EXTRAS().GetText();
            var extras = FunctionRegistry.GetAllExtras(Data, FunctionScope.Basic);
            Stack.Push(new SentinelNode("BasicExtra"));

            do
            {
                var startLength = fname.Length;
                var lname = fname.ToLowerInvariant();
                foreach (var extra in extras)
                {
                    if (currentExtra != null)
                    {
                        // check for multipart extras
                        foreach (var follower in currentExtra.MultipartFollowers)
                        {
                            if (follower.Key.Length > 0 && lname.StartsWith(follower.Key, StringComparison.InvariantCulture))
                            {
                                fname = fname.Substring(follower.Key.Length);
                                if (fname.Length == 0)
                                {
                                    Stack.Push(new FunctionNode(FunctionScope.Basic, follower.Value, args, Data));
                                }
                                else
                                {
                                    Stack.Push(new FunctionNode(FunctionScope.Basic, follower.Value, new List<DiceAST>(), Data));
                                }

                                goto extraProcessed;
                            }
                        }
                    }

                    // if no multipart extras match (or if we're just starting a string of extras),
                    // check for a globally-registered extra
                    if (extra.ExtraName.Length > 0 && lname.StartsWith(extra.ExtraName, StringComparison.InvariantCulture))
                    {
                        fname = fname.Substring(extra.ExtraName.Length);
                        var resolved = FunctionRegistry.GetExtraSlot(Data, extra.ExtraName, FunctionScope.Basic);
                        currentExtra = extra;

                        if (fname.Length == 0)
                        {
                            Stack.Push(new FunctionNode(FunctionScope.Basic, resolved.Name, args, Data));
                        }
                        else
                        {
                            Stack.Push(new FunctionNode(FunctionScope.Basic, resolved.Name, new List<DiceAST>(), Data));
                        }

                        goto extraProcessed;
                    }
                }

            extraProcessed:
                if (fname.Length == startLength)
                {
                    // no extra found
                    throw new DiceException(DiceErrorCode.NoSuchExtra, lname);
                }
            } while (fname.Length > 0);

            // let future invocations know that there may be a continuation from this call
            Stack.Push(new SentinelNode("MultipartExtra", currentExtra));
        }

        public override void ExitBasicEmptyExtra([NotNull] DiceGrammarParser.BasicEmptyExtraContext context)
        {
            if (!FunctionRegistry.ExtraExists(Data, String.Empty, FunctionScope.Basic))
            {
                throw new DiceException(DiceErrorCode.NoSuchExtra, String.Empty);
            }

            var resolved = FunctionRegistry.GetExtraSlot(Data, String.Empty, FunctionScope.Basic);
            var args = new List<DiceAST>() { Stack.Pop() };

            Stack.Push(new SentinelNode("BasicExtra"));
            Stack.Push(new FunctionNode(FunctionScope.Basic, resolved.Name, args, Data));
            Stack.Push(new SentinelNode("MultipartExtra", FunctionRegistry.GetExtraData(Data, String.Empty)));
        }

        public override void ExitBasicFunction([NotNull] DiceGrammarParser.BasicFunctionContext context)
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

            var fname = context.T_FUNCTION().GetText();
            Stack.Push(new FunctionNode(FunctionScope.Basic, fname, args, Data));
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
            var fname = context.T_FUNCTION().GetText();
            Stack.Push(new FunctionNode(FunctionScope.Global, fname, args, Data));
        }

        public override void ExitCompImplicit([NotNull] DiceGrammarParser.CompImplicitContext context)
        {
            var top = Stack.Pop();
            Stack.Push(new ImplicitComparisonNode(top));
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
    }
}
