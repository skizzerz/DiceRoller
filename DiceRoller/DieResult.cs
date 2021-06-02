using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Dice
{
    /// <summary>
    /// Contains the result of an individual die roll.
    /// </summary>
    [Serializable]
    public struct DieResult : ISerializable, IEquatable<DieResult>
    {
        /// <summary>
        /// What type of die was rolled
        /// </summary>
        public DieType DieType { get; set; }

        /// <summary>
        /// How many sides the die had
        /// </summary>
        public int NumSides { get; set; }

        /// <summary>
        /// What the result of the roll was
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Any special flags giving more information about the roll
        /// </summary>
        public DieFlags Flags { get; set; }

        /// <summary>
        /// Additional data about the roll. Used with SpecialDie.Text to display arbitrary text
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// What type of special die this is. Calling this is an error if DieType != DieType.SpecialDie
        /// </summary>
        public SpecialDie SpecialDie
        {
            get { return (SpecialDie)Value; }
            set { Value = (decimal)value; }
        }

        /// <summary>
        /// Returns 1 if the die is a success, -1 if the die is a failure, and 0 otherwise
        /// </summary>
        /// <returns></returns>
        public int SuccessCount => Flags.HasFlag(DieFlags.Success) ? 1 : (Flags.HasFlag(DieFlags.Failure) ? -1 : 0);

        public DieResult(SpecialDie specialDie)
            : this()
        {
            DieType = DieType.Special;
            SpecialDie = specialDie;
        }

        /// <summary>
        /// Creates a Text die with the given data
        /// </summary>
        /// <param name="text"></param>
        public DieResult(string text)
            : this(SpecialDie.Text)
        {
            Data = text;
        }

        /// <summary>
        /// Deserializes a DieResult
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
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
        /// Mark a die as a success
        /// </summary>
        /// <param name="die"></param>
        /// <returns></returns>
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
        /// Mark a die as a failure
        /// </summary>
        /// <param name="die"></param>
        /// <returns></returns>
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
        /// <param name="flags"></param>
        /// <returns></returns>
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
        /// Checks if this die is a special die of the specified type
        /// </summary>
        /// <param name="specialDie"></param>
        /// <returns></returns>
        public bool IsSpecialDie(SpecialDie specialDie)
        {
            return DieType == DieType.Special && SpecialDie == specialDie;
        }

        /// <summary>
        /// Checks if the die is not a special die and has not been dropped
        /// </summary>
        /// <returns></returns>
        public bool IsLiveDie()
        {
            return DieType != DieType.Special && !Flags.HasFlag(DieFlags.Dropped);
        }

        public override bool Equals(object obj)
        {
            if (obj is DieResult d)
            {
                return Equals(d);
            }

            return false;
        }

        public bool Equals(DieResult d)
        {
            return DieType == d.DieType && NumSides == d.NumSides && Value == d.Value && Flags == d.Flags;
        }

        public override int GetHashCode()
        {
            return new { DieType, NumSides, Value, Flags }.GetHashCode();
        }

        /// <summary>
        /// Serializes binary data to the given stream.
        /// This method should be called when serializing this object to the database, to ensure that it is deserialized in the correct state.
        /// </summary>
        /// <param name="serializationStream"></param>
        public void Serialize(Stream serializationStream)
        {
            var formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Persistence));
            formatter.Serialize(serializationStream, this);
        }

        /// <summary>
        /// Deserializes binary data from the given stream, that data must have been serialized via RollPost.Serialize().
        /// </summary>
        /// <param name="serializationStream"></param>
        /// <returns></returns>
        public static DieResult Deserialize(Stream serializationStream)
        {
            var formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Persistence));
            return (DieResult)formatter.Deserialize(serializationStream);
        }

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

        public static bool operator ==(DieResult a, DieResult b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(DieResult a, DieResult b)
        {
            return !a.Equals(b);
        }
    }
}
