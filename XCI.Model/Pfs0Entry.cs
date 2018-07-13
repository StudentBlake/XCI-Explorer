using System;

namespace XCI.Model
{
    internal static partial class Pfs0
    {
        public class Pfs0Entry
        {
            public byte[] Data;
            public string Name;
            public int NamePtr;
            public long Offset;
            public int Reserved;
            public long Size;

            public Pfs0Entry(byte[] data)
            {
                Data = data;
                Offset = BitConverter.ToInt64(data, 0);
                Size = BitConverter.ToInt64(data, 8);
                NamePtr = BitConverter.ToInt32(data, 16);
                Reserved = BitConverter.ToInt32(data, 20);
            }
        }
    }
}