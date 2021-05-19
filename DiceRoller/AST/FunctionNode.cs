using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            rolls += CallFunction();

            return rolls;
        }

        protected override long RerollInternal(RollData data, DiceAST root, int depth)
        {
            long rolls = Context.Expression?.Reroll(data, root, depth + 1) ?? 0;

            foreach (var arg in Context.Arguments)
            {
                rolls += arg.Reroll(data, root, depth + 1);
            }

            rolls += CallFunction();

            return rolls;
        }

        private long CallFunction()
        {
            Context.NumRolls = 0;
            Slot.Callback(Context);
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
