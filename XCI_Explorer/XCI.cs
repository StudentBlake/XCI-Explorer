using System;
using System.Linq;
using System.Text;

namespace XCI_Explorer
{
	public static class XCI
	{
		public class XCI_Header
		{
			public byte[] Data;

			public string Magic;

			public byte CardSize1;

			public long CardSize2;

			public long HFS0OffsetPartition;

			public long HFS0SizeParition;

			public XCI_Header(byte[] data)
			{
				Data = data;
				Magic = Encoding.UTF8.GetString(Data.Skip(256).Take(4).ToArray());
				CardSize1 = Data[269];
				CardSize2 = BitConverter.ToInt64(data, 280);
				HFS0OffsetPartition = BitConverter.ToInt64(data, 304);
				HFS0SizeParition = BitConverter.ToInt64(data, 312);
			}
		}

		public static XCI_Header[] XCI_Headers = new XCI_Header[1];
	}
}
