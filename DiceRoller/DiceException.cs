using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;

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
        /// <summary>
        /// Error code for this exception.
        /// </summary>
        public DiceErrorCode ErrorCode { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiceException"/> class.
        /// </summary>
        /// <param name="error">Error code whose description string does not require parameters.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="DiceException"/> class.
        /// </summary>
        /// <param name="error">Error code whose description string requires a parameter.</param>
        /// <param name="param">Object to use for the {0} parameter in the description.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="DiceException"/> class.
        /// </summary>
        /// <param name="error">Error code whose description string does not require parameters.</param>
        /// <param name="innerException">Inner exception.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="DiceException"/> class.
        /// </summary>
        /// <param name="error">Error code whose description string requires a parameter.</param>
        /// <param name="param">Object to use for the {0} parameter in the description.</param>
        /// <param name="innerException">Inner exception.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="DiceException"/> class
        /// using serialized data.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Streaming context.</param>
        protected DiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = (DiceErrorCode)info.GetValue("ErrorCode", typeof(int));
        }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("ErrorCode", (int)ErrorCode);
        }
    }
}
