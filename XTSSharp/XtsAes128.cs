using System;
using System.Security.Cryptography;

namespace XTSSharp;

public class XtsAes128 : Xts
{
    private const int KEY_LENGTH = 128;
    private const int KEY_BYTE_LENGTH = 16;

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
        byte[] array = new byte[16];
        byte[] array2 = new byte[16];
        Buffer.BlockCopy(key, 0, array, 0, 16);
        Buffer.BlockCopy(key, 16, array2, 0, 16);
        return new XtsAes128(Aes.Create, array, array2);
    }
}
