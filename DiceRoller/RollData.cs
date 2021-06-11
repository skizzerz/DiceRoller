using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice
{
    /// <summary>
    /// Holds data relevant to the scope of a single roll (as opposed to the global scope of RollerConfig).
    /// </summary>
    public class RollData
    {
        /// <summary>
        /// Functions specific to this roll. If these have the same name as a global function, this is executed
        /// instead. Cannot be null.
        /// </summary>
        public FunctionRegistry FunctionRegistry { get; set; } = new FunctionRegistry();

        /// <summary>
        /// Macros specific to this roll. If these have the same name as a global macro, this is executed
        /// instead. Cannot be null.
        /// </summary>
        public MacroRegistry MacroRegistry { get; set; } = new MacroRegistry();

        /// <summary>
        /// An optional metadata object that is passed as-is to the RollResult and is serialized alongside it.
        /// The object should be able to roundtrip when serialized/deserialized. Implement ISerializable if needed.
        /// Default null.
        /// </summary>
        public object? Metadata { get; set; }

        /// <summary>
        /// The config that was rolled along this roll.
        /// </summary>
        internal RollerConfig Config { get; set; } = Roller.DefaultConfig;

        /// <summary>
        /// Opaque contextual information used when evaluating dice expressions.
        /// </summary>
        internal InternalContext InternalContext { get; set; } = new InternalContext();
    }
}
