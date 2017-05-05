using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Reflection;

using SConvert = System.Convert;

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

                // http://i.imgur.com/c4jt321.png
                object[] arr = (object[])tag.Data;
                Type eleType = type.GetElementType();
                var method = typeof(Enumerable)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .First(m => m.Name == "Select" && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(object), eleType);
                var method2 = typeof(Enumerable).GetMethod("ToArray", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(eleType);
                var cmethod = typeof(SConvert).GetMethod(
                    "ChangeType",
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    CallingConventions.Any,
                    new Type[] { typeof(object), typeof(Type) },
                    null);

                var ctype = typeof(Func<,>).MakeGenericType(typeof(object), eleType);
                var cparam = Expression.Parameter(typeof(object));
                var cbody = Expression.Convert(Expression.Call(cmethod, cparam, Expression.Constant(eleType)), eleType);
                var cfunc = Expression.Lambda(ctype, cbody, cparam).Compile();

                var convArr = method.Invoke(null, new object[] { arr, cfunc });
                return method2.Invoke(null, new object[] { convArr });
            }

            return SConvert.ChangeType(tag.Data, type);
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

            return SConvert.ToSByte(tag.Data) != 0;
        }

        public byte ToByte(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Byte)
            {
                throw new SerializationException("Cannot convert non-byte to byte");
            }

            return SConvert.ToByte(tag.Data);
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

            // was already converted back to host byte order in deserialization process
            return new decimal((int[])tag.Data);
        }

        public double ToDouble(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Double)
            {
                throw new SerializationException("Cannot convert non-double to double");
            }

            return SConvert.ToDouble(tag.Data);
        }

        public short ToInt16(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Short)
            {
                throw new SerializationException("Cannot convert non-short to short");
            }

            return SConvert.ToInt16(tag.Data);
        }

        public int ToInt32(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Int)
            {
                throw new SerializationException("Cannot convert non-int to int");
            }

            return SConvert.ToInt32(tag.Data);
        }

        public long ToInt64(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Long)
            {
                throw new SerializationException("Cannot convert non-long to long");
            }

            return SConvert.ToInt64(tag.Data);
        }

        public sbyte ToSByte(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Byte)
            {
                throw new SerializationException("Cannot convert non-byte to sbyte");
            }

            return SConvert.ToSByte(tag.Data);
        }

        public float ToSingle(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Float)
            {
                throw new SerializationException("Cannot convert non-float to float");
            }

            return SConvert.ToSingle(tag.Data);
        }

        public string ToString(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.String && tag.Data != null)
            {
                throw new SerializationException("Cannot convert non-string to string");
            }

            return SConvert.ToString(tag.Data);
        }

        public ushort ToUInt16(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Short)
            {
                throw new SerializationException("Cannot convert non-short to ushort");
            }

            return SConvert.ToUInt16(tag.Data);
        }

        public uint ToUInt32(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Int)
            {
                throw new SerializationException("Cannot convert non-int to uint");
            }

            return SConvert.ToUInt32(tag.Data);
        }

        public ulong ToUInt64(object value)
        {
            NbtTag tag = value as NbtTag;

            if (tag.TagType != NbtTagType.Long)
            {
                throw new SerializationException("Cannot convert non-long to ulong");
            }

            return SConvert.ToUInt64(tag.Data);
        }
    }
}
