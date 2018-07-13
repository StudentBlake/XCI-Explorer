using System;
using System.IO;

namespace XCI.XTSSharp
{
    public class RandomAccessSectorStream : Stream
    {
        private readonly byte[] _buffer;
        private readonly int _bufferSize;
        private readonly bool _isStreamOwned;
        private readonly SectorStream _s;
        private bool _bufferDirty;
        private bool _bufferLoaded;
        private int _bufferPos;

        public RandomAccessSectorStream(SectorStream s)
            : this(s, false)
        {
        }

        public RandomAccessSectorStream(SectorStream s, bool isStreamOwned)
        {
            _s = s;
            _isStreamOwned = isStreamOwned;
            _buffer = new byte[s.SectorSize];
            _bufferSize = s.SectorSize;
        }

        public override bool CanRead => _s.CanRead;
        public override bool CanSeek => _s.CanSeek;
        public override bool CanWrite => _s.CanWrite;
        public override long Length => _s.Length + _bufferPos;

        public override long Position
        {
            get
            {
                if (!_bufferLoaded) return _s.Position + _bufferPos;
                return _s.Position - _bufferSize + _bufferPos;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                var num = value % _bufferSize;
                var position = value - num;
                if (_bufferLoaded)
                {
                    var num2 = _s.Position - _bufferSize;
                    if (value > num2 && value < num2 + _bufferSize)
                    {
                        _bufferPos = (int) num;
                        return;
                    }
                }
                if (_bufferDirty) WriteSector();
                _s.Position = position;
                ReadSector();
                _bufferPos = (int) num;
            }
        }

        protected override void Dispose(bool disposing)
        {
            Flush();
            base.Dispose(disposing);
            if (_isStreamOwned) _s.Dispose();
        }

        public override void Flush()
        {
            if (_bufferDirty) WriteSector();
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
            var num = value % _s.SectorSize;
            if (num > 0) value = value - num + _bufferSize;
            _s.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var position = Position;
            if (position + count > _s.Length) count = (int) (_s.Length - position);
            if (!_bufferLoaded) ReadSector();
            var num = 0;
            while (count > 0)
            {
                var num2 = Math.Min(count, _bufferSize - _bufferPos);
                Buffer.BlockCopy(_buffer, _bufferPos, buffer, offset, num2);
                offset += num2;
                _bufferPos += num2;
                count -= num2;
                num += num2;
                if (_bufferPos == _bufferSize) ReadSector();
            }
            return num;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                if (!_bufferLoaded) ReadSector();
                var num = Math.Min(count, _bufferSize - _bufferPos);
                Buffer.BlockCopy(buffer, offset, _buffer, _bufferPos, num);
                offset += num;
                _bufferPos += num;
                count -= num;
                _bufferDirty = true;
                if (_bufferPos == _bufferSize) WriteSector();
            }
        }

        private void ReadSector()
        {
            if (_bufferLoaded && _bufferDirty) WriteSector();
            if (_s.Position == _s.Length) return;
            var num = _s.Read(_buffer, 0, _buffer.Length);
            if (num != _bufferSize) Array.Clear(_buffer, num, _buffer.Length - num);
            _bufferLoaded = true;
            _bufferPos = 0;
            _bufferDirty = false;
        }

        private void WriteSector()
        {
            if (_bufferLoaded) _s.Seek(-_bufferSize, SeekOrigin.Current);
            _s.Write(_buffer, 0, _bufferSize);
            _bufferDirty = false;
            _bufferLoaded = false;
            _bufferPos = 0;
            Array.Clear(_buffer, 0, _bufferSize);
        }
    }
}