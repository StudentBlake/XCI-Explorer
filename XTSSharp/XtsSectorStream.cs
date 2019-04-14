using System.IO;

namespace XTSSharp
{
    public class XtsSectorStream : SectorStream
    {
        public const int DEFAULT_SECTOR_SIZE = 512;
        private readonly byte[] _tempBuffer;
        private readonly Xts _xts;
        private XtsCryptoTransform _decryptor;
        private XtsCryptoTransform _encryptor;

        public XtsSectorStream(Stream baseStream, Xts xts)
            : this(baseStream, xts, 512)
        {
        }

        public XtsSectorStream(Stream baseStream, Xts xts, int sectorSize)
            : this(baseStream, xts, sectorSize, 0L)
        {
        }

        public XtsSectorStream(Stream baseStream, Xts xts, int sectorSize, long offset)
            : base(baseStream, sectorSize, offset)
        {
            _xts = xts;
            _tempBuffer = new byte[sectorSize];
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_encryptor != null)
            {
                _encryptor.Dispose();
            }
            if (_decryptor != null)
            {
                _decryptor.Dispose();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ValidateSize(count);
            if (count != 0)
            {
                ulong currentSector = base.CurrentSector;
                if (_encryptor == null)
                {
                    _encryptor = _xts.CreateEncryptor();
                }
                int count2 = _encryptor.TransformBlock(buffer, offset, count, _tempBuffer, 0, currentSector);
                base.Write(_tempBuffer, 0, count2);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ValidateSize(count);
            ulong currentSector = base.CurrentSector;
            int num = base.Read(_tempBuffer, 0, count);
            if (num == 0)
            {
                return 0;
            }
            if (_decryptor == null)
            {
                _decryptor = _xts.CreateDecryptor();
            }
            return _decryptor.TransformBlock(_tempBuffer, 0, num, buffer, offset, currentSector);
        }
    }
}
