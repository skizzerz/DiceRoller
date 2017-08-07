using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics.CodeAnalysis;

using Dice.AST;

namespace Dice
{
    /// <summary>
    /// Holds the result of a roll, allowing easy access to the total result
    /// as well as the individual die results of the roll.
    /// </summary>
    [Serializable]
    public class RollResult : ISerializable, IEquatable<RollResult>
    {
        /// <summary>
        /// This RollResult is used as a placeholder whenever an invalid roll is added to a RollPost.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "RollResult is immutable")]
        public static readonly RollResult InvalidRoll = new RollResult()
        {
            Value = 0,
            Values = new DieResult[0],
            ResultType = ResultType.Total,
            RollRoot = null,
            Expression = "#INVALID",
            NumRolls = 0,
            Metadata = null,
            AllRolls = new uint[0],
            AllMacros = new decimal[0]
        };

        /// <summary>
        /// The result of the roll. This will either be the total or the number of successes.
        /// ResultType can be used to determine which it is.
        /// </summary>
        public decimal Value { get; private set; }

        /// <summary>
        /// The values of the individual dice rolled. This is not necessarily all dice rolled,
        /// just the ones that are exposed to the user. For example, in (1d8)d6, 1d8 is rolled,
        /// and that many d6s are rolled. Values will only contain the results of the d6s.
        /// Inspecting the d8 requires walking the AST beginning at RollRoot.
        /// </summary>
        public IReadOnlyList<DieResult> Values { get; private set; }

        /// <summary>
        /// Whether or not Value represents the roll total or the number of successes.
        /// </summary>
        public ResultType ResultType { get; private set; }

        /// <summary>
        /// The root of the AST for this roll. Accessing this is usually not required,
        /// but is exposed if deeper introspection into the roll is desired.
        /// This will be null if a RollResult is deserialized.
        /// </summary>
        public DiceAST RollRoot { get; private set; }

        /// <summary>
        /// The normalized dice expression for the roll, equivalent to RollRoot.ToString(),
        /// except that this will be defined even if RollRoot is null.
        /// </summary>
        public string Expression { get; private set; }

        /// <summary>
        /// The number of dice rolls that were needed to fully evaluate this expression.
        /// </summary>
        public int NumRolls { get; private set; }

        /// <summary>
        /// An optional object which contains metadata about the roll.
        /// </summary>
        public object Metadata { get; private set; }

        /// <summary>
        /// The rolled value for every die, from left-to-right according to the normalized Expression.
        /// By taking Expression, AllRolls, and AllMacros, it is possible to reconstruct the full
        /// DiceAST and arrive at exactly the same result. In the event access to RollRoot is needed
        /// after a RollResult is deserialized, this is how RollRoot is created.
        /// </summary>
        private IReadOnlyList<uint> AllRolls;

        /// <summary>
        /// The value for every macro, from left-to-right according to the normalized Expression.
        /// By taking Expression, AllRolls, and AllMacros, it is possible to reconstruct the full
        /// DiceAST and arrive at exactly the same result. In the event access to RollRoot is needed
        /// after a RollResult is deserialized, this is how RollRoot is created.
        /// </summary>
        private IReadOnlyList<decimal> AllMacros;

        private RollResult() { }

        internal RollResult(RollData data, DiceAST rollRoot, int numRolls)
        {
            RollRoot = rollRoot ?? throw new ArgumentNullException("rollRoot");
            // cache some commonly-referenced information directly in this class instead of requiring
            // the user to drill down into RollRoot for everything (and because RollRoot isn't available
            // if deserializing).
            ResultType = rollRoot.ValueType;
            Value = rollRoot.Value;
            Values = rollRoot.Values;
            NumRolls = numRolls;
            Expression = rollRoot.ToString();
            Metadata = data.Metadata;
            AllRolls = data.InternalContext.AllRolls;
            AllMacros = data.InternalContext.AllMacros;
        }

        /// <summary>
        /// Constructs a new instance of RollResult using the serialized data.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RollResult(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            RollRoot = null;
            int version = info.GetInt32("_Version");

            ResultType = (ResultType)info.GetInt32("ResultType");
            Value = info.GetDecimal("Value");
            Values = (DieResult[])info.GetValue("Values", typeof(DieResult[]));
            NumRolls = info.GetInt32("NumRolls");
            Expression = info.GetString("Expression");
            AllRolls = (uint[])info.GetValue("AllRolls", typeof(uint[]));
            AllMacros = (decimal[])info.GetValue("AllMacros", typeof(decimal[]));

            if (version >= 3)
            {
                Metadata = info.GetValue("Metadata", typeof(object));
            }
            else
            {
                Metadata = null;
            }
        }

        /// <summary>
        /// Display a representation of the roll
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Expression);

            sb.Append(" => ");
            foreach (var die in Values)
            {
                if (die.DieType == DieType.Special)
                {
                    switch ((SpecialDie)die.Value)
                    {
                        case SpecialDie.Add:
                            sb.Append(" + ");
                            break;
                        case SpecialDie.Subtract:
                            sb.Append(" - ");
                            break;
                        case SpecialDie.Multiply:
                            sb.Append(" * ");
                            break;
                        case SpecialDie.Divide:
                            sb.Append(" / ");
                            break;
                        case SpecialDie.OpenParen:
                            sb.Append("(");
                            break;
                        case SpecialDie.CloseParen:
                            sb.Append(")");
                            break;
                        case SpecialDie.Negate:
                            sb.Append("-");
                            break;
                        case SpecialDie.Comma:
                            sb.Append(", ");
                            break;
                        case SpecialDie.Text:
                            sb.Append(die.Data);
                            break;
                        default:
                            sb.Append("<<UNKOWN SPECIAL>>");
                            break;
                    }
                }
                else
                {
                    if (die.Flags.HasFlag(DieFlags.Success))
                    {
                        sb.Append("$");
                    }
                    else if (die.Flags.HasFlag(DieFlags.Failure))
                    {
                        sb.Append("#");
                    }

                    sb.Append(die.Value);

                    if (die.Flags.HasFlag(DieFlags.Critical) || die.Flags.HasFlag(DieFlags.Fumble))
                    {
                        sb.Append("!");
                    }

                    if (die.Flags.HasFlag(DieFlags.Dropped))
                    {
                        sb.Append("*");
                    }
                }
            }

            sb.Append(" => ");
            sb.Append(Value);

            if (ResultType == ResultType.Successes)
            {
                if (Value == 1)
                {
                    sb.Append(" success");
                }
                else
                {
                    sb.Append(" successes");
                }
            }

            return sb.ToString();
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
        public static RollResult Deserialize(Stream serializationStream)
        {
            var formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Persistence));
            return (RollResult)formatter.Deserialize(serializationStream);
        }

        /// <summary>
        /// Serializes the RollResult.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("_Version", 3);
            info.AddValue("ResultType", (int)ResultType);
            info.AddValue("Value", Value);
            info.AddValue("Values", Values.ToArray(), typeof(DieResult[]));
            info.AddValue("NumRolls", NumRolls);
            info.AddValue("Expression", Expression);
            info.AddValue("Metadata", Metadata);
            info.AddValue("AllRolls", AllRolls.ToArray(), typeof(uint[]));
            info.AddValue("AllMacros", AllMacros.ToArray(), typeof(decimal[]));
        }

        public override bool Equals(object obj)
        {
            if (obj is RollResult r)
            {
                return Equals(r);
            }

            return false;
        }

        public virtual bool Equals(RollResult r)
        {
            // RollRoot is not considered when determining if two RollResults are equal.
            // This is because RollRoot is not preserved on serialization, and does not contain
            // any information that is not represented via other properties/fields.
            // Metadata is not considered because it doesn't impact the result itself.
            return ResultType == r.ResultType
                && Value == r.Value
                && Values.SequenceEqual(r.Values)
                && NumRolls == r.NumRolls
                && AllRolls.SequenceEqual(r.AllRolls)
                && AllMacros.SequenceEqual(r.AllMacros);
        }

        public override int GetHashCode()
        {
            // See above for why RollRoot and Metadata isn't present in the hash code
            return new { ResultType, Value, Values = Values.ToArray(), NumRolls, Expression, AllRolls = AllRolls.ToArray(), AllMacros = AllMacros.ToArray() }.GetHashCode();
        }

        public static bool operator ==(RollResult a, RollResult b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(RollResult a, RollResult b)
        {
            return !a.Equals(b);
        }
    }
}
