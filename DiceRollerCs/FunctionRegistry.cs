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
    /// to built-in dice expressions. Note that functions cannot be removed once registered.
    /// </summary>
    public static class FunctionRegistry
    {
        internal static Dictionary<(string name, FunctionScope scope), (FunctionTiming timing, FunctionCallback callback)> Callbacks;

        static FunctionRegistry()
        {
            Callbacks = new Dictionary<(string name, FunctionScope scope), (FunctionTiming timing, FunctionCallback callback)>();
        }

        /// <summary>
        /// Registers a type, which causes all public static methods of that type with the
        /// DiceFunctionAttribute to be registered as callbacks. To register instance methods as well
        /// as static methods, use the other RegisterType overload.
        /// </summary>
        /// <param name="t">Type to register methods from</param>
        public static void RegisterType(Type t)
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

                    switch (attr.Scope)
                    {
                        case FunctionScope.All:
                            Callbacks.Add((attr.Name, FunctionScope.Global), (attr.Timing, callback));
                            Callbacks.Add((attr.Name, FunctionScope.Basic), (attr.Timing, callback));
                            Callbacks.Add((attr.Name, FunctionScope.Group), (attr.Timing, callback));
                            break;
                        case FunctionScope.Roll:
                            Callbacks.Add((attr.Name, FunctionScope.Basic), (attr.Timing, callback));
                            Callbacks.Add((attr.Name, FunctionScope.Group), (attr.Timing, callback));
                            break;
                        default:
                            Callbacks.Add((attr.Name, attr.Scope), (attr.Timing, callback));
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
        public static void RegisterType<T>(T obj)
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

                    switch (attr.Scope)
                    {
                        case FunctionScope.All:
                            Callbacks.Add((attr.Name, FunctionScope.Global), (attr.Timing, callback));
                            Callbacks.Add((attr.Name, FunctionScope.Basic), (attr.Timing, callback));
                            Callbacks.Add((attr.Name, FunctionScope.Group), (attr.Timing, callback));
                            break;
                        case FunctionScope.Roll:
                            Callbacks.Add((attr.Name, FunctionScope.Basic), (attr.Timing, callback));
                            Callbacks.Add((attr.Name, FunctionScope.Group), (attr.Timing, callback));
                            break;
                        default:
                            Callbacks.Add((attr.Name, attr.Scope), (attr.Timing, callback));
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Registers the specified callback to the given name
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="callback">Callback to invoke for this function</param>
        /// <param name="scope">Scope in which function is valid</param>
        /// <param name="timing">Timing of function execution</param>
        public static void RegisterFunction(string name, FunctionCallback callback, FunctionScope scope = FunctionScope.Global, FunctionTiming timing = FunctionTiming.Last)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (name == String.Empty)
            {
                throw new ArgumentException("Function name cannot be empty", "name");
            }

            Callbacks.Add((name, scope), (timing, callback));
        }
    }
}
