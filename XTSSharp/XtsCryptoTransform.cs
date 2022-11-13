using System;
using System.Security.Cryptography;

namespace XTSSharp;

public class XtsCryptoTransform : IDisposable
{
    private readonly byte[] _cc = new byte[16];
    private readonly bool _decrypting;
    private readonly ICryptoTransform _key1;
    private readonly ICryptoTransform _key2;
    private readonly byte[] _pp = new byte[16];
    private readonly byte[] _t = new byte[16];
    private readonly byte[] _tweak = new byte[16];

    public XtsCryptoTransform(ICryptoTransform key1, ICryptoTransform key2, bool decrypting)
    {
        if (key1 == null)
        {
            throw new ArgumentNullException(nameof(key1));
        }
        if (key2 == null)
        {
            throw new ArgumentNullException(nameof(key2));
        }
        _key1 = key1;
        _key2 = key2;
        _decrypting = decrypting;
    }

    public void Dispose()
    {
        _key1.Dispose();
        _key2.Dispose();
    }

    public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset, ulong sector)
    {
        FillArrayFromSectorLittleEndian(_tweak, sector);
        int num = inputCount >> 4;
        int num2 = inputCount & 0xF;
        _key2.TransformBlock(_tweak, 0, _tweak.Length, _t, 0);
        int num3 = (num2 != 0) ? (num - 1) : num;
        for (int i = 0; i < num3; i++)
        {
            TweakCrypt(inputBuffer, inputOffset, outputBuffer, outputOffset, _t);
            inputOffset += 16;
            outputOffset += 16;
        }
        if (num2 > 0)
        {
            if (_decrypting)
            {
                Buffer.BlockCopy(_t, 0, _cc, 0, 16);
                MultiplyByX(_cc);
                TweakCrypt(inputBuffer, inputOffset, _pp, 0, _cc);
                int j;
                for (j = 0; j < num2; j++)
                {
                    _cc[j] = inputBuffer[16 + j + inputOffset];
                    outputBuffer[16 + j + outputOffset] = _pp[j];
                }
                for (; j < 16; j++)
                {
                    _cc[j] = _pp[j];
                }
                TweakCrypt(_cc, 0, outputBuffer, outputOffset, _t);
            }
            else
            {
                TweakCrypt(inputBuffer, inputOffset, _cc, 0, _t);
                int k;
                for (k = 0; k < num2; k++)
                {
                    _pp[k] = inputBuffer[16 + k + inputOffset];
                    outputBuffer[16 + k + outputOffset] = _cc[k];
                }
                for (; k < 16; k++)
                {
                    _pp[k] = _cc[k];
                }
                TweakCrypt(_pp, 0, outputBuffer, outputOffset, _t);
            }
        }
        return inputCount;
    }

    private static void FillArrayFromSectorBigEndian(byte[] value, ulong sector)
    {
        value[7] = (byte)((sector >> 56) & 0xFF);
        value[6] = (byte)((sector >> 48) & 0xFF);
        value[5] = (byte)((sector >> 40) & 0xFF);
        value[4] = (byte)((sector >> 32) & 0xFF);
        value[3] = (byte)((sector >> 24) & 0xFF);
        value[2] = (byte)((sector >> 16) & 0xFF);
        value[1] = (byte)((sector >> 8) & 0xFF);
        value[0] = (byte)(sector & 0xFF);
    }

    private static void FillArrayFromSectorLittleEndian(byte[] value, ulong sector)
    {
        value[8] = (byte)((sector >> 56) & 0xFF);
        value[9] = (byte)((sector >> 48) & 0xFF);
        value[10] = (byte)((sector >> 40) & 0xFF);
        value[11] = (byte)((sector >> 32) & 0xFF);
        value[12] = (byte)((sector >> 24) & 0xFF);
        value[13] = (byte)((sector >> 16) & 0xFF);
        value[14] = (byte)((sector >> 8) & 0xFF);
        value[15] = (byte)(sector & 0xFF);
    }

    private void TweakCrypt(byte[] inputBuffer, int inputOffset, byte[] outputBuffer, int outputOffset, byte[] t)
    {
        for (int i = 0; i < 16; i++)
        {
            outputBuffer[i + outputOffset] = (byte)(inputBuffer[i + inputOffset] ^ t[i]);
        }
        _key1.TransformBlock(outputBuffer, outputOffset, 16, outputBuffer, outputOffset);
        for (int j = 0; j < 16; j++)
        {
            outputBuffer[j + outputOffset] = (byte)(outputBuffer[j + outputOffset] ^ t[j]);
        }
        MultiplyByX(t);
    }

    private static void MultiplyByX(byte[] i)
    {
        byte b = 0;
        byte b2 = 0;
        for (int j = 0; j < 16; j++)
        {
            b2 = (byte)(i[j] >> 7);
            i[j] = (byte)(((i[j] << 1) | b) & 0xFF);
            b = b2;
        }
        if (b2 > 0)
        {
            i[0] ^= 135;
        }
    }
}
