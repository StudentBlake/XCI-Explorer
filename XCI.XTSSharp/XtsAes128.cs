using System;
using System.Security.Cryptography;

namespace XCI.XTSSharp
{
    public class XtsAes128 : Xts
    {

        protected XtsAes128(Func<SymmetricAlgorithm> create, byte[] key1, byte[] key2)
            : base(create, VerifyKey(128, key1), VerifyKey(128, key2))
        {
        }

        public static Xts Create(byte[] key1, byte[] key2)
        {
            VerifyKey(128, key1);
            VerifyKey(128, key2);
            return new XtsAes128(Aes.Create, key1, key2);
        }

        public static Xts Create(byte[] key)
        {
            VerifyKey(256, key);
            var array = new byte[16];
            var array2 = new byte[16];
            Buffer.BlockCopy(key, 0, array, 0, 16);
            Buffer.BlockCopy(key, 16, array2, 0, 16);
            return new XtsAes128(Aes.Create, array, array2);
        }
    }
}