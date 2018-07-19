using System;
using System.IO;
using System.Linq;

namespace XCI.Explorer.Helpers
{
    public static class Util
    {
        public static string GetCapacity(int id)
        {
            switch (id)
            {
                case 248:
                    return "2GB";
                case 240:
                    return "4GB";
                case 224:
                    return "8GB";
                case 225:
                    return "16GB";
                case 226:
                    return "32GB";
                default:
                    return "Unrecognized Size";
            }
        }

        public static string GetMasterKey(byte id)
        {
            switch (id)
            {
                case 0:
                case 1:
                    return "MasterKey0 (1.0.0-2.3.0)";
                case 2:
                    return "MasterKey1 (3.0.0)";
                case 3:
                    return "MasterKey2 (3.0.1-3.0.2)";
                case 4:
                    return "MasterKey3 (4.0.0-4.1.0)";
                case 5:
                    return "MasterKey4 (5.0.0+)";
                default:
                    return "MasterKey unknown";
            }
        }

        public static bool CheckFile(string filepath)
        {
            return File.Exists(filepath);
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            return (from x in Enumerable.Range(0, hex.Length)
                where x % 2 == 0
                select Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
        }
    }
}