using System;
using System.Linq;
using System.Text;

namespace XCI.Model
{
    public static partial class Nca
    {
        public class NcaHeader
        {
            public byte[] Data;
            public string Magic;
            public byte MasterKeyRev;
            public byte SdkVersion1;
            public byte SdkVersion2;
            public byte SdkVersion3;
            public byte SdkVersion4;
            public long TitleId;

            public NcaHeader(byte[] data)
            {
                Data = data;
                Magic = Encoding.UTF8.GetString(Data.Skip(512).Take(4).ToArray());
                TitleId = BitConverter.ToInt64(data, 528);
                SdkVersion1 = Data[540];
                SdkVersion2 = Data[541];
                SdkVersion3 = Data[542];
                SdkVersion4 = Data[543];
                MasterKeyRev = Data[544];
            }
        }
    }
}