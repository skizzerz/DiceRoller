using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace Dice.Serialization
{
    /// <summary>
    /// Specialized formatter to serve NBT data (uncompressed, as compression can be achieved
    /// via specialized streams and such). This formatter is not designed for general use.
    /// </summary>
    sealed internal class NbtFormatter : IFormatter
    {
        /// <summary>
        /// Required for IFormatter but unused.
        /// </summary>
        public ISurrogateSelector SurrogateSelector { get; set; }
        /// <summary>
        /// Required for IFormatter but unused.
        /// </summary>
        public SerializationBinder Binder { get; set; }
        /// <summary>
        /// Streaming context.
        /// </summary>
        public StreamingContext Context { get; set; }

        public NbtFormatter()
        {
            // by default we assume that we are persisting data if this is used
            Context = new StreamingContext(StreamingContextStates.Persistence);
        }

        public NbtFormatter(ISurrogateSelector selector, StreamingContext context)
        {
            SurrogateSelector = selector;
            Context = context;
        }

        public object Deserialize(Stream serializationStream)
        {
            if (serializationStream == null)
            {
                throw new ArgumentNullException(nameof(serializationStream));
            }

            if (serializationStream.CanSeek && serializationStream.Length == 0)
            {
                throw new SerializationException("Cannot deserialize empty stream");
            }

            return ParseTag(serializationStream);
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            Serialize(serializationStream, graph, "Root");
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "I can't be bothered right now")]
        private void Serialize(Stream serializationStream, object graph, string header)
        {
            if (serializationStream == null)
            {
                throw new ArgumentNullException(nameof(serializationStream));
            }

            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            Type t = graph.GetType();
            if (t == typeof(byte) || t == typeof(sbyte))
            {
                WriteTag(serializationStream, NbtTagType.Byte, header, graph);
                return;
            }
            else if (t == typeof(short) || t == typeof(ushort))
            {
                WriteTag(serializationStream, NbtTagType.Short, header, IPAddress.HostToNetworkOrder(Convert.ToInt16(graph)));
                return;
            }
            else if (t == typeof(int) || t == typeof(uint))
            {
                WriteTag(serializationStream, NbtTagType.Int, header, IPAddress.HostToNetworkOrder(Convert.ToInt32(graph)));
                return;
            }
            else if (t == typeof(long) || t == typeof(ulong))
            {
                WriteTag(serializationStream, NbtTagType.Long, header, IPAddress.HostToNetworkOrder(Convert.ToInt64(graph)));
                return;
            }
            else if (t == typeof(float))
            {
                byte[] bytes = BitConverter.GetBytes(Convert.ToSingle(graph));
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes);
                }

                WriteTag(serializationStream, NbtTagType.Float, header, bytes);
                return;
            }
            else if (t == typeof(double))
            {
                byte[] bytes = BitConverter.GetBytes(Convert.ToDouble(graph));
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes);
                }

                WriteTag(serializationStream, NbtTagType.Float, header, bytes);
                return;
            }
            else if (t == typeof(decimal))
            {
                int[] ints = Decimal.GetBits(Convert.ToDecimal(graph));
                if (BitConverter.IsLittleEndian)
                {
                    for (var i = 0; i < ints.Length; i++)
                    {
                        ints[i] = IPAddress.HostToNetworkOrder(ints[i]);
                    }
                }

                WriteTag(serializationStream, NbtTagType.IntArray, header, ints);
                return;
            }
            else if (t == typeof(string))
            {
                WriteTag(serializationStream, NbtTagType.String, header, Convert.ToString(graph));
            }
            else if (t.IsArray)
            {
                Array arr = (Array)graph;
                WriteTag(serializationStream, NbtTagType.List, header, null);

                if (arr.Length == 0)
                {
                    WriteTag(serializationStream, NbtTagType.End, null, null);
                }
                else
                {
                    serializationStream.WriteByte((byte)GetTagType(arr.GetValue(0).GetType()));
                }

                WriteTag(serializationStream, NbtTagType.Int, null, IPAddress.HostToNetworkOrder(arr.Length));

                for (int i = 0; i < arr.Length; i++)
                {
                    Serialize(serializationStream, arr.GetValue(i), null);
                }

                return;
            }
            else if (graph is ISerializable)
            {
                // no-op, handled below
            }
            else
            {
                throw new SerializationException("Cannot serialize this type");
            }

            SerializationInfo info = new SerializationInfo(graph.GetType(), new NbtFormatterConverter());
            ((ISerializable)graph).GetObjectData(info, Context);

            if (header != null)
            {
                WriteTag(serializationStream, NbtTagType.Compound, header, null);
            }

            foreach (var e in info)
            {
                NbtTagType tag;
                object data;

                if (e.Value == null)
                {
                    tag = NbtTagType.ByteArray;
                    data = new byte[0];
                }
                else if (e.ObjectType == typeof(byte) || e.ObjectType == typeof(sbyte))
                {
                    tag = NbtTagType.Byte;
                    data = e.Value;
                }
                else if (e.ObjectType == typeof(short) || e.ObjectType == typeof(ushort))
                {
                    tag = NbtTagType.Short;
                    data = IPAddress.HostToNetworkOrder(Convert.ToInt16(e.Value));
                }
                else if (e.ObjectType == typeof(int) || e.ObjectType == typeof(uint))
                {
                    tag = NbtTagType.Int;
                    data = IPAddress.HostToNetworkOrder(Convert.ToInt32(e.Value));
                }
                else if (e.ObjectType == typeof(long) || e.ObjectType == typeof(ulong))
                {
                    tag = NbtTagType.Long;
                    data = IPAddress.HostToNetworkOrder(Convert.ToInt64(e.Value));
                }
                else if (e.ObjectType == typeof(float))
                {
                    tag = NbtTagType.Float;
                    byte[] bytes = BitConverter.GetBytes(Convert.ToSingle(e.Value));
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(bytes);
                    }

                    data = bytes;
                }
                else if (e.ObjectType == typeof(double))
                {
                    tag = NbtTagType.Double;
                    byte[] bytes = BitConverter.GetBytes(Convert.ToDouble(e.Value));
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(bytes);
                    }

                    data = bytes;
                }
                else if (e.ObjectType == typeof(decimal))
                {
                    tag = NbtTagType.IntArray;
                    int[] ints = Decimal.GetBits(Convert.ToDecimal(e.Value));
                    if (BitConverter.IsLittleEndian)
                    {
                        for (var i = 0; i < ints.Length; i++)
                        {
                            ints[i] = IPAddress.HostToNetworkOrder(ints[i]);
                        }
                    }

                    data = ints;
                }
                else if (e.ObjectType == typeof(string))
                {
                    tag = NbtTagType.String;
                    data = Convert.ToString(e.Value);
                }
                else if (e.ObjectType.IsArray)
                {
                    tag = NbtTagType.List;
                    data = null;
                }
                else if (e.Value is ISerializable)
                {
                    tag = NbtTagType.Compound;
                    data = null;
                }
                else
                {
                    throw new SerializationException("Cannot serialize this type");
                }

                WriteTag(serializationStream, tag, e.Name, data);

                if (e.ObjectType.IsArray)
                {
                    Array arr = (Array)e.Value;
                    if (arr.Length == 0)
                    {
                        WriteTag(serializationStream, NbtTagType.End, null, null);
                    }
                    else
                    {
                        serializationStream.WriteByte((byte)GetTagType(arr.GetValue(0).GetType()));
                    }

                    WriteTag(serializationStream, NbtTagType.Int, null, IPAddress.HostToNetworkOrder(arr.Length));

                    for (int i = 0; i < arr.Length; i++)
                    {
                        Serialize(serializationStream, arr.GetValue(i), null);
                    }
                }
                else if (e.Value is ISerializable)
                {
                    Serialize(serializationStream, e.Value, null);
                }
            }

            WriteTag(serializationStream, NbtTagType.End, null, null);
        }

        private static void WriteTag(Stream s, NbtTagType tag, string name, object data)
        {
            if (name != null || tag == NbtTagType.End)
            {
                s.WriteByte((byte)tag);
            }

            if (name != null)
            {
                var nb = Encoding.UTF8.GetBytes(name);
                s.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)nb.Length)), 0, 2);
                s.Write(nb, 0, nb.Length);
            }

            if (data == null)
            {
                // writing start of tag, but payload is coming via subsequent calls to WriteTag
                // (really only used for compound tags)
                return;
            }

            switch (tag)
            {
                // End is handled above
                case NbtTagType.Byte:
                    s.WriteByte(Convert.ToByte(data));
                    break;
                case NbtTagType.Short:
                    s.Write(BitConverter.GetBytes(Convert.ToInt16(data)), 0, 2);
                    break;
                case NbtTagType.Int:
                    s.Write(BitConverter.GetBytes(Convert.ToInt32(data)), 0, 4);
                    break;
                case NbtTagType.Long:
                    s.Write(BitConverter.GetBytes(Convert.ToInt64(data)), 0, 8);
                    break;
                case NbtTagType.Float:
                    s.Write((byte[])data, 0, 4);
                    break;
                case NbtTagType.Double:
                    s.Write((byte[])data, 0, 8);
                    break;
                case NbtTagType.IntArray:
                    int[] ia = (int[])data;
                    s.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(ia.Length)), 0, 4);
                    for (int i = 0; i < ia.Length; i++)
                    {
                        s.Write(BitConverter.GetBytes(ia[i]), 0, 4);
                    }
                    break;
                case NbtTagType.String:
                    byte[] sbytes = Encoding.UTF8.GetBytes(Convert.ToString(data));
                    s.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)sbytes.Length)), 0, 2);
                    s.Write(sbytes, 0, sbytes.Length);
                    break;
                case NbtTagType.ByteArray:
                    byte[] ba = (byte[])data;
                    s.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(ba.Length)), 0, 4);
                    if (ba.Length > 0)
                    {
                        s.Write(ba, 0, ba.Length);
                    }
                    break;
                default:
                    throw new SerializationException("Unknown data/tag combination");
            }
        }

        private static NbtTagType GetTagType(Type t)
        {
            if (t == typeof(byte) || t == typeof(sbyte))
            {
                return NbtTagType.Byte;
            }
            else if (t == typeof(short) || t == typeof(ushort))
            {
                return NbtTagType.Short;
            }
            else if (t == typeof(int) || t == typeof(uint))
            {
                return NbtTagType.Int;
            }
            else if (t == typeof(long) || t == typeof(ulong))
            {
                return NbtTagType.Long;
            }
            else if (t == typeof(float))
            {
                return NbtTagType.Float;
            }
            else if (t == typeof(double))
            {
                return NbtTagType.Double;
            }
            else if (t == typeof(decimal))
            {
                return NbtTagType.IntArray;
            }
            else if (t == typeof(string))
            {
                return NbtTagType.String;
            }
            else if (t.IsArray)
            {
                return NbtTagType.List;
            }
            else if (t.GetInterfaces().Contains(typeof(ISerializable)))
            {
                return NbtTagType.Compound;
            }
            else
            {
                throw new SerializationException("Cannot serialize this type");
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "I can't be bothered right now")]
        private NbtTag ParseTag(Stream s, NbtTagType? tag = null)
        {
            byte[] buffer = new byte[8];
            int read, size;
            object data = null;
            string name = null;

            if (tag == null)
            {
                read = s.ReadByte();
                if (read == -1)
                {
                    throw new SerializationException("Unexpected end of stream");
                }

                tag = (NbtTagType)read;
                if (tag == NbtTagType.End)
                {
                    // end tags are never named
                    return NbtTag.End;
                }

                name = (string)ParseTag(s, NbtTagType.String).Data;
            }

            switch (tag.Value)
            {
                case NbtTagType.Byte:
                    read = s.ReadByte();
                    if (read == -1)
                    {
                        throw new SerializationException("Unexpected end of stream");
                    }

                    data = (sbyte)read;
                    break;
                case NbtTagType.Short:
                    read = s.Read(buffer, 0, 2);
                    if (read < 2)
                    {
                        throw new SerializationException("Unexpected end of stream");
                    }

                    data = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 0));
                    break;
                case NbtTagType.Int:
                    read = s.Read(buffer, 0, 4);
                    if (read < 4)
                    {
                        throw new SerializationException("Unexpected end of stream");
                    }

                    data = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 0));
                    break;
                case NbtTagType.Long:
                    read = s.Read(buffer, 0, 8);
                    if (read < 8)
                    {
                        throw new SerializationException("Unexpected end of stream");
                    }

                    data = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer, 0));
                    break;
                case NbtTagType.Float:
                    read = s.Read(buffer, 0, 4);
                    if (read < 4)
                    {
                        throw new SerializationException("Unexpected end of stream");
                    }

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(buffer, 0, 4);
                    }

                    data = BitConverter.ToSingle(buffer, 0);
                    break;
                case NbtTagType.Double:
                    read = s.Read(buffer, 0, 8);
                    if (read < 8)
                    {
                        throw new SerializationException("Unexpected end of stream");
                    }

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(buffer, 0, 8);
                    }

                    data = BitConverter.ToDouble(buffer, 0);
                    break;
                case NbtTagType.ByteArray:
                    size = (int)ParseTag(s, NbtTagType.Int).Data;
                    if (size < 0)
                    {
                        throw new SerializationException("Invalid array size");
                    }

                    byte[] babuf = new byte[size];
                    if (size > 0)
                    {
                        read = s.Read(babuf, 0, size);
                        if (read < size)
                        {
                            throw new SerializationException("Unexpected end of stream");
                        }

                        data = babuf.Cast<sbyte>().ToArray();
                    }
                    else
                    {
                        // 0-length byte array is special case for null
                        data = null;
                    }

                    break;
                case NbtTagType.String:
                    size = (short)ParseTag(s, NbtTagType.Short).Data;
                    if (size < 0)
                    {
                        throw new SerializationException("Invalid string size");
                    }
                    else if (size == 0)
                    {
                        data = String.Empty;
                    }
                    else
                    {
                        byte[] sbuf = new byte[size];
                        read = s.Read(sbuf, 0, size);
                        if (read < size)
                        {
                            throw new SerializationException("Unexpected end of stream");
                        }

                        data = Encoding.UTF8.GetString(sbuf);
                    }

                    break;
                case NbtTagType.List:
                    NbtTagType ltype = (NbtTagType)(sbyte)ParseTag(s, NbtTagType.Byte).Data;
                    size = (int)ParseTag(s, NbtTagType.Int).Data;
                    if (size < 0)
                    {
                        throw new SerializationException("Invalid string size");
                    }

                    object[] lbuf = new object[size];
                    for (int i = 0; i < size; i++)
                    {
                        lbuf[i] = ParseTag(s, ltype).Data;
                    }

                    data = lbuf;
                    break;
                case NbtTagType.Compound:
                    Dictionary<string, NbtTag> sd = new Dictionary<string, NbtTag>();
                    for (var sde = ParseTag(s); sde != NbtTag.End; sde = ParseTag(s))
                    {
                        if (sd.ContainsKey(sde.Name))
                        {
                            throw new SerializationException("Duplicate info entry");
                        }

                        sd[sde.Name] = sde;
                    }

                    if (!sd.ContainsKey("_Class"))
                    {
                        throw new SerializationException("Missing _Class element");
                    }

                    Type objType;
                    switch ((SerializedClass)(sbyte)sd["_Class"].Data)
                    {
                        case SerializedClass.DieResult:
                            objType = typeof(DieResult);
                            break;
                        case SerializedClass.RollResult:
                            objType = typeof(RollResult);
                            break;
                        case SerializedClass.RollPost:
                            objType = typeof(PbP.RollPost);
                            break;
                        default:
                            throw new SerializationException("Unknown class type");
                    }

                    SerializationInfo info = new SerializationInfo(objType, new NbtFormatterConverter());
                    ConstructorInfo ctor = objType.GetConstructor(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        CallingConventions.Any,
                        new Type[] { typeof(SerializationInfo), typeof(StreamingContext) },
                        null);

                    foreach (var kvp in sd)
                    {
                        info.AddValue(kvp.Key, kvp.Value);
                    }

                    data = ctor.Invoke(new object[] { info, Context });
                    if (data is IDeserializationCallback cb)
                    {
                        cb.OnDeserialization(Context);
                    }

                    break;
                case NbtTagType.IntArray:
                    size = (int)ParseTag(s, NbtTagType.Int).Data;
                    if (size < 0)
                    {
                        throw new SerializationException("Invalid array size");
                    }

                    byte[] iabuf = new byte[size * 4];
                    int[] ia = new int[size];
                    if (size > 0)
                    {
                        read = s.Read(iabuf, 0, size * 4);
                        if (read < size)
                        {
                            throw new SerializationException("Unexpected end of stream");
                        }
                    }

                    for (int i = 0; i < size; i++)
                    {
                        ia[i] = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(iabuf, i * 4));
                    }

                    data = ia;
                    break;
            }

            return new NbtTag(tag.Value, name, data);
        }
    }
}
