using System;
using System.Linq;
using System.Text;

namespace XCI.Model
{
    public static partial class Xci
    {
        public class XciHeader
        {
            public byte CardSize1;
            public long CardSize2;
            public byte[] Data;
            public long Hfs0OffsetPartition;
            public long Hfs0SizeParition;
            public string Magic;

            public XciHeader(byte[] data)
            {
                Data = data;
                Magic = Encoding.UTF8.GetString(Data.Skip(256).Take(4).ToArray());
                CardSize1 = Data[269];
                CardSize2 = BitConverter.ToInt64(data, 280);
                Hfs0OffsetPartition = BitConverter.ToInt64(data, 304);
                Hfs0SizeParition = BitConverter.ToInt64(data, 312);
            }
        }
    }
}