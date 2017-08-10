using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice
{
    public class MacroRegistry
    {
        /// <summary>
        /// Callbacks which should be executed on every macro run.
        /// </summary>
        /// <remarks>
        /// Currently internal due to ExecuteMacro wiring up to this.
        /// When ExecuteMacro is removed, this will become private.
        /// </remarks>
        internal MacroCallback GlobalCallbacks { get; set; }

        private Dictionary<string, (string name, MacroCallback callback)> Callbacks = new Dictionary<string, (string, MacroCallback)>();

        /// <summary>
        /// Registers a type, which causes all public static methods of that type with the
        /// DiceMacroAttribute to be registered as callbacks. To register instance methods as well
        /// as static methods, use the other RegisterType overload.
        /// </summary>
        /// <param name="t">Type to register methods from</param>
        public void RegisterType(Type t)
        {
            foreach (var m in t.GetMethods().Where(m => m.IsPublic && m.IsStatic))
            {
                var attrs = m.GetCustomAttributes(typeof(DiceMacroAttribute), false).Cast<DiceMacroAttribute>();
                foreach (var attr in attrs)
                {
                    var paras = m.GetParameters();
                    if (m.ReturnType != typeof(void) || paras.Length != 1 || paras[0].ParameterType != typeof(MacroContext))
                    {
                        throw new InvalidOperationException("A DiceMacroAttribute can only be applied to a MacroCallback");
                    }

                    var callback = (MacroCallback)m.CreateDelegate(typeof(MacroCallback));
                    var lname = attr.Name.ToLower();
                    
                    if (Contains(lname))
                    {
                        throw new InvalidOperationException("A macro with the same name has already been registered");
                    }

                    Callbacks.Add(lname, (attr.Name, callback));
                }
            }
        }

        /// <summary>
        /// Registers a type given an object of that type, which causes all public static methods of that type
        /// as well as all public instance methods of that type with the DiceMacroAttribute to be registered
        /// as callbacks. The passed-in object will be used when calling instance methods.
        /// </summary>
        /// <typeparam name="T">Type whose methods will be registered.</typeparam>
        /// <param name="obj">Object to use when invoking instance methods.</param>
        public void RegisterType<T>(T obj)
        {
            foreach (var m in obj.GetType().GetMethods().Where(m => m.IsPublic))
            {
                var attrs = m.GetCustomAttributes(typeof(DiceMacroAttribute), false).Cast<DiceMacroAttribute>();
                foreach (var attr in attrs)
                {
                    var paras = m.GetParameters();
                    if (m.ReturnType != typeof(void) || paras.Length != 1 || paras[0].ParameterType != typeof(MacroContext))
                    {
                        throw new InvalidOperationException("A DiceMacroAttribute can only be applied to a MacroCallback");
                    }

                    MacroCallback callback;
                    if (m.IsStatic)
                    {
                        callback = (MacroCallback)m.CreateDelegate(typeof(MacroCallback));
                    }
                    else
                    {
                        callback = (MacroCallback)m.CreateDelegate(typeof(MacroCallback), obj);
                    }

                    var lname = attr.Name.ToLower();

                    if (Contains(lname))
                    {
                        throw new InvalidOperationException("A macro with the same name has already been registered");
                    }

                    Callbacks.Add(lname, (attr.Name, callback));
                }
            }
        }

        /// <summary>
        /// Registers the specified callback to the given name.
        /// </summary>
        /// <param name="name">Macro name to register. Macro names are case-insensitive.</param>
        /// <param name="callback">The method to be called whenever the macro is called in a dice expression.</param>
        public void RegisterMacro(string name, MacroCallback callback)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (name.Length == 0)
            {
                throw new ArgumentException("Macro name cannot be empty", "name");
            }

            var lname = name.ToLower();

            if (Contains(lname))
            {
                throw new InvalidOperationException("A macro with the same name has already been registered");
            }

            Callbacks.Add(lname, (name, callback));
        }

        /// <summary>
        /// Registers a callback to the global macro registry.
        /// The same callback may be registered more than once.
        /// </summary>
        /// <param name="callback"></param>
        public void RegisterGlobalMacro(MacroCallback callback)
        {
            GlobalCallbacks += callback ?? throw new ArgumentNullException("callback");
        }

        /// <summary>
        /// Removes the macro with the given name.
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            var lname = name.ToLower();

            Callbacks.Remove(lname);
        }

        /// <summary>
        /// Removes all instances of the callback from the global macro registry.
        /// </summary>
        /// <param name="callback"></param>
        public void RemoveGlobal(MacroCallback callback)
        {
            GlobalCallbacks -= callback ?? throw new ArgumentNullException("callback");
        }

        internal (string name, MacroCallback callback) Get(string lname)
        {
            return Callbacks[lname.ToLower()];
        }

        internal bool Contains(string name)
        {
            return Callbacks.ContainsKey(name.ToLower());
        }
    }
}
