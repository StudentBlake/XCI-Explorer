using System;
using System.Security.Cryptography;

namespace XTSSharp {
    public class Xts {
        private readonly SymmetricAlgorithm _key1;
        private readonly SymmetricAlgorithm _key2;

        protected Xts(Func<SymmetricAlgorithm> create, byte[] key1, byte[] key2) {
            if (create == null) {
                throw new ArgumentNullException("create");
            }
            if (key1 == null) {
                throw new ArgumentNullException("key1");
            }
            if (key2 == null) {
                throw new ArgumentNullException("key2");
            }
            _key1 = create();
            _key2 = create();
            if (key1.Length != key2.Length) {
                throw new ArgumentException("Key lengths don't match");
            }
            _key1.KeySize = key1.Length * 8;
            _key2.KeySize = key2.Length * 8;
            _key1.Key = key1;
            _key2.Key = key2;
            _key1.Mode = CipherMode.ECB;
            _key2.Mode = CipherMode.ECB;
            _key1.Padding = PaddingMode.None;
            _key2.Padding = PaddingMode.None;
            _key1.BlockSize = 128;
            _key2.BlockSize = 128;
        }

        public XtsCryptoTransform CreateEncryptor() {
            return new XtsCryptoTransform(_key1.CreateEncryptor(), _key2.CreateEncryptor(), false);
        }

        public XtsCryptoTransform CreateDecryptor() {
            return new XtsCryptoTransform(_key1.CreateDecryptor(), _key2.CreateEncryptor(), true);
        }

        protected static byte[] VerifyKey(int expectedSize, byte[] key) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }
            if (key.Length * 8 != expectedSize) {
                throw new ArgumentException($"Expected key length of {expectedSize} bits, got {key.Length * 8}");
            }
            return key;
        }
    }
}
