using System;
using System.IO;

namespace XCI_Explorer.Helpers
{
	public class BetterBinaryReader : IDisposable
	{
		public string FileName;

		public bool Initiated;

		public Stream Stream;

		private BinaryReader br;

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
			br = new BinaryReader(Stream);
		}

		public void Dispose()
		{
			Initiated = false;
			br.Close();
			br = null;
			Stream.Close();
			Stream = null;
		}

		public void Load(string file)
		{
			FileName = file;
			Stream = new FileStream(file, FileMode.Open);
			br = new BinaryReader(Stream);
			Initiated = true;
		}

		public void Seek(long o)
		{
			if (o > -1)
			{
				Stream.Seek(o, SeekOrigin.Begin);
			}
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
			return br.ReadBytes(1)[0];
		}

		public int Read(byte[] buffer, int index, int count)
		{
			return br.Read(buffer, index, count);
		}

		public int Read(char[] buffer, int index, int count)
		{
			return br.Read(buffer, index, count);
		}

		public byte[] ReadBytes(int l)
		{
			if (l >= 0 && l <= 2147483647)
			{
				return br.ReadBytes(l);
			}
			return new byte[0];
		}

		public Stream ReadBytesButLonger(long l)
		{
			MemoryStream memoryStream = new MemoryStream();
			for (long num = 0L; num < l; num++)
			{
			}
			Console.WriteLine(memoryStream.Length);
			return memoryStream;
		}

		public string ReadCharsAsString(int l)
		{
			return new string(br.ReadChars(l));
		}

		public short ReadShort()
		{
			return br.ReadInt16();
		}

		public short ReadInt16()
		{
			return br.ReadInt16();
		}

		public int ReadInt()
		{
			return br.ReadInt32();
		}

		public int ReadInt32()
		{
			return br.ReadInt32();
		}

		public long ReadLong()
		{
			return br.ReadInt64();
		}

		public long ReadInt64()
		{
			return br.ReadInt64();
		}

		public string ReadString()
		{
			return br.ReadString();
		}

		private long GreatestDivisor(long n)
		{
			long result = 0L;
			for (long num = 1L; num < n / 64; num++)
			{
				if (n % num == 0L && num != n)
				{
					result = num;
				}
			}
			return result;
		}
	}
}
