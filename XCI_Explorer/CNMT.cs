using System;
using System.Linq;

namespace XCI_Explorer;

internal static class CNMT
{
    public class CNMT_Header
    {
        public byte[] Data;
        public long TitleID;
        public int TitleVersion;
        public byte Type;
        public byte Reserved1;
        public short Offset;
        public short ContentCount;
        public short MetaCount;
        public byte[] Reserved2;

        public enum TitleType
        {
            SYSTEM_PROGRAMS = 0x01,
            SYSTEM_DATA_ARCHIVES,
            SYSTEM_UPDATE,
            FIRMWARE_PACKAGE_A,
            FIRMWARE_PACKAGE_B,
            REGULAR_APPLICATION = 0x80,
            UPDATE_TITLE,
            ADD_ON_CONTENT,
            DELTA_TITLE
        }

        public CNMT_Header(byte[] data)
        {
            Data = data;
            TitleID = BitConverter.ToInt64(data, 0);
            TitleVersion = BitConverter.ToInt32(data, 8);
            Type = Data[12];
            Reserved1 = Data[13];
            Offset = BitConverter.ToInt16(data, 14);
            ContentCount = BitConverter.ToInt16(data, 16);
            MetaCount = BitConverter.ToInt16(data, 16);
            Reserved2 = Data.Skip(20).Take(12).ToArray();
        }
    }

    public class CNMT_Entry
    {
        public byte[] Data;
        public byte[] Hash;
        public byte[] NcaId;
        public long Size;
        public byte Type;
        public byte Reserved;

        public enum ContentType
        {
            META,
            PROGRAM,
            DATA,
            CONTROL,
            OFFLINE_MANUAL,
            LEGAL,
            GAME_UPDATE
        }

        public CNMT_Entry(byte[] data)
        {
            Data = data;
            Hash = Data.Skip(0).Take(32).ToArray();
            NcaId = Data.Skip(32).Take(16).ToArray();
            Size = BitConverter.ToInt32(data, 48) + BitConverter.ToInt16(data, 52) * 65536;
            Type = Data[54];
            Reserved = Data[55];
        }
    }

    public static CNMT_Header[] CNMT_Headers = new CNMT_Header[1];
}
