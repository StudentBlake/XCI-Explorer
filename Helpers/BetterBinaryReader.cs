using System;
using System.IO;

namespace XCI.Explorer.Helpers
{
    public class BetterBinaryReader : IDisposable
    {
        private BinaryReader _br;
        public string FileName;
        public bool Initiated;
        public Stream Stream;

        public BetterBinaryReader()
        {
            Initiated = false;
        }

        public BetterBinaryReader(string file)
        {
            Initiated = false;
            Load(file);
        }

        public BetterBinaryReader(Stream s)
        {
            Initiated = false;
            FileName = "";
            Stream = s;
            _br = new BinaryReader(Stream);
        }

        public void Dispose()
        {
            Initiated = false;
            _br.Close();
            _br = null;
            Stream.Close();
            Stream = null;
        }

        public void Load(string file)
        {
            FileName = file;
            Stream = new FileStream(file, FileMode.Open);
            _br = new BinaryReader(Stream);
            Initiated = true;
        }

        public void Seek(long o)
        {
            if (o > -1) Stream.Seek(o, SeekOrigin.Begin);
        }

        public void Skip(long o)
        {
            Stream.Seek(o, SeekOrigin.Current);
        }

        public long Position()
        {
            return Stream.Position;
        }

        public int Read()
        {
            return _br.ReadBytes(1)[0];
        }

        public int Read(byte[] buffer, int index, int count)
        {
            return _br.Read(buffer, index, count);
        }

        public int Read(char[] buffer, int index, int count)
        {
            return _br.Read(buffer, index, count);
        }

        public byte[] ReadBytes(int l)
        {
            return l >= 0 ? _br.ReadBytes(l) : new byte[0];
        }

        public Stream ReadBytesButLonger(long l)
        {
            var memoryStream = new MemoryStream();
            Console.WriteLine(memoryStream.Length);
            return memoryStream;
        }

        public string ReadCharsAsString(int l)
        {
            return new string(_br.ReadChars(l));
        }

        public short ReadShort()
        {
            return _br.ReadInt16();
        }

        public short ReadInt16()
        {
            return _br.ReadInt16();
        }

        public int ReadInt()
        {
            return _br.ReadInt32();
        }

        public int ReadInt32()
        {
            return _br.ReadInt32();
        }

        public long ReadLong()
        {
            return _br.ReadInt64();
        }

        public long ReadInt64()
        {
            return _br.ReadInt64();
        }

        public string ReadString()
        {
            return _br.ReadString();
        }
    }
}