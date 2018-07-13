using System.Linq;
using System.Text;

namespace XCI.Model
{
    public static partial class Nacp
    {
        public class NacpData
        {
            public byte[] Data;
            public string GameProd;
            public string GameVer;

            public NacpData(byte[] data)
            {
                Data = data;
                GameVer = Encoding.UTF8.GetString(Data.Skip(0x60).Take(16).ToArray());
                GameProd = Encoding.UTF8.GetString(Data.Skip(0xA8).Take(8).ToArray());
            }
        }
    }
}