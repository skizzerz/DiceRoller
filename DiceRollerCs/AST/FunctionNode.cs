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
        private List<DieResult> _values;

        /// <summary>
        /// The context for this function call
        /// </summary>
        public FunctionContext Context { get; private set; }

        internal FunctionTiming Timing;
        private FunctionCallback Function;

        public override IReadOnlyList<DieResult> Values
        {
            get { return _values; }
        }

        internal FunctionNode(FunctionScope scope, string name, IReadOnlyList<DiceAST> arguments)
        {
            Context = new FunctionContext(scope, name, arguments);
            _values = new List<DieResult>();

            try
            {
                (Timing, Function) = FunctionRegistry.Callbacks[(name, scope)];
            }
            catch (KeyNotFoundException)
            {
                throw new DiceException(DiceErrorCode.NoSuchFunction, name);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Context.Expression?.ToString() ?? String.Empty);
            if (Context.Scope != FunctionScope.Global)
            {
                sb.Append(".");
            }

            sb.Append(Context.Name);
            sb.Append("(");
            sb.Append(String.Join(", ", Context.Arguments.Select(o => o.ToString())));
            sb.Append(")");

            return sb.ToString();
        }

        protected override long EvaluateInternal(RollerConfig conf, DiceAST root, int depth)
        {
            long rolls = Context.Expression?.Evaluate(conf, root, depth + 1) ?? 0;

            foreach (var arg in Context.Arguments)
            {
                rolls += arg.Evaluate(conf, root, depth + 1);
            }

            CallFunction();

            return rolls;
        }

        protected override long RerollInternal(RollerConfig conf, DiceAST root, int depth)
        {
            long rolls = Context.Expression?.Reroll(conf, root, depth + 1) ?? 0;

            foreach (var arg in Context.Arguments)
            {
                rolls += arg.Reroll(conf, root, depth + 1);
            }

            CallFunction();

            return rolls;
        }

        internal override DiceAST GetUnderlyingRollNode()
        {
            if (Context.Expression == null)
            {
                return this;
            }

            return Context.Expression.GetUnderlyingRollNode();
        }

        private void CallFunction()
        {
            Function(Context);
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
        }
    }
}
