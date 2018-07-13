using System;
using System.Linq;

namespace XCI.Model
{
    internal static partial class Hfs0
    {
        public class Hsf0Entry
        {
            public byte[] Data;
            public byte[] Hash;
            public int HashedRegionSize;
            public string Name;
            public int NamePtr;
            public long Offset;
            public long Padding;
            public long Size;

            public Hsf0Entry(byte[] data)
            {
                Data = data;
                Offset = BitConverter.ToInt64(data, 0);
                Size = BitConverter.ToInt64(data, 8);
                NamePtr = BitConverter.ToInt32(data, 16);
                HashedRegionSize = BitConverter.ToInt32(data, 20);
                Padding = BitConverter.ToInt64(data, 24);
                Hash = Data.Skip(32).Take(32).ToArray();
            }
        }
    }
}