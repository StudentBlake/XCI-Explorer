using System;
using System.IO;

namespace XCI.XTSSharp
{
    public class SectorStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly long _offset;

        public SectorStream(Stream baseStream, int sectorSize)
            : this(baseStream, sectorSize, 0L)
        {
        }

        public SectorStream(Stream baseStream, int sectorSize, long offset)
        {
            SectorSize = sectorSize;
            _baseStream = baseStream;
            _offset = offset;
        }

        public int SectorSize { get; }

        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => _baseStream.CanWrite;
        public override long Length => _baseStream.Length - _offset;

        public override long Position
        {
            get => _baseStream.Position - _offset;
            set
            {
                ValidateSizeMultiple(value);
                _baseStream.Position = value + _offset;
                CurrentSector = (ulong) (value / SectorSize);
            }
        }

        protected ulong CurrentSector { get; private set; }

        private void ValidateSizeMultiple(long value)
        {
            if (value % SectorSize == 0L) return;
            throw new ArgumentException($"Value needs to be a multiple of {SectorSize}");
        }

        protected void ValidateSize(long value)
        {
            if (value == SectorSize) return;
            throw new ArgumentException($"Value needs to be {SectorSize}");
        }

        protected void ValidateSize(int value)
        {
            if (value == SectorSize) return;
            throw new ArgumentException($"Value needs to be {SectorSize}");
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long num;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    num = offset;
                    break;
                case SeekOrigin.End:
                    num = Length - offset;
                    break;
                default:
                    num = Position + offset;
                    break;
            }
            Position = num;
            return num;
        }

        public override void SetLength(long value)
        {
            ValidateSizeMultiple(value);
            _baseStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ValidateSize(count);
            var result = _baseStream.Read(buffer, offset, count);
            CurrentSector++;
            return result;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ValidateSize(count);
            _baseStream.Write(buffer, offset, count);
            CurrentSector++;
        }
    }
}