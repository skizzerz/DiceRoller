using System;
using System.Collections.Generic;
using System.Text;

namespace Dice
{
    /// <summary>
    /// Represents an entry in the function registry
    /// </summary>
    public struct FunctionSlot
    {
        public string Name;
        public FunctionTiming Timing;
        public FunctionBehavior Behavior;
        public FunctionCallback Callback;
    }
}
