using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Net;

namespace Dice.Serialization
{
    internal class NbtFormatterConverter : IFormatterConverter
    {
        public object Convert(object value, Type type)
        {
            NbtTag tag = value as NbtTag;

            if (type.IsArray)
            {
                if (tag.TagType != NbtTagType.List)
                {
                    throw new SerializationException("Cannot convert non-list to array");
                }
            }

            return System.Convert.ChangeType(tag.Data, type);
        }

        public object Convert(object value, TypeCode typeCode)
        {
            throw new NotImplementedException();
        }

        public bool ToBoolean(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Byte)
            {
                throw new SerializationException("Cannot convert non-byte to boolean");
            }

            return (sbyte)tag.Data != 0;
        }

        public byte ToByte(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Byte)
            {
                throw new SerializationException("Cannot convert non-byte to byte");
            }

            return unchecked((byte)tag.Data);
        }

        public char ToChar(object value)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(object value)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.IntArray)
            {
                throw new SerializationException("Cannot convert non-int-array to decimal");
            }

            int[] arr = (int[])tag.Data;
            if (BitConverter.IsLittleEndian)
            {
                // not sure if we need to also reverse the first 3 elements of arr as well...
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = IPAddress.NetworkToHostOrder(arr[i]);
                }
            }

            return new decimal(arr);
        }

        public double ToDouble(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Double)
            {
                throw new SerializationException("Cannot convert non-double to double");
            }

            return (double)tag.Data;
        }

        public short ToInt16(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Short)
            {
                throw new SerializationException("Cannot convert non-short to short");
            }

            return (short)tag.Data;
        }

        public int ToInt32(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Int)
            {
                throw new SerializationException("Cannot convert non-int to int");
            }

            return (int)tag.Data;
        }

        public long ToInt64(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Long)
            {
                throw new SerializationException("Cannot convert non-long to long");
            }

            return (long)tag.Data;
        }

        public sbyte ToSByte(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Byte)
            {
                throw new SerializationException("Cannot convert non-byte to sbyte");
            }

            return (sbyte)tag.Data;
        }

        public float ToSingle(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Float)
            {
                throw new SerializationException("Cannot convert non-float to float");
            }

            return (float)tag.Data;
        }

        public string ToString(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.String)
            {
                throw new SerializationException("Cannot convert non-string to string");
            }

            return (string)tag.Data;
        }

        public ushort ToUInt16(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Short)
            {
                throw new SerializationException("Cannot convert non-short to ushort");
            }

            return unchecked((ushort)tag.Data);
        }

        public uint ToUInt32(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Int)
            {
                throw new SerializationException("Cannot convert non-int to uint");
            }

            return unchecked((uint)tag.Data);
        }

        public ulong ToUInt64(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Long)
            {
                throw new SerializationException("Cannot convert non-long to ulong");
            }

            return unchecked((ulong)tag.Data);
        }
    }
}
