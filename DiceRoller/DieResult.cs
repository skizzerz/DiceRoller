using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Dice
{
    /// <summary>
    /// Contains the result of an individual die roll.
    /// </summary>
    [Serializable]
    public struct DieResult : ISerializable, IEquatable<DieResult>
    {
        /// <summary>
        /// What type of die was rolled.
        /// </summary>
        public DieType DieType { get; set; }

        /// <summary>
        /// How many sides the die had.
        /// </summary>
        public int NumSides { get; set; }

        /// <summary>
        /// What the result of the roll was.
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Any special flags giving more information about the roll.
        /// </summary>
        public DieFlags Flags { get; set; }

        /// <summary>
        /// Additional data about the roll. Used with SpecialDie.Text to display arbitrary text.
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// What type of special die this is. Calling this is an error if DieType != DieType.SpecialDie.
        /// </summary>
        public SpecialDie SpecialDie
        {
            get => DieType == DieType.Special ? (SpecialDie)Value : throw new InvalidOperationException("Die is not a special die");
            set => Value = (decimal)value;
        }

        /// <summary>
        /// Returns the number of successes this die counts for.
        /// </summary>
        /// <returns>How many successes this die counts for. 0 is returned if this die is not a success/failure die.</returns>
        public int SuccessCount
        {
            get
            {
                if (Flags.HasFlag(DieFlags.Success | DieFlags.Critical))
                {
                    return 2;
                }

                if (Flags.HasFlag(DieFlags.Success))
                {
                    return 1;
                }

                if (Flags.HasFlag(DieFlags.Failure | DieFlags.Fumble))
                {
                    return -2;
                }

                if (Flags.HasFlag(DieFlags.Failure))
                {
                    return -1;
                }

                return 0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DieResult"/> struct
        /// as a Special die of the given type. Do not use this constructor for Text dice,
        /// use the constructor overload which takes a string instead.
        /// </summary>
        /// <param name="specialDie">Special die type.</param>
        public DieResult(SpecialDie specialDie)
            : this()
        {
            DieType = DieType.Special;
            SpecialDie = specialDie;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DieResult"/> struct
        /// as a Text die with the given data.
        /// </summary>
        /// <param name="text">Text to use for the die.</param>
        public DieResult(string text)
            : this(SpecialDie.Text)
        {
            Data = text;
        }

        private DieResult(SerializationInfo info, StreamingContext context)
            : this()
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            DieType = (DieType)info.GetInt32("DieType");
            NumSides = info.GetInt32("NumSides");
            Value = info.GetDecimal("Value");
            Flags = (DieFlags)info.GetInt32("Flags");
            Data = info.GetString("Data");
        }

        /// <summary>
        /// Mark a die as a success. This will strip the Failure, Critical, and Fumble flags from the die.
        /// </summary>
        /// <returns>A new DieResult with the Success flag.</returns>
        public DieResult Success()
        {
            if (DieType == DieType.Special)
            {
                return this;
            }

            return new DieResult()
            {
                DieType = DieType,
                NumSides = NumSides,
                Value = Value,
                Data = Data,
                Flags = (Flags & ~DieFlags.Failure & ~DieFlags.Critical & ~DieFlags.Fumble) | DieFlags.Success
            };
        }

        /// <summary>
        /// Mark a die as a failure. This will strip the Success, Critical, and Fumble flags from the die.
        /// </summary>
        /// <returns>A new DieResult with the Failure flag.</returns>
        public DieResult Failure()
        {
            if (DieType == DieType.Special)
            {
                return this;
            }

            return new DieResult()
            {
                DieType = DieType,
                NumSides = NumSides,
                Value = Value,
                Data = Data,
                Flags = (Flags & ~DieFlags.Success & ~DieFlags.Critical & ~DieFlags.Fumble) | DieFlags.Failure
            };
        }

        /// <summary>
        /// Mark a die as dropped.
        /// </summary>
        /// <returns>A new DieResult with the Dropped flag.</returns>
        public DieResult Drop()
        {
            if (DieType == DieType.Special)
            {
                return this;
            }

            return new DieResult()
            {
                DieType = DieType,
                NumSides = NumSides,
                Value = Value,
                Data = Data,
                Flags = Flags | DieFlags.Dropped
            };
        }

        /// <summary>
        /// Checks if this die is a special die of the specified type.
        /// </summary>
        /// <param name="specialDie">Special die type to check.</param>
        /// <returns>true if the die is a special die and is of the passed-in type, false otherwise.</returns>
        public bool IsSpecialDie(SpecialDie specialDie)
        {
            return DieType == DieType.Special && SpecialDie == specialDie;
        }

        /// <summary>
        /// Checks if the die is not a special die and has not been dropped.
        /// </summary>
        /// <returns>true if the die is not a special die and has not been dropped, false otherwise.</returns>
        public bool IsLiveDie()
        {
            return DieType != DieType.Special && !Flags.HasFlag(DieFlags.Dropped);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is DieResult d)
            {
                return Equals(d);
            }

            return false;
        }

        /// <inheritdoc/>
        public bool Equals(DieResult other)
        {
            return DieType == other.DieType && NumSides == other.NumSides && Value == other.Value && Flags == other.Flags;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return new { DieType, NumSides, Value, Flags }.GetHashCode();
        }

        /// <summary>
        /// Serializes binary data to the given stream.
        /// This method should be called when serializing this object to the database, to ensure that it is deserialized in the correct state.
        /// <para>BINARY SERIALIZATION IS NOT SAFE. You should prefer to serialize to JSON or XML rather than call this method.</para>
        /// </summary>
        /// <param name="serializationStream">Stream to write serialized data to.</param>
        public void Serialize(Stream serializationStream)
        {
            var formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Persistence));
            formatter.Serialize(serializationStream, this);
        }

        /// <summary>
        /// Deserializes binary data from the given stream, that data must have been serialized via RollPost.Serialize().
        /// <para>THIS METHOD IS NOT SAFE TO CALL ON UNTRUSTED INPUT.</para>
        /// </summary>
        /// <param name="serializationStream">Serialization stream.</param>
        /// <returns>Deserialized DieResult.</returns>
        public static DieResult Deserialize(Stream serializationStream)
        {
            var formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Persistence));
            return (DieResult)formatter.Deserialize(serializationStream);
        }

        /// <inheritdoc/>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("_Version", 2);
            info.AddValue("DieType", (int)DieType);
            info.AddValue("NumSides", NumSides);
            info.AddValue("Value", Value);
            info.AddValue("Flags", (int)Flags);
            info.AddValue("Data", Data);
        }

        /// <summary>
        /// Test if two DieResults are equal.
        /// </summary>
        /// <param name="a">First DieResult to check.</param>
        /// <param name="b">Second DieResult to check.</param>
        /// <returns>true if they are equal, false otherwise.</returns>
        public static bool operator ==(DieResult a, DieResult b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Test if two DieResults are not equal.
        /// </summary>
        /// <param name="a">First DieResult to check.</param>
        /// <param name="b">Second DieResult to check.</param>
        /// <returns>true if they are not equal, false otherwise.</returns>
        public static bool operator !=(DieResult a, DieResult b)
        {
            return !a.Equals(b);
        }
    }
}
