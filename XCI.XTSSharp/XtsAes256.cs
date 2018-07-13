using System;
using System.Security.Cryptography;

namespace XCI.XTSSharp
{
    public class XtsAes256 : Xts
    {

        protected XtsAes256(Func<SymmetricAlgorithm> create, byte[] key1, byte[] key2)
            : base(create, VerifyKey(256, key1), VerifyKey(256, key2))
        {
        }

        public static Xts Create(byte[] key1, byte[] key2)
        {
            VerifyKey(256, key1);
            VerifyKey(256, key2);
            return new XtsAes256(Aes.Create, key1, key2);
        }

        public static Xts Create(byte[] key)
        {
            VerifyKey(512, key);
            var array = new byte[32];
            var array2 = new byte[32];
            Buffer.BlockCopy(key, 0, array, 0, 32);
            Buffer.BlockCopy(key, 32, array2, 0, 32);
            return new XtsAes256(Aes.Create, array, array2);
        }
    }
}