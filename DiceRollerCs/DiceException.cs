using System;
using System.Runtime.Serialization;

namespace Dice
{
    /// <summary>
    /// Represents an error with the user-inputted dice expression. These errors should generally
    /// be made visible to the user in some fashion to let them know to adjust their dice expression.
    /// Errors which indicate bugs in the library or are programmer errors do not use this exception type.
    /// </summary>
    public class DiceException : Exception
    {
        public DiceErrorCode ErrorCode { get; protected set; }

        public DiceException(DiceErrorCode error)
            : base(error.GetDescriptionString())
        {
            ErrorCode = error;
        }

        public DiceException(DiceErrorCode error, object param)
            : base(String.Format(error.GetDescriptionString(), param))
        {
            ErrorCode = error;
        }

        public DiceException(DiceErrorCode error, Exception innerException)
            : base(error.GetDescriptionString(), innerException)
        {
            ErrorCode = error;
        }

        public DiceException(DiceErrorCode error, object param, Exception innerException)
            : base(String.Format(error.GetDescriptionString(), param), innerException)
        {
            ErrorCode = error;
        }

        protected DiceException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
