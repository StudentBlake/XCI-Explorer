using System.Linq;
using System.Text;

namespace XCI.Model
{
    public static partial class Nacp
    {
        public class NacpString
        {
            public byte Check;
            public byte[] Data;
            public string GameAuthor;
            public string GameName;

            public NacpString(byte[] data)
            {
                Data = data;
                Check = Data[0];
                GameName = Encoding.UTF8.GetString(Data.Take(512).ToArray());
                GameAuthor = Encoding.UTF8.GetString(Data.Skip(512).Take(256).ToArray());
            }
        }
    }
}