using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dice.AST
{
    /// <summary>
    /// Represents a function call, either a global function
    /// or one attached to a roll.
    /// </summary>
    public class FunctionNode : DiceAST
    {
        private readonly List<DieResult> _values;

        /// <summary>
        /// The context for this function call
        /// </summary>
        public FunctionContext Context { get; private set; }

        internal readonly FunctionSlot Slot;

        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        protected internal override DiceAST UnderlyingRollNode => Context.Expression?.UnderlyingRollNode ?? this;

        internal FunctionNode(FunctionScope scope, string name, IReadOnlyList<DiceAST> arguments, RollData data)
        {
            try
            {
                Slot = FunctionRegistry.GetFunction(data, name, scope);
                Context = new FunctionContext(scope, Slot.Name, arguments, data);
                _values = new List<DieResult>();
            }
            catch (KeyNotFoundException)
            {
                throw new DiceException(DiceErrorCode.NoSuchFunction, name);
            }
        }

        public bool IsGlobalFunction()
        {
            return Context.Scope == FunctionScope.Global;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Context.Expression?.ToString() ?? String.Empty);
            if (!IsGlobalFunction())
            {
                sb.Append('.');
            }

            sb.Append(Context.Name);
            sb.Append('(');
            sb.Append(String.Join(", ", Context.Arguments.Select(o => o.ToString())));
            sb.Append(')');

            return sb.ToString();
        }

        protected override long EvaluateInternal(RollData data, DiceAST root, int depth)
        {
            long rolls = Context.Expression?.Evaluate(data, root, depth + 1) ?? 0;

            foreach (var arg in Context.Arguments)
            {
                rolls += arg.Evaluate(data, root, depth + 1);
            }

            rolls += CallFunction(root, depth);

            return rolls;
        }

        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            long rolls = Context.Expression?.Reroll(data, root, depth + 1) ?? 0;

            foreach (var arg in Context.Arguments)
            {
                rolls += arg.Reroll(data, root, depth + 1);
            }

            rolls += CallFunction(root, depth);

            return rolls;
        }

        private long CallFunction(DiceAST root, int depth)
        {
            // validate arguments if an argument pattern was specified
            if (Slot.ArgumentPattern != null)
            {
                // argument string where ImplicitComparisonNodes are converted into Expressions
                var argStrImpE = new string(Context.Arguments.Select(a => a.GetType() == typeof(ComparisonNode) ? 'C' : 'E').ToArray());

                // argument string where ImplicitComparisonNodes are converted into Comparisons
                var argStrImpC = new string(Context.Arguments.Select(a => a is ComparisonNode ? 'C' : 'E').ToArray());

                var argTypeRegex = new Regex($"^{Slot.ArgumentPattern}$");
                var argArityRegex = new Regex($"^{Slot.ArgumentPattern.Replace('E', '.').Replace('C', '.')}$");

                if (!argArityRegex.IsMatch(argStrImpE))
                {
                    throw new DiceException(DiceErrorCode.IncorrectArity, Slot.Name);
                }

                if (argTypeRegex.IsMatch(argStrImpE))
                {
                    // ImplicitComparisonNode can only ever appear as the very first argument
                    if (Context.Arguments.Count > 0 && Context.Arguments[0] is ImplicitComparisonNode ic)
                    {
                        ic.IsExpression = true;
                    }
                }
                else if (!argTypeRegex.IsMatch(argStrImpC))
                {
                    throw new DiceException(DiceErrorCode.IncorrectArgType, Slot.Name);
                }
            }

            // call the function
            Context.NumRolls = 0;
            Context.Root = root;
            Context.Depth = depth;
            Slot.Callback(Context);
            Context.Root = null;
            Context.Depth = null;
            Value = Context.Value;
            ValueType = Context.ValueType;
            _values.Clear();

            if (Context.Values != null)
            {
                _values.AddRange(Context.Values);
            }
            else if (Context.Expression?.Values != null)
            {
                _values.AddRange(Context.Expression.Values);
            }
            else
            {
                _values.Add(new DieResult()
                {
                    DieType = DieType.Literal,
                    NumSides = 0,
                    Value = Context.Value,
                    Flags = DieFlags.Macro // this techincally isn't a macro but it is acting like one if Values is empty, ergo set this flag
                });
            }

            if (Context.Value == Decimal.MinValue)
            {
                throw new InvalidOperationException("Function callback did not modify context.Value");
            }

            return Context.NumRolls;
        }
    }
}
