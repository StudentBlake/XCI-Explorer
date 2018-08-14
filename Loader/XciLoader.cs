using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using XCI.Explorer.DTO;
using XCI.Explorer.Helpers;
using XCI.Model;
using XCI.XTSSharp;

namespace XCI.Explorer.Loader
{
    public class XciLoader : ILoader
    {
        private readonly GameDto _gameDto;
        //Don't know what this value is representing
        private const long MagicalSecureSize = -9223372036854775808L;
        //Don't know what this value is representing (apart from being int16.MaxValue + 1)
        private const int MagicNumber = 32768;
        private long[] SecureOffset;
        private long[] SecureSize;
        private long PFS0Offset;
        private long PFS0Size;
        private long _gameNcaOffset;
        public List<char> Chars = new List<char>();
        private long[] NormalOffset;
        private long[] NormalSize;
        private KeyHandler _keyhandler;
        public long GameNcaSize { get; private set; }
        private static long GameDtoUsedSize => Xci.XciHeaders[0].CardSize2 * 512 + 512;
        private static long MagicalNumbersToPosition => Xci.XciHeaders[0].Hfs0OffsetPartition + 16 + 64 * Hfs0.Hfs0Headers[0].FileCount;


        public XciLoader(GameDto gameDto, KeyHandler keyhandler)
        {
            _gameDto = gameDto;
            _keyhandler = keyhandler;
        }

        public void GetRomSize(string filePath)
        {
            decimal fileSizeAsBytes = new FileInfo(filePath).Length;

            _gameDto.ExactSize = Util.GetFileSizeCategory(fileSizeAsBytes, SizeCategories.Bytes);
            _gameDto.Size = Util.GetFileSizeCategory(fileSizeAsBytes, SizeCategories.Gigabytes);

            _gameDto.UsedSize = GameDtoUsedSize;
            _gameDto.ExactUsedSpace = Util.GetFileSizeCategory((decimal)_gameDto.UsedSize, SizeCategories.Bytes);
            
            _gameDto.UsedSpace = Util.GetFileSizeCategory((decimal)_gameDto.UsedSize, SizeCategories.Megabytes);
            _gameDto.Capacity = Util.GetCapacity(Xci.XciHeaders[0].CardSize1);
        }

        public void LoadPartitions(string filename, TreeViewFileSystem tvFileSystem, BetterTreeNode rootNode, TreeView tvPartitions)
        {
            var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var headerEntries = new Hfs0.Hsf0Entry[Hfs0.Hfs0Headers[0].FileCount];
            var partitionOffset = Xci.XciHeaders[0].Hfs0OffsetPartition + Xci.XciHeaders[0].Hfs0SizePartition;
            var array3 = new byte[16];

            fileStream.Position = MagicalNumbersToPosition;

            CreateParentNodes(fileStream, headerEntries, partitionOffset, array3, tvFileSystem, rootNode, tvPartitions);
            CreateBootNodes(fileStream, array3, tvFileSystem, rootNode, tvPartitions);

            fileStream.Close();
        }

        private void CreateParentNodes(FileStream fileStream, Hfs0.Hsf0Entry[] headerEntries, long partitionOffset, byte[] array3, TreeViewFileSystem tvFileSystem, BetterTreeNode rootNode, TreeView tvPartitions)
        {
            var array2 = new byte[64];

            for (var i = 0; i < Hfs0.Hfs0Headers[0].FileCount; i++)
            {
                fileStream.Position = Xci.XciHeaders[0].Hfs0OffsetPartition + 16 + 64 * i;
                fileStream.Read(array2, 0, 64);
                headerEntries[i] = new Hfs0.Hsf0Entry(array2);
                fileStream.Position = Xci.XciHeaders[0].Hfs0OffsetPartition + 16 +
                                      64 * Hfs0.Hfs0Headers[0].FileCount + headerEntries[i].NamePtr;
                int num2;
                while ((num2 = fileStream.ReadByte()) != 0 && num2 != 0) Chars.Add((char)num2);
                headerEntries[i].Name = new string(Chars.ToArray());
                Chars.Clear();

                var offset = partitionOffset + headerEntries[i].Offset;
                var hashBuffer = new byte[headerEntries[i].HashedRegionSize];
                fileStream.Position = offset;
                fileStream.Read(hashBuffer, 0, headerEntries[i].HashedRegionSize);
                var actualHash = Util.Sha256Bytes(hashBuffer);

                tvFileSystem.AddFile(headerEntries[i].Name + ".hfs0", rootNode, offset, headerEntries[i].Size, headerEntries[i].HashedRegionSize,
                    Util.ByteArrayToString(headerEntries[i].Hash), actualHash);
                var betterTreeNode = tvFileSystem.AddDir(headerEntries[i].Name, rootNode);
                var array5 = new Hfs0.Hfs0Header[1];
                fileStream.Position = headerEntries[i].Offset + partitionOffset;
                fileStream.Read(array3, 0, 16);
                array5[0] = new Hfs0.Hfs0Header(array3);
                switch (headerEntries[i].Name)
                {
                    case "secure":
                        SecureSize = new long[array5[0].FileCount];
                        SecureOffset = new long[array5[0].FileCount];
                        break;
                    case "normal":
                        NormalSize = new long[array5[0].FileCount];
                        NormalOffset = new long[array5[0].FileCount];
                        break;
                }

                CreateChildNodes(fileStream, headerEntries, partitionOffset, array5, i, array2, betterTreeNode, tvFileSystem, tvPartitions);
            }
        }
        private void CreateChildNodes(FileStream fileStream, Hfs0.Hsf0Entry[] array, long num, Hfs0.Hfs0Header[] array5, int i, byte[] array2, BetterTreeNode betterTreeNode, TreeViewFileSystem tvFileSystem, TreeView tvPartitions)
        {
            var array6 = new Hfs0.Hsf0Entry[array5[0].FileCount];
            for (var j = 0; j < array5[0].FileCount; j++)
            {
                fileStream.Position = array[i].Offset + num + 16 + 64 * j;
                fileStream.Read(array2, 0, 64);
                array6[j] = new Hfs0.Hsf0Entry(array2);
                fileStream.Position = array[i].Offset + num + 16 + 64 * array5[0].FileCount + array6[j].NamePtr;
                switch (array[i].Name)
                {
                    case "secure":
                        SecureSize[j] = array6[j].Size;
                        SecureOffset[j] = array[i].Offset + array6[j].Offset + num + 16 + array5[0].StringTableSize +
                                          array5[0].FileCount * 64;
                        break;
                    case "normal":
                        NormalSize[j] = array6[j].Size;
                        NormalOffset[j] = array[i].Offset + array6[j].Offset + num + 16 + array5[0].StringTableSize +
                                          array5[0].FileCount * 64;
                        break;
                }
                int num2;
                while ((num2 = fileStream.ReadByte()) != 0 && num2 != 0) Chars.Add((char)num2);
                array6[j].Name = new string(Chars.ToArray());
                Chars.Clear();

                var offset = array[i].Offset + array6[j].Offset + num + 16 + array5[0].StringTableSize +
                              array5[0].FileCount * 64;
                var hashBuffer = new byte[array6[j].HashedRegionSize];
                fileStream.Position = offset;
                fileStream.Read(hashBuffer, 0, array6[j].HashedRegionSize);
                var actualHash = Util.Sha256Bytes(hashBuffer);

                tvFileSystem.AddFile(array6[j].Name, betterTreeNode, offset, array6[j].Size, array6[j].HashedRegionSize,
                    Util.ByteArrayToString(array6[j].Hash), actualHash);
                var array7 = tvPartitions.Nodes.Find(betterTreeNode.Text, true);
                if (array7.Length != 0) tvFileSystem.AddFile(array6[j].Name, (BetterTreeNode)array7[0], 0L, 0L);
            }
        }
        private void CreateBootNodes(FileStream fileStream, byte[] array3, TreeViewFileSystem tvFileSystem, BetterTreeNode rootNode, TreeView tvPartitions)
        {
            var array4 = new byte[24];

            var num3 = -9223372036854775808L;
            for (var k = 0; k < SecureSize.Length; k++)
                if (SecureSize[k] > num3)
                {
                    GameNcaSize = SecureSize[k];
                    _gameNcaOffset = SecureOffset[k];
                    num3 = SecureSize[k];
                }
            PFS0Offset = _gameNcaOffset + 32768;
            fileStream.Position = PFS0Offset;
            fileStream.Read(array3, 0, 16);
            Pfs0.Pfs0Headers[0] = new Pfs0.Pfs0Header(array3);
            var array8 = new Pfs0.Pfs0Entry[Pfs0.Pfs0Headers[0].FileCount];
            for (var m = 0; m < Pfs0.Pfs0Headers[0].FileCount; m++)
            {
                fileStream.Position = PFS0Offset + 16 + 24 * m;
                fileStream.Read(array4, 0, 24);
                array8[m] = new Pfs0.Pfs0Entry(array4);
                PFS0Size += array8[m].Size;
            }
            tvFileSystem.AddFile("boot.psf0", rootNode, PFS0Offset,
                16 + 24 * Pfs0.Pfs0Headers[0].FileCount + 64 + PFS0Size);
            var betterTreeNode2 = tvFileSystem.AddDir("boot", rootNode);
            for (var n = 0; n < Pfs0.Pfs0Headers[0].FileCount; n++)
            {
                fileStream.Position = PFS0Offset + 16 + 24 * Pfs0.Pfs0Headers[0].FileCount + array8[n].NamePtr;
                int num4;
                while ((num4 = fileStream.ReadByte()) != 0 && num4 != 0) Chars.Add((char)num4);
                array8[n].Name = new string(Chars.ToArray());
                Chars.Clear();
                tvFileSystem.AddFile(array8[n].Name, betterTreeNode2,
                    PFS0Offset + array8[n].Offset + 16 + Pfs0.Pfs0Headers[0].StringTableSize +
                    Pfs0.Pfs0Headers[0].FileCount * 24, array8[n].Size);
                var array9 = tvPartitions.Nodes.Find(betterTreeNode2.Text, true);
                if (array9.Length != 0) tvFileSystem.AddFile(array8[n].Name, (BetterTreeNode)array9[0], 0L, 0L);
            }
        }

        public void LoadNca(TextBox TB_TID, TextBox TB_SDKVer, TextBox TB_MKeyRev, string filepath)
        {
            Nca.NcaHeaders[0] = new Nca.NcaHeader(DecryptNcaHeader(_gameNcaOffset, filepath));
            TB_TID.Text = @"0" + Nca.NcaHeaders[0].TitleId.ToString("X");
            TB_SDKVer.Text =
                $@"{Nca.NcaHeaders[0].SdkVersion4}.{Nca.NcaHeaders[0].SdkVersion3}.{Nca.NcaHeaders[0].SdkVersion2}.{
                        Nca.NcaHeaders[0].SdkVersion1
                    }";
            TB_MKeyRev.Text = Util.GetMasterKeyVersion(Nca.NcaHeaders[0].MasterKeyRev);
        }


        public byte[] DecryptNcaHeader(long offset, string filepath)
        {
            var array = new byte[3072];
            if (!File.Exists(filepath)) return array;
            var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read) { Position = offset };
            fileStream.Read(array, 0, 3072);
            File.WriteAllBytes(filepath + ".tmp", array);
            var xts = XtsAes128.Create(_keyhandler.NcaHeaderEncryptionKey1Prod,
                _keyhandler.NcaHeaderEncryptionKey2Prod);
            using (var binaryReader = new BinaryReader(File.OpenRead(filepath + ".tmp")))
            {
                using (var xtsStream = new XtsStream(binaryReader.BaseStream, xts, 512))
                {
                    xtsStream.Read(array, 0, 3072);
                }
            }
            File.Delete(filepath + ".tmp");
            fileStream.Close();
            return array;
        }

        public void LoadInfos(TextBox TB_Name, TextBox TB_Dev, PictureBox PB_GameIcon, string filepath, ComboBox _cbRegionName, Image[] _icons, TextBox TB_GameRev, TextBox TB_ProdCode)
        {
            _cbRegionName.Items.Clear();
            _cbRegionName.Enabled = true;
            TB_Name.Text = "";
            TB_Dev.Text = "";
            PB_GameIcon.BackgroundImage = null;
            Array.Clear(_icons, 0, _icons.Length);
            if (_keyhandler.GetMasterKey(Nca.NcaHeaders.First().MasterKeyRev))
            {
                using (var fileStream = File.OpenRead(filepath))
                {
                    for (var si = 0; si < SecureSize.Length; si++)
                    {
                        if (SecureSize[si] > 0x4E20000) continue;

                        if (File.Exists("meta")) File.Delete("meta");

                        if (Directory.Exists("data")) Directory.Delete("data", true);

                        var process = GetMetadata(fileStream, si);
                        process.Start();
                        process.WaitForExit();

                        if (LoadNacpData(_cbRegionName,_icons,PB_GameIcon,TB_GameRev,TB_ProdCode)) break;
                    }
                    fileStream.Close();
                }
            }
            else
            {
                TB_Dev.Text = _keyhandler.MasterKey + " not found";
                TB_Name.Text = _keyhandler.MasterKey + " not found";
            }
        }


        private Process GetMetadata(FileStream fileStream, int si)
        {
            ReadMetadataFromStream(fileStream, si);

            return ReadWithHactool();
        }

        private static Process ReadWithHactool()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "hactool.exe",
                    Arguments = "-k keys.txt --romfsdir=data meta"
                }
            };
            return process;
        }

        private void ReadMetadataFromStream(FileStream fileStream, int si)
        {
            using (var fileStream2 = File.OpenWrite("meta"))
            {
                fileStream.Position = SecureOffset[si];
                var buffer = new byte[8192];
                var num = SecureSize[si];
                int num2;
                while ((num2 = fileStream.Read(buffer, 0, 8192)) > 0 && num > 0)
                {
                    fileStream2.Write(buffer, 0, num2);
                    num -= num2;
                }
                fileStream2.Close();
            }
        }
        private bool LoadNacpData(ComboBox _cbRegionName, Image[] _icons, PictureBox PB_GameIcon, TextBox TB_GameRev, TextBox TB_ProdCode)
        {
            if (File.Exists("data\\control.nacp"))
            {
                var source = File.ReadAllBytes("data\\control.nacp");
                Nacp.NacpDatas[0] = new Nacp.NacpData(source.Skip(0x3000).Take(0x1000).ToArray());
                for (var i = 0; i < Nacp.NacpStrings.Length; i++)
                {
                    Nacp.NacpStrings[i] =
                        new Nacp.NacpString(source.Skip(i * 0x300).Take(0x300).ToArray());
                    if (Nacp.NacpStrings[i].Check != 0)
                    {
                        _cbRegionName.Items.Add(Util.Language[i]);
                        var iconFilename = "data\\icon_" + Util.Language[i].Replace(" ", "") + ".dat";
                        if (File.Exists(iconFilename))
                            using (var original = new Bitmap(iconFilename))
                            {
                                _icons[i] = new Bitmap(original);
                                PB_GameIcon.BackgroundImage = _icons[i];
                            }
                    }
                }
                TB_GameRev.Text = Nacp.NacpDatas[0].GameVer;
                TB_ProdCode.Text = Nacp.NacpDatas[0].GameProd;
                if (TB_ProdCode.Text == "") TB_ProdCode.Text = "No Prod. ID";
                try
                {
                    File.Delete("meta");
                    Directory.Delete("data", true);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                _cbRegionName.SelectedIndex = 0;
                return true;
            }
            return false;
        }
    }
}