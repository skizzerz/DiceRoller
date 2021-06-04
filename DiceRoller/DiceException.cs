using System;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Globalization;

namespace Dice
{
    /// <summary>
    /// Represents an error with the user-inputted dice expression. These errors should generally
    /// be made visible to the user in some fashion to let them know to adjust their dice expression.
    /// Errors which indicate bugs in the library or are programmer errors do not use this exception type.
    /// </summary>
    [Serializable]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors",
        Justification = "ErrorCode is required, and the exception message is derived from ErrorCode")]
    public class DiceException : Exception
    {
        public DiceErrorCode ErrorCode { get; protected set; }

        public DiceException(DiceErrorCode error)
            : base(error.GetDescriptionString())
        {
            ErrorCode = error;

#if DEBUG
            if (error.GetDescriptionString().Contains("{0}"))
            {
                throw new InvalidOperationException("This DiceException constructor cannot be used with a message that requires a parameter", this);
            }
#endif
        }

        public DiceException(DiceErrorCode error, object param)
            : base(String.Format(CultureInfo.CurrentCulture, error.GetDescriptionString(), param))
        {
            ErrorCode = error;

#if DEBUG
            if (!error.GetDescriptionString().Contains("{0}"))
            {
                throw new InvalidOperationException("This DiceException constructor cannot be used with a message that requires a parameter", this);
            }
#endif
        }

        public DiceException(DiceErrorCode error, Exception innerException)
            : base(error.GetDescriptionString(), innerException)
        {
            ErrorCode = error;

#if DEBUG
            if (error.GetDescriptionString().Contains("{0}"))
            {
                throw new InvalidOperationException("This DiceException constructor cannot be used with a message that requires a parameter", this);
            }
#endif
        }

        public DiceException(DiceErrorCode error, object param, Exception innerException)
            : base(String.Format(CultureInfo.CurrentCulture, error.GetDescriptionString(), param), innerException)
        {
            ErrorCode = error;

#if DEBUG
            if (!error.GetDescriptionString().Contains("{0}"))
            {
                throw new InvalidOperationException("This DiceException constructor cannot be used with a message that requires a parameter", this);
            }
#endif
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
