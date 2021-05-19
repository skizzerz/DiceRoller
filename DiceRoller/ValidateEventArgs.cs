using System;
using System.Collections.Generic;
using System.Text;

namespace Dice
{
    public class ValidateEventArgs : EventArgs
    {
        public FunctionTiming Timing { get; private set; }

        public IReadOnlyList<FunctionContext> Contexts { get; private set; }

        internal ValidateEventArgs(FunctionTiming timing, IReadOnlyList<FunctionContext> contexts)
        {
            Timing = timing;
            Contexts = contexts;
        }
    }
}
