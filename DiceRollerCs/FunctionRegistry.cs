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
        private Dictionary<(string lname, FunctionScope scope), (string name, FunctionTiming timing, FunctionCallback callback)> Callbacks
            = new Dictionary<(string lname, FunctionScope scope), (string name, FunctionTiming timing, FunctionCallback callback)>();

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
                    var lname = attr.Name.ToLower();

                    switch (attr.Scope)
                    {
                        case FunctionScope.All:
                            Callbacks.Add((lname, FunctionScope.Global), (attr.Name, attr.Timing, callback));
                            Callbacks.Add((lname, FunctionScope.Basic), (attr.Name, attr.Timing, callback));
                            Callbacks.Add((lname, FunctionScope.Group), (attr.Name, attr.Timing, callback));
                            break;
                        case FunctionScope.Roll:
                            Callbacks.Add((lname, FunctionScope.Basic), (attr.Name, attr.Timing, callback));
                            Callbacks.Add((lname, FunctionScope.Group), (attr.Name, attr.Timing, callback));
                            break;
                        default:
                            Callbacks.Add((lname, attr.Scope), (attr.Name, attr.Timing, callback));
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Registers a type given an object of that type, which causes all public static methods of that type
        /// as well as all public instance methods of that type with the DiceFunctionAttribute to be registered
        /// as callbacks. The passed-in object will be used when calling instance methods.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public void RegisterType<T>(T obj)
        {
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

                    var lname = attr.Name.ToLower();

                    switch (attr.Scope)
                    {
                        case FunctionScope.All:
                            Callbacks.Add((lname, FunctionScope.Global), (attr.Name, attr.Timing, callback));
                            Callbacks.Add((lname, FunctionScope.Basic), (attr.Name, attr.Timing, callback));
                            Callbacks.Add((lname, FunctionScope.Group), (attr.Name, attr.Timing, callback));
                            break;
                        case FunctionScope.Roll:
                            Callbacks.Add((lname, FunctionScope.Basic), (attr.Name, attr.Timing, callback));
                            Callbacks.Add((lname, FunctionScope.Group), (attr.Name, attr.Timing, callback));
                            break;
                        default:
                            Callbacks.Add((lname, attr.Scope), (attr.Name, attr.Timing, callback));
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Registers the specified global callback to the given name
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="callback">Callback to invoke for this function</param>
        public void RegisterFunction(string name, FunctionCallback callback)
        {
            RegisterFunction(name, callback, FunctionScope.Global, FunctionTiming.Last);
        }

        /// <summary>
        /// Registers the specified callback to the given name
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="callback">Callback to invoke for this function</param>
        public void RegisterFunction(string name, FunctionCallback callback, FunctionScope scope)
        {
            RegisterFunction(name, callback, scope, FunctionTiming.Last);
        }

        /// <summary>
        /// Registers the specified callback to the given name
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="callback">Callback to invoke for this function</param>
        /// <param name="scope">Scope in which function is valid</param>
        /// <param name="timing">Timing of function execution</param>
        public void RegisterFunction(string name, FunctionCallback callback, FunctionScope scope, FunctionTiming timing)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (name.Length == 0)
            {
                throw new ArgumentException("Function name cannot be empty", "name");
            }

            var lname = name.ToLower();
            Callbacks.Add((lname, scope), (name, timing, callback));
        }

        /// <summary>
        /// Removes the function with the given name and scope.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="scope"></param>
        public void Remove(string name, FunctionScope scope)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            var lname = name.ToLower();

            if (BuiltinFunctions.ReservedNames.ContainsKey(lname))
            {
                throw new ArgumentException("Cannot remove a reserved function name", "name");
            }

            Callbacks.Remove((lname, scope));
        }

        internal (string name, FunctionTiming timing, FunctionCallback callback) Get(string lname, FunctionScope scope)
        {
            return Callbacks[(lname.ToLower(), scope)];
        }

        internal bool Contains(string lname, FunctionScope scope, bool includeReserved = true)
        {
            if (includeReserved && BuiltinFunctions.ReservedNames.ContainsKey(lname.ToLower()))
            {
                return true;
            }

            return Callbacks.ContainsKey((lname.ToLower(), scope));
        }
    }
}
