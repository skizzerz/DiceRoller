using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Dice
{
    /// <summary>
    /// Represents an entry in the function registry
    /// </summary>
    [SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "Immutable struct; access via property buys us nothing here")]
    public readonly struct FunctionSlot : IEquatable<FunctionSlot>
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

        public bool Equals(FunctionSlot other)
        {
            return Name == other.Name
                && Callback == other.Callback
                && Timing == other.Timing
                && Behavior == other.Behavior
                && ArgumentPattern == other.ArgumentPattern;
        }

        public override bool Equals(object obj)
        {
            return obj is FunctionSlot slot && Equals(slot);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode()
                + Callback.GetHashCode()
                + Timing.GetHashCode()
                + Behavior.GetHashCode()
                + (ArgumentPattern?.GetHashCode() ?? 0);
        }

        public static bool operator ==(FunctionSlot left, FunctionSlot right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FunctionSlot left, FunctionSlot right)
        {
            return !(left == right);
        }
    }
}
