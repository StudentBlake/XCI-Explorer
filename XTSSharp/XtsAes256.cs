using System;
using System.Security.Cryptography;

namespace XTSSharp {
    public class XtsAes256 : Xts {
        private const int KEY_LENGTH = 256;
        private const int KEY_BYTE_LENGTH = 32;

        protected XtsAes256(Func<SymmetricAlgorithm> create, byte[] key1, byte[] key2)
            : base(create, Xts.VerifyKey(256, key1), Xts.VerifyKey(256, key2)) {
        }

        public static Xts Create(byte[] key1, byte[] key2) {
            Xts.VerifyKey(256, key1);
            Xts.VerifyKey(256, key2);
            return new XtsAes256(Aes.Create, key1, key2);
        }

        public static Xts Create(byte[] key) {
            Xts.VerifyKey(512, key);
            byte[] array = new byte[32];
            byte[] array2 = new byte[32];
            Buffer.BlockCopy(key, 0, array, 0, 32);
            Buffer.BlockCopy(key, 32, array2, 0, 32);
            return new XtsAes256(Aes.Create, array, array2);
        }
    }
}
