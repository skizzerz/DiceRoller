using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Dice
{
    /// <summary>
    /// Represents an entry in the function registry.
    /// </summary>
    [SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "Immutable struct; access via property buys us nothing here")]
    public readonly struct FunctionSlot : IEquatable<FunctionSlot>
    {
        /// <summary>
        /// Function name, in normalized case.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Function timing.
        /// </summary>
        public readonly FunctionTiming Timing;

        /// <summary>
        /// How the function behaves when it is specified multiple times on the same roll.
        /// </summary>
        public readonly FunctionBehavior Behavior;

        /// <summary>
        /// Callback to execute when evaluating the function.
        /// </summary>
        public readonly FunctionCallback Callback;

        /// <summary>
        /// Number and types of arguments this function accepts, or null to not check.
        /// </summary>
        public readonly string? ArgumentPattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionSlot"/> struct.
        /// </summary>
        /// <param name="name">Function name, in normalized case.</param>
        /// <param name="callback">Function callback.</param>
        /// <param name="timing">Function timing, by default the function executes after all builtins have executed.</param>
        /// <param name="behavior">Function behavior, by default mutliples of the same function are executed sequentially.</param>
        /// <param name="argumentPattern">
        /// Types of arguments this function accepts, or null to not check function arity and argument types.
        /// This parameter accepts a subset of a regular expression syntax:
        /// <list type="bullet">
        /// <item>
        /// <term>C</term>
        /// <description>Comparison</description>
        /// </item>
        /// <item>
        /// <term>E</term>
        /// <description>Non-Comparison Expression</description>
        /// </item>
        /// <item>
        /// <term>.</term>
        /// <description>Either a Comparison or a non-Comparison Expression</description>
        /// </item>
        /// <item>
        /// <term>()</term>
        /// <description>Denotes a group of arguments that can be augmented by ?, *, or +</description>
        /// </item>
        /// <item>
        /// <term>?</term>
        /// <description>0 or 1 of the previous argument or group, or when applied to * or +, makes them non-greedy</description>
        /// </item>
        /// <item>
        /// <term>*</term>
        /// <description>0 or more of the previous argument or group</description>
        /// </item>
        /// <item>
        /// <term>+</term>
        /// <description>1 or more of the previous argument or group</description>
        /// </item>
        /// </list>
        /// </param>
        public FunctionSlot(
            string name,
            FunctionCallback callback,
            FunctionTiming timing = FunctionTiming.Last,
            FunctionBehavior behavior = FunctionBehavior.ExecuteSequentially,
            string? argumentPattern = null)
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

        /// <inheritdoc/>
        public bool Equals(FunctionSlot other)
        {
            return Name == other.Name
                && Callback == other.Callback
                && Timing == other.Timing
                && Behavior == other.Behavior
                && ArgumentPattern == other.ArgumentPattern;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is FunctionSlot slot && Equals(slot);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Name.GetHashCode()
                + Callback.GetHashCode()
                + Timing.GetHashCode()
                + Behavior.GetHashCode()
                + (ArgumentPattern?.GetHashCode() ?? 0);
        }

        /// <summary>
        /// Test if two FunctionSlots are equal.
        /// </summary>
        /// <param name="left">First FunctionSlot to check.</param>
        /// <param name="right">Second FunctionSlot to check.</param>
        /// <returns>true if both are equal, false otherwise.</returns>
        public static bool operator ==(FunctionSlot left, FunctionSlot right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Test if two FunctionSlots are not equal.
        /// </summary>
        /// <param name="left">First FunctionSlot to check.</param>
        /// <param name="right">Second FunctionSlot to check.</param>
        /// <returns>true if both are not equal, false otherwise.</returns>
        public static bool operator !=(FunctionSlot left, FunctionSlot right)
        {
            return !(left == right);
        }
    }
}
