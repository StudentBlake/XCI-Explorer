using System;
using System.Linq;
using System.Text;

namespace XCI_Explorer
{
    internal static class PFS0
    {
        public class PFS0_Header
        {
            public byte[] Data;
            public string Magic;
            public int FileCount;
            public int StringTableSize;
            public int Reserved;

            public PFS0_Header(byte[] data)
            {
                Data = data;
                Magic = Encoding.UTF8.GetString(Data.Take(4).ToArray());
                FileCount = BitConverter.ToInt32(data, 4);
                StringTableSize = BitConverter.ToInt32(data, 8);
                Reserved = BitConverter.ToInt32(data, 12);
            }
        }

        public class PFS0_Entry
        {
            public byte[] Data;
            public long Offset;
            public long Size;
            public int Name_ptr;
            public int Reserved;
            public string Name;

            public PFS0_Entry(byte[] data)
            {
                Data = data;
                Offset = BitConverter.ToInt64(data, 0);
                Size = BitConverter.ToInt64(data, 8);
                Name_ptr = BitConverter.ToInt32(data, 16);
                Reserved = BitConverter.ToInt32(data, 20);
            }
        }

        public static PFS0_Header[] PFS0_Headers = new PFS0_Header[1];
    }
}
