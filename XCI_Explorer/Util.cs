using System;
using System.Linq;
using System.Text;

namespace XCI_Explorer;

internal static class Util
{
    public static string GetCapacity(int id)
    {
        return id switch
        {
            250 => "1GB",
            248 => "2GB",
            240 => "4GB",
            224 => "8GB",
            225 => "16GB",
            226 => "32GB",
            _ => "?",
        };
    }

    public static string GetMkey(byte id)
    {
        return id switch
        {
            0 or 1 => "MasterKey0 (1.0.0-2.3.0)",
            2 => "MasterKey1 (3.0.0)",
            3 => "MasterKey2 (3.0.1-3.0.2)",
            4 => "MasterKey3 (4.0.0-4.1.0)",
            5 => "MasterKey4 (5.0.0-5.1.0)",
            6 => "MasterKey5 (6.0.0-6.1.0)",
            7 => "MasterKey6 (6.2.0)",
            8 => "MasterKey7 (7.0.0-8.0.1)",
            9 => "MasterKey8 (8.1.0-8.1.1)",
            10 => "MasterKey9 (9.0.0-9.0.1)",
            11 => "MasterKey10 (9.1.0-12.0.3)",
            12 => "MasterKey11 (12.1.0)",
            13 => "MasterKey12 (13.0.0-13.2.1)",
            14 => "MasterKey13 (14.0.0-14.1.2)",
            15 => "MasterKey14 (15.0.0-15.0.1)",
            16 => "MasterKey15 (16.0.0-16.1.0)",
            17 => "MasterKey16 (17.0.0-?)",
            18 => "MasterKey17",
            19 => "MasterKey18",
            20 => "MasterKey19",
            21 => "MasterKey20",
            22 => "MasterKey21",
            23 => "MasterKey22",
            24 => "MasterKey23",
            25 => "MasterKey24",
            26 => "MasterKey25",
            27 => "MasterKey26",
            28 => "MasterKey27",
            29 => "MasterKey28",
            30 => "MasterKey29",
            31 => "MasterKey30",
            32 => "MasterKey31",
            33 => "MasterKey32",
            _ => "?",
        };
    }

    public static byte[] StringToByteArray(string hex) => (from x in Enumerable.Range(0, hex.Length)
                                                           where x % 2 == 0
                                                           select Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();

    public static string Base64Encode(string plainText)
    => Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));

    public static string Base64Decode(string base64EncodedData)
    => Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
}
