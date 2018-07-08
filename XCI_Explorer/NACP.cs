using System.Linq;
using System.Text;

namespace XCI_Explorer {
    public static class NACP {
        public class NACP_String {
            public byte[] Data;
            public byte Check;
            public string GameName;
            public string GameAuthor;

            public NACP_String(byte[] data) {
                Data = data;
                Check = Data[0];
                GameName = Encoding.UTF8.GetString(Data.Take(512).ToArray());
                GameAuthor = Encoding.UTF8.GetString(Data.Skip(512).Take(256).ToArray());
            }
        }

        public class NACP_Data {
            public byte[] Data;
            public string GameVer;
            public string GameProd;

            public NACP_Data(byte[] data) {
                Data = data;
                GameVer = Encoding.UTF8.GetString(Data.Skip(0x60).Take(16).ToArray());
                GameProd = Encoding.UTF8.GetString(Data.Skip(0xA8).Take(8).ToArray());
            }
        }

        public static NACP_String[] NACP_Strings = new NACP_String[16];

        public static NACP_Data[] NACP_Datas = new NACP_Data[1];
    }
}
