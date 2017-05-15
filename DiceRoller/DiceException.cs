using System;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace Dice
{
    /// <summary>
    /// Represents an error with the user-inputted dice expression. These errors should generally
    /// be made visible to the user in some fashion to let them know to adjust their dice expression.
    /// Errors which indicate bugs in the library or are programmer errors do not use this exception type.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors",
        Justification = "ErrorCode is required, and the exception message is derived from ErrorCode")]
    [Serializable]
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
            : base(info, context)
        {
            ErrorCode = (DiceErrorCode)info.GetValue("ErrorCode", typeof(int));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("ErrorCode", (int)ErrorCode);
        }
    }
}
