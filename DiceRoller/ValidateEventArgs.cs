using System;
using System.Collections.Generic;
using System.Text;

namespace Dice
{
    /// <summary>
    /// Event fired when validating the collection of functions with a given timing.
    /// This is useful to ensure that certain combinations of functions are not used.
    /// The event listener should throw an exception if validation fails, and do nothing
    /// if validation succeeds.
    /// </summary>
    public class ValidateEventArgs : EventArgs
    {
        /// <summary>
        /// Function timing being validated.
        /// </summary>
        public FunctionTiming Timing { get; private set; }

        /// <summary>
        /// List of functions attached to the roll that have the given Timing.
        /// </summary>
        public IReadOnlyList<FunctionContext> Contexts { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateEventArgs"/> class.
        /// </summary>
        /// <param name="timing">Function timing to validate.</param>
        /// <param name="contexts">All functions attached to the roll that have the given timing.</param>
        internal ValidateEventArgs(FunctionTiming timing, IReadOnlyList<FunctionContext> contexts)
        {
            Timing = timing;
            Contexts = contexts;
        }
    }
}
