using System;
using System.Linq;
using System.Text;

namespace XCI_Explorer
{
	internal static class HFS0
	{
		public class HFS0_Header
		{
			public byte[] Data;

			public string Magic;

			public int FileCount;

			public int StringTableSize;

			public int Reserved;

			public HFS0_Header(byte[] data)
			{
				Data = data;
				Magic = Encoding.UTF8.GetString(Data.Take(4).ToArray());
				FileCount = BitConverter.ToInt32(data, 4);
				StringTableSize = BitConverter.ToInt32(data, 8);
				Reserved = BitConverter.ToInt32(data, 12);
			}
		}

		public class HSF0_Entry
		{
			public byte[] Data;

			public long Offset;

			public long Size;

			public int Name_ptr;

			public int HashedRegionSize;

			public long Padding;

			public byte[] Hash;

			public string Name;

			public HSF0_Entry(byte[] data)
			{
				Data = data;
				Offset = BitConverter.ToInt64(data, 0);
				Size = BitConverter.ToInt64(data, 8);
				Name_ptr = BitConverter.ToInt32(data, 16);
				HashedRegionSize = BitConverter.ToInt32(data, 20);
				Padding = BitConverter.ToInt64(data, 24);
				Hash = Data.Skip(32).Take(32).ToArray();
			}
		}

		public static HFS0_Header[] HFS0_Headers = new HFS0_Header[1];
	}
}
