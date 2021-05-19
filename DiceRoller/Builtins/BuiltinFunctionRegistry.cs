using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Dice.Builtins
{
    public class BuiltinFunctionRegistry : FunctionRegistry
    {
        private readonly bool _finalized;

        internal BuiltinFunctionRegistry()
        {
            _finalized = false;

            // global functions
            RegisterType(typeof(ConditionalFunctions));
            RegisterType(typeof(MathFunctions));
            RegisterType(typeof(OutputFunctions));

            // roll functions
            RegisterType(typeof(ExplodeFunctions));
            RegisterType(typeof(RerollFunctions));

            // validation handlers
            Validate += ExplodeFunctions.ValidateExplode;
            Validate += RerollFunctions.ValidateReroll;

            // mark class as finished; users can remove functions but cannot add any
            _finalized = true;
        }

        public override void RegisterFunction(string name, string? extra, FunctionCallback callback, FunctionScope scope, FunctionTiming timing, FunctionBehavior behavior)
        {
            if (_finalized)
            {
                throw new InvalidOperationException("Cannot add items to the builtin function registry");
            }

            base.RegisterFunction(name, extra, callback, scope, timing, behavior);
        }

        /// <summary>
        /// Removes all builtin functions matching the given scope.
        /// This does not clear validation handlers
        /// </summary>
        /// <param name="scope"></param>
        public void Clear(FunctionScope scope)
        {
            if (scope == FunctionScope.All)
            {
                Clear(FunctionScope.Global);
                Clear(FunctionScope.Basic);
                Clear(FunctionScope.Group);
                return;
            }
            else if (scope == FunctionScope.Roll)
            {
                Clear(FunctionScope.Basic);
                Clear(FunctionScope.Group);
                return;
            }

            var toRemove = Callbacks.Where(kv => kv.Key.scope == scope).Select(kv => kv.Key).ToList();
            foreach (var callback in toRemove)
            {
                Remove(callback.lname, callback.scope);
            }
        }
    }
}
