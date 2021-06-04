using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dice.Builtins
{
    /// <summary>
    /// Registry of built-in functions. These are registered by default
    /// but can be removed if undesired. Any functions registered in other
    /// registries will be used before these should names collide.
    /// </summary>
    public class BuiltinFunctionRegistry : FunctionRegistry
    {
        private readonly bool _finalized;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltinFunctionRegistry"/> class.
        /// </summary>
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
            RegisterType(typeof(KeepFunctions));
            RegisterType(typeof(SuccessFunctions));
            RegisterType(typeof(CritFunctions));
            RegisterType(typeof(SortFunctions));

            // wire multipart extras
            Extras[String.Empty].AddMultipart("f", "failure");
            Extras["cs"].AddMultipart("f", "fumble");

            // validation handlers
            Validate += ExplodeFunctions.ValidateExplode;
            Validate += RerollFunctions.ValidateReroll;
            Validate += KeepFunctions.ValidateKeep;
            Validate += SortFunctions.ValidateSort;

            // mark class as finished; users can remove functions but cannot add any
            _finalized = true;
        }

        /// <inheritdoc/>
        public override void RegisterFunction(FunctionSlot slot, FunctionScope scope, string? extra = null)
        {
            if (_finalized)
            {
                throw new InvalidOperationException("Cannot add items to the builtin function registry");
            }

            base.RegisterFunction(slot, scope, extra);
        }

        /// <summary>
        /// Removes all builtin functions matching the given scope.
        /// This does not clear validation handlers.
        /// </summary>
        /// <param name="scope">Function scope to remove.</param>
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
