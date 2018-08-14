using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace XCI.Explorer.Helpers
{
    public static class Util
    {
        public static readonly string[] Language =
        {
            "American English",
            "British English",
            "Japanese",
            "French",
            "German",
            "Latin American Spanish",
            "Spanish",
            "Italian",
            "Dutch",
            "Canadian French",
            "Portuguese",
            "Russian",
            "Korean",
            "Taiwanese",
            "Chinese",
            "???"
        };

        public static readonly string[] SizeCategories =
        {
            "Bytes",
            "Kilobytes",
            "Megabytes",
            "Gigabytes",
            "TB"
        };

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

        public static string GetMasterKeyVersion(byte id)
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

        public static string Sha256Bytes(byte[] ba)
        {
            var mySha256 = SHA256.Create();
            var hashValue = mySha256.ComputeHash(ba);
            return ByteArrayToString(hashValue);
        }

        //https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa
        public static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2 + 2);
            hex.Append("0x");
            foreach (var b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
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

        public static string GetFileSizeCategory(decimal fileSizeAsBytes, SizeCategories sizeCategory)
        {
            const int kilobyte = 1024;
            const int megabyte = 1048576;
            const int gigabyte = 1073741824;

            if (sizeCategory == Helpers.SizeCategories.Gigabytes)
            {
                var fileSize = decimal.Divide(fileSizeAsBytes, gigabyte);
                return $"{fileSize:##.##} GB";
            }
            if (sizeCategory == Helpers.SizeCategories.Megabytes) 
            {
                var fileSize = decimal.Divide(fileSizeAsBytes, megabyte);
                return $"{fileSize:##.##} MB";
            }
            if (sizeCategory == Helpers.SizeCategories.Kilobytes)
            {
                var fileSize = decimal.Divide(fileSizeAsBytes, kilobyte);
                return $"{fileSize:##.##} KB";
            }
            if (sizeCategory == Helpers.SizeCategories.Bytes)
            {
                decimal fileSize = fileSizeAsBytes;
                return $"{fileSize:##.##} Bytes";
            }
            return "0 Bytes";
        }
    }
}