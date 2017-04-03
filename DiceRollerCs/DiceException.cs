using System;
using System.Runtime.Serialization;

namespace Dice
{
    /// <summary>
    /// Base class for all dice exceptions.
    /// Individual exceptions that subclass this are found in the Dice.Exceptions namespace.
    /// </summary>
    public abstract class DiceException : Exception
    {
        public DiceException() : base() { }
        public DiceException(string message) : base(message) { }
        public DiceException(string message, Exception innerException) : base(message, innerException) { }
        protected DiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
