using System;
using System.Linq;
using System.Text;

namespace XCI.Model
{
    public static partial class Pfs0
    {
        public class Pfs0Header
        {
            public byte[] Data;
            public int FileCount;
            public string Magic;
            public int Reserved;
            public int StringTableSize;

            public Pfs0Header(byte[] data)
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