using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Dice
{
    /// <summary>
    /// Represents an entry in the function registry
    /// </summary>
    public readonly struct FunctionSlot
    {
        public readonly string Name;
        public readonly FunctionTiming Timing;
        public readonly FunctionBehavior Behavior;
        public readonly FunctionCallback Callback;
        public readonly string? ArgumentPattern;

        public FunctionSlot(
            string name,
            FunctionCallback callback,
            FunctionTiming timing = FunctionTiming.Last,
            FunctionBehavior behavior = FunctionBehavior.ExecuteSequentially,
            string? argumentPattern = null
            )
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.Length == 0)
            {
                throw new ArgumentException("Function name cannot be empty", nameof(name));
            }

            if (argumentPattern != null)
            {
                if (!Regex.IsMatch(argumentPattern, "^[CE.()?*+]*$"))
                {
                    throw new ArgumentException("Argument pattern contains unexpected characters; only CE.()?*+ are allowed", nameof(argumentPattern));
                }

                try
                {
                    _ = new Regex($"^{argumentPattern}$");
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException("Argument pattern is not valid", nameof(argumentPattern), e);
                }
            }

            Name = name;
            Timing = timing;
            Behavior = behavior;
            Callback = callback ?? throw new ArgumentNullException(nameof(callback));
            ArgumentPattern = argumentPattern;
        }
    }
}
