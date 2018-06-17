using System;
using System.Linq;
using System.Text;

namespace XCI_Explorer
{
	internal static class NCA
	{
		public class NCA_Header
		{
			public byte[] Data;

			public string Magic;

			public long TitleID;

			public byte SDKVersion1;

			public byte SDKVersion2;

			public byte SDKVersion3;

			public byte SDKVersion4;

			public byte MasterKeyRev;

			public NCA_Header(byte[] data)
			{
				Data = data;
				Magic = Encoding.UTF8.GetString(Data.Skip(512).Take(4).ToArray());
				TitleID = BitConverter.ToInt64(data, 528);
				SDKVersion1 = Data[540];
				SDKVersion2 = Data[541];
				SDKVersion3 = Data[542];
				SDKVersion4 = Data[543];
				MasterKeyRev = Data[544];
			}
		}

		public static NCA_Header[] NCA_Headers = new NCA_Header[1];
	}
}
