﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Dice
{
    /// <summary>
    /// Stores a mapping of all macros to their appropriate callbacks.
    /// </summary>
    public class MacroRegistry
    {
        /// <summary>
        /// Callbacks which should be executed on every macro run.
        /// </summary>
        internal MacroCallback? GlobalCallbacks { get; set; }

        private readonly Dictionary<string, (string Name, MacroCallback Callback)> Callbacks = new Dictionary<string, (string, MacroCallback)>();

        /// <summary>
        /// Registers a type, which causes all public static methods of that type with the
        /// DiceMacroAttribute to be registered as callbacks. To register instance methods as well
        /// as static methods, use the other RegisterType overload.
        /// </summary>
        /// <param name="t">Type to register methods from.</param>
        public void RegisterType(Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t));
            }

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
                    var lname = attr.Name.ToLowerInvariant();

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
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

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

                    var lname = attr.Name.ToLowerInvariant();

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
                throw new ArgumentNullException(nameof(name));
            }

            if (name.Length == 0)
            {
                throw new ArgumentException("Macro name cannot be empty", nameof(name));
            }

            var lname = name.ToLowerInvariant();

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
        /// <param name="callback">Callback to register.</param>
        public void RegisterGlobalMacro(MacroCallback callback)
        {
            GlobalCallbacks += callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <summary>
        /// Removes the macro with the given name.
        /// </summary>
        /// <param name="name">Name to remove.</param>
        public void Remove(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var lname = name.ToLowerInvariant();

            Callbacks.Remove(lname);
        }

        /// <summary>
        /// Removes all instances of the callback from the global macro registry.
        /// </summary>
        /// <param name="callback">Callback to remove.</param>
        public void RemoveGlobal(MacroCallback callback)
        {
            GlobalCallbacks -= callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <summary>
        /// Retrieves the macro with the given name.
        /// </summary>
        /// <param name="lname">Name to retrieve.</param>
        /// <returns>Returns a tuple of the normalized name and the callback.</returns>
        internal (string Name, MacroCallback Callback) Get(string lname)
        {
            return Callbacks[lname.ToLowerInvariant()];
        }

        /// <summary>
        /// Check if the macro registry contains a callback with the given name.
        /// </summary>
        /// <param name="name">Name to check.</param>
        /// <returns>true if a callback exists with the name, false otherwise.</returns>
        internal bool Contains(string name)
        {
            return Callbacks.ContainsKey(name.ToLowerInvariant());
        }
    }
}
