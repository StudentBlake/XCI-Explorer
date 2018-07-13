using System;
using System.Linq;
using System.Text;

namespace XCI.Model
{
    internal static partial class Hfs0
    {
        public class Hfs0Header
        {
            public byte[] Data;
            public int FileCount;
            public string Magic;
            public int Reserved;
            public int StringTableSize;

            public Hfs0Header(byte[] data)
            {
                Data = data;
                Magic = Encoding.UTF8.GetString(Data.Take(4).ToArray());
                FileCount = BitConverter.ToInt32(data, 4);
                StringTableSize = BitConverter.ToInt32(data, 8);
                Reserved = BitConverter.ToInt32(data, 12);
            }
        }
    }
}