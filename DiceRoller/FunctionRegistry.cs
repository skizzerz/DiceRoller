using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice
{
    /// <summary>
    /// Stores a mapping of all functions to their appropriate callbacks.
    /// Functions registered here can specify when they should be executed in relation
    /// to built-in dice expressions.
    /// </summary>
    public class FunctionRegistry
    {
        protected readonly Dictionary<(string lname, FunctionScope scope), FunctionSlot> Callbacks = new Dictionary<(string, FunctionScope), FunctionSlot>();
        protected readonly Dictionary<string, FunctionExtra> Extras = new Dictionary<string, FunctionExtra>();

        public event EventHandler<ValidateEventArgs>? Validate;

        public static bool FunctionExists(RollData data, string name, FunctionScope scope)
        {
            return data.FunctionRegistry.Contains(name, scope)
                || data.Config.FunctionRegistry.Contains(name, scope)
                || data.Config.BuiltinFunctionRegistry.Contains(name, scope);
        }

        public static bool ExtraExists(RollData data, string name, FunctionScope scope)
        {
            if (scope == FunctionScope.All || scope == FunctionScope.Global)
            {
                throw new ArgumentException("Scope cannot be All or Global", nameof(scope));
            }

            return data.FunctionRegistry.ContainsExtra(name, scope)
                || data.Config.FunctionRegistry.ContainsExtra(name, scope)
                || data.Config.BuiltinFunctionRegistry.ContainsExtra(name, scope);
        }

        public static FunctionSlot GetFunction(RollData data, string name, FunctionScope scope)
        {
            if (scope == FunctionScope.All || scope == FunctionScope.Roll)
            {
                throw new ArgumentException("Scope cannot be All or Roll", nameof(scope));
            }

            var lname = name.ToLowerInvariant();

            if (data.FunctionRegistry.Contains(lname, scope))
            {
                return data.FunctionRegistry.Callbacks[(lname, scope)];
            }

            if (data.Config.FunctionRegistry.Contains(lname, scope))
            {
                return data.Config.FunctionRegistry.Callbacks[(lname, scope)];
            }

            if (data.Config.BuiltinFunctionRegistry.Contains(lname, scope))
            {
                return data.Config.BuiltinFunctionRegistry.Callbacks[(lname, scope)];
            }

            throw new KeyNotFoundException($"No registered function matches the name and scope ({name}, FunctionScope.{scope})");
        }

        public static FunctionSlot GetExtraSlot(RollData data, string name, FunctionScope scope)
        {
            if (scope == FunctionScope.All || scope == FunctionScope.Roll || scope == FunctionScope.Global)
            {
                throw new ArgumentException("Scope cannot be All, Roll, or Global", nameof(scope));
            }

            var lname = name.ToLowerInvariant();

            if (data.FunctionRegistry.ContainsExtra(lname, scope))
            {
                return data.FunctionRegistry.Callbacks[(data.FunctionRegistry.Extras[lname].FunctionName, scope)];
            }

            if (data.Config.FunctionRegistry.ContainsExtra(name, scope))
            {
                return data.Config.FunctionRegistry.Callbacks[(data.Config.FunctionRegistry.Extras[lname].FunctionName, scope)];
            }

            if (data.Config.BuiltinFunctionRegistry.ContainsExtra(name, scope))
            {
                return data.Config.BuiltinFunctionRegistry.Callbacks[(data.Config.BuiltinFunctionRegistry.Extras[lname].FunctionName, scope)];
            }

            throw new KeyNotFoundException($"No registered top-level extra matches the name and scope ({name}, FunctionScope.{scope})");
        }

        public static FunctionExtra GetExtraData(RollData data, string name)
        {
            var lname = name.ToLowerInvariant();

            if (data.FunctionRegistry.Extras.ContainsKey(lname))
            {
                return data.FunctionRegistry.Extras[lname];
            }

            if (data.Config.FunctionRegistry.Extras.ContainsKey(lname))
            {
                return data.Config.FunctionRegistry.Extras[lname];
            }

            if (data.Config.BuiltinFunctionRegistry.Extras.ContainsKey(lname))
            {
                return data.Config.BuiltinFunctionRegistry.Extras[lname];
            }

            throw new KeyNotFoundException($"There is no registered top-level extra named \"{name}\"");
        }

        /// <summary>
        /// Retrieve all extras registered for the given scope in descending order by string length
        /// </summary>
        /// <param name="data"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        internal static List<FunctionExtra> GetAllExtras(RollData data, FunctionScope scope)
        {
            var extras = new List<FunctionExtra>();

            // user-defined extras attached to the current roll
            extras.AddRange(data.FunctionRegistry.Extras.Where(e => data.FunctionRegistry.Contains(e.Value.FunctionName, scope)).Select(e => e.Value));

            // user-defined extras attached to global config
            foreach (var extra in data.Config.FunctionRegistry.Extras.Where(e => data.Config.FunctionRegistry.Contains(e.Value.FunctionName, scope)).Select(e => e.Value))
            {
                if (!extras.Any(e => e.ExtraName == extra.ExtraName))
                {
                    extras.Add(extra);
                }
            }

            // built-in extras
            foreach (var extra in data.Config.BuiltinFunctionRegistry.Extras.Where(e => data.Config.BuiltinFunctionRegistry.Contains(e.Value.FunctionName, scope)).Select(e => e.Value))
            {
                if (!extras.Any(e => e.ExtraName == extra.ExtraName))
                {
                    extras.Add(extra);
                }
            }

            return extras.OrderByDescending(e => e.ExtraName.Length).ToList();
        }

        /// <summary>
        /// Fire the Validate event on all registries associated with this roll.
        /// An event handler may throw an exception on validation failure to indicate an invalid set of function calls.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="eventArgs"></param>
        internal static void FireValidateEvent(RollData data, ValidateEventArgs eventArgs)
        {
            data.FunctionRegistry.Validate?.Invoke(data.FunctionRegistry, eventArgs);
            data.Config.FunctionRegistry.Validate?.Invoke(data.Config.FunctionRegistry, eventArgs);
            data.Config.BuiltinFunctionRegistry.Validate?.Invoke(data.Config.BuiltinFunctionRegistry, eventArgs);
        }

        /// <summary>
        /// Registers a type, which causes all public static methods of that type with the
        /// DiceFunctionAttribute to be registered as callbacks. To register instance methods as well
        /// as static methods, use the other RegisterType overload.
        /// </summary>
        /// <param name="t">Type to register methods from</param>
        public void RegisterType(Type t)
        {
            foreach (var m in t.GetMethods().Where(m => m.IsPublic && m.IsStatic))
            {
                var attr = m.GetCustomAttributes(typeof(DiceFunctionAttribute), false).Cast<DiceFunctionAttribute>().SingleOrDefault();
                if (attr != null)
                {
                    var paras = m.GetParameters();
                    if (m.ReturnType != typeof(void) || paras.Length != 1 || paras[0].ParameterType != typeof(FunctionContext))
                    {
                        throw new InvalidOperationException("A DiceFunctionAttribute can only be applied to a FunctionCallback");
                    }

                    var callback = (FunctionCallback)m.CreateDelegate(typeof(FunctionCallback));
                    var slot = new FunctionSlot(attr.Name, callback, attr.Timing, attr.Behavior, attr.ArgumentPattern);
                    RegisterFunction(slot, attr.Scope, attr.Extra);
                }
            }
        }

        /// <summary>
        /// Registers a type given an object of that type, which causes all public static methods of that type
        /// as well as all public instance methods of that type with the DiceFunctionAttribute to be registered
        /// as callbacks. The passed-in object will be used when calling instance methods.
        /// </summary>
        /// <typeparam name="T">Type whose methods will be registered.</typeparam>
        /// <param name="obj">Object to use when invoking instance methods.</param>
        public void RegisterType<T>(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            foreach (var m in obj.GetType().GetMethods().Where(m => m.IsPublic))
            {
                var attr = m.GetCustomAttributes(typeof(DiceFunctionAttribute), false).Cast<DiceFunctionAttribute>().SingleOrDefault();
                if (attr != null)
                {
                    var paras = m.GetParameters();
                    if (m.ReturnType != typeof(void) || paras.Length != 1 || paras[0].ParameterType != typeof(FunctionContext))
                    {
                        throw new InvalidOperationException("A DiceFunctionAttribute can only be applied to a FunctionCallback");
                    }

                    FunctionCallback callback;
                    if (m.IsStatic)
                    {
                        callback = (FunctionCallback)m.CreateDelegate(typeof(FunctionCallback));
                    }
                    else
                    {
                        callback = (FunctionCallback)m.CreateDelegate(typeof(FunctionCallback), obj);
                    }

                    var slot = new FunctionSlot(attr.Name, callback, attr.Timing, attr.Behavior, attr.ArgumentPattern);
                    RegisterFunction(slot, attr.Scope, attr.Extra);
                }
            }
        }

        /// <summary>
        /// Registers the specified global callback to the given name.
        /// </summary>
        /// <param name="name">Function name to register. Function names are case-insensitive.</param>
        /// <param name="callback">The method to be called whenever the function is called in a dice expression.</param>
        public void RegisterFunction(string name, FunctionCallback callback)
        {
            RegisterFunction(new FunctionSlot(name, callback), FunctionScope.Global);
        }

        /// <summary>
        /// Registers the specified callback to the given name and scope.
        /// </summary>
        /// <param name="name">Function name to register. Function names are case-insensitive.</param>
        /// <param name="callback">The method to be called whenever the function is called in a dice expression.</param>
        /// <param name="scope">The scope of the function.</param>
        public void RegisterFunction(string name, FunctionCallback callback, FunctionScope scope)
        {
            RegisterFunction(new FunctionSlot(name, callback), scope);
        }

        /// <summary>
        /// Registers the specified callback to the given name, scope, and timing.
        /// </summary>
        /// <param name="name">Function name to register. Function names are case-insensitive.</param>
        /// <param name="callback">The method to be called whenever the function is called in a dice expression.</param>
        /// <param name="scope">The scope of the function.</param>
        /// <param name="timing">When in the order of evaluation of a roll the function should be executed. Ignored for global functions.</param>
        public void RegisterFunction(string name, FunctionCallback callback, FunctionScope scope, FunctionTiming timing)
        {
            RegisterFunction(new FunctionSlot(name, callback, timing), scope);
        }

        /// <summary>
        /// Registers the specified callback to the given name, scope, and timing.
        /// </summary>
        /// <param name="slot">Details on the function itself.</param>
        /// <param name="scope">The scope of the function.</param>
        /// <param name="extra">Details on the "extra" for the function. Ignored when registering a function on global scope.</param>
        /// <remarks>When overriding in a subclass, you will need to call the parent method for registration to fully succeed.</remarks>
        public virtual void RegisterFunction(FunctionSlot slot, FunctionScope scope, string? extra = null)
        {
            var lname = slot.Name.ToLowerInvariant();

            switch (scope)
            {
                case FunctionScope.All:
                    if (Contains(lname, FunctionScope.Global) || Contains(lname, FunctionScope.Basic) || Contains(lname, FunctionScope.Group))
                    {
                        throw new InvalidOperationException("A function with the same name and scope has already been registered");
                    }

                    Callbacks.Add((lname, FunctionScope.Global), slot);
                    Callbacks.Add((lname, FunctionScope.Basic), slot);
                    Callbacks.Add((lname, FunctionScope.Group), slot);
                    break;
                case FunctionScope.Roll:
                    if (Contains(lname, FunctionScope.Basic) || Contains(lname, FunctionScope.Group))
                    {
                        throw new InvalidOperationException("A function with the same name and scope has already been registered");
                    }

                    Callbacks.Add((lname, FunctionScope.Basic), slot);
                    Callbacks.Add((lname, FunctionScope.Group), slot);
                    break;
                default:
                    if (Contains(lname, scope))
                    {
                        throw new InvalidOperationException("A function with the same name and scope has already been registered");
                    }

                    Callbacks.Add((lname, scope), slot);
                    break;
            }

            if (extra != null && scope != FunctionScope.Global)
            {
                var lextra = extra.ToLowerInvariant();
                if (Extras.ContainsKey(lextra) && Extras[lextra].FunctionName != lname)
                {
                    throw new InvalidOperationException("An extra with the same name has already been registered");
                }

                Extras[lextra] = new FunctionExtra(lextra, lname);
            }
        }

        /// <summary>
        /// Removes the function with the given name and scope.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="scope"></param>
        /// <remarks>When overriding in a subclass, you will need to call the parent method for removal to fully succeed.</remarks>
        public void Remove(string name, FunctionScope scope)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (scope == FunctionScope.All)
            {
                Remove(name, FunctionScope.Global);
                Remove(name, FunctionScope.Basic);
                Remove(name, FunctionScope.Group);
                return;
            }
            else if (scope == FunctionScope.Roll)
            {
                Remove(name, FunctionScope.Basic);
                Remove(name, FunctionScope.Group);
                return;
            }

            var lname = name.ToLowerInvariant();

            Callbacks.Remove((lname, scope));

            // if the last roll callback for this name was removed, also remove any extra for it
            if (scope != FunctionScope.Group && !Contains(name, FunctionScope.Roll))
            {
                var toRemove = Extras.Where(kv => kv.Value.FunctionName == lname).Select(kv => kv.Key).ToList();
                foreach (var extra in toRemove)
                {
                    Extras.Remove(extra);
                }
            }
        }

        /// <summary>
        /// Controls whether or not this particular registry contains a callback with the given name and scope
        /// </summary>
        /// <param name="name"></param>
        /// <param name="scope">Scope to check; if All or Roll this function will return true if a callback is defined for any of their encompassing scopes</param>
        /// <returns></returns>
        protected internal bool Contains(string name, FunctionScope scope)
        {
            return scope switch
            {
                FunctionScope.All => Contains(name, FunctionScope.Global) || Contains(name, FunctionScope.Basic) || Contains(name, FunctionScope.Group),
                FunctionScope.Roll => Contains(name, FunctionScope.Basic) || Contains(name, FunctionScope.Group),
                _ => Callbacks.ContainsKey((name.ToLowerInvariant(), scope))
            };
        }

        /// <summary>
        /// Controls whether or not this particular registry contains an extra with the given name and scope.
        /// This always returns false when given a scope of All or Global
        /// </summary>
        /// <param name="name"></param>
        /// <param name="scope">Scope to check; if Roll this function will return true if an extra is defined for either Basic or Group</param>
        /// <returns></returns>
        protected internal bool ContainsExtra(string name, FunctionScope scope)
        {
            return scope switch
            {
                FunctionScope.All => false,
                FunctionScope.Global => false,
                FunctionScope.Roll => ContainsExtra(name, FunctionScope.Basic) || ContainsExtra(name, FunctionScope.Group),
                _ => Extras.ContainsKey(name.ToLowerInvariant()) && Contains(Extras[name.ToLowerInvariant()].FunctionName, scope)
            };
        }
    }
}
