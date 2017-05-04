using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice.Serialization
{
    internal class NbtTag
    {
        public NbtTagType TagType;
        public string Name;
        public object Data;

        public NbtTag(NbtTagType tagType, string name, object data)
        {
            TagType = tagType;
            Name = name;
            Data = data;
        }

        public static NbtTag End = new NbtTag(NbtTagType.End, null, null);
    }
}
