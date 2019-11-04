using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using XCI_Explorer.Helpers;
using XTSSharp;

namespace XCI_Explorer
{
    public partial class MainForm : Form
    {
        public List<char> chars = new List<char>();
        public byte[] NcaHeaderEncryptionKey1_Prod;
        public byte[] NcaHeaderEncryptionKey2_Prod;
        public string Mkey;
        public double UsedSize;
        private Image[] Icons = new Image[16];
        private string[] Language = new string[16] {
            "American English",
            "British English",
            "Japanese",
            "French",
            "German",
            "Latin American Spanish",
            "Spanish",
            "Italian",
            "Dutch",
            "Canadian French",
            "Portuguese",
            "Russian",
            "Korean",
            "Taiwanese",
            "Chinese",
            "???"
        };

        public MainForm()
        {
            InitializeComponent();

            this.Text = "XCI Explorer v" + getAssemblyVersion();

            LB_SelectedData.Text = "";
            LB_DataOffset.Text = "";
            LB_DataSize.Text = "";
            LB_HashedRegionSize.Text = "";
            LB_ActualHash.Text = "";
            LB_ExpectedHash.Text = "";

            //MAC - Set Current Directory to application directory so it can find the keys
            String startupPath = Application.StartupPath;
            Directory.SetCurrentDirectory(startupPath);

            if (!File.Exists("keys.txt"))
            {
                if (MessageBox.Show("keys.txt is missing.\nDo you want to automatically download it now?\n\nBy pressing 'Yes' you agree that you own these keys.\n", "XCI Explorer", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(Util.Base64Decode("aHR0cHM6Ly9wYXN0ZWJpbi5jb20vcmF3LzRieWY1NzFk"), "keys.txt");
                    }
                }

                if (!File.Exists("keys.txt"))
                {
                    MessageBox.Show("keys.txt failed to load.\nPlease include keys.txt in the root folder.");
                    Environment.Exit(0);
                }
            }

            if (!File.Exists($"tools{Path.DirectorySeparatorChar}hactool.exe"))
            {
                Directory.CreateDirectory("tools");
                MessageBox.Show("hactool.exe is missing.\nPlease include hactool.exe in the 'tools' folder.");
                Environment.Exit(0);
            }

            getKey();

            //MAC - Set the double clicked file name into the UI and process file
            String[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                TB_File.Text = args[1];
                Application.DoEvents();
                ProcessFile();
            }
        }

        private string getAssemblyVersion()
        {
            string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string[] versionArray = assemblyVersion.Split('.');

            assemblyVersion = string.Join(".", versionArray.Take(3));

            return assemblyVersion;
        }

        private void getKey()
        {
            string text = (from x in File.ReadAllLines("keys.txt")
                           select x.Split('=') into x
                           where x.Length > 1
                           select x).ToDictionary((string[] x) => x[0].Trim(), (string[] x) => x[1])["header_key"].Replace(" ", "");
            NcaHeaderEncryptionKey1_Prod = Util.StringToByteArray(text.Remove(32, 32));
            NcaHeaderEncryptionKey2_Prod = Util.StringToByteArray(text.Remove(0, 32));
        }

        public bool getMKey()
        {
            Dictionary<string, string> dictionary = (from x in File.ReadAllLines("keys.txt")
                                                     select x.Split('=') into x
                                                     where x.Length > 1
                                                     select x).ToDictionary((string[] x) => x[0].Trim(), (string[] x) => x[1]);
            Mkey = "master_key_";
            if (NCA.NCA_Headers[0].MasterKeyRev == 0 || NCA.NCA_Headers[0].MasterKeyRev == 1)
            {
                Mkey += "00";
            }
            else if (NCA.NCA_Headers[0].MasterKeyRev < 17)
            {
                int num = NCA.NCA_Headers[0].MasterKeyRev - 1;
                Mkey = Mkey + "0" + num.ToString();
            }
            else if (NCA.NCA_Headers[0].MasterKeyRev >= 17)
            {
                int num2 = NCA.NCA_Headers[0].MasterKeyRev - 1;
                Mkey += num2.ToString();
            }
            try
            {
                Mkey = dictionary[Mkey].Replace(" ", "");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ProcessFile()
        {
            // Code needs refactoring 
            LB_SelectedData.Text = "";
            LB_DataOffset.Text = "";
            LB_DataSize.Text = "";
            LB_HashedRegionSize.Text = "";
            LB_ExpectedHash.Text = "";
            LB_ActualHash.Text = "";
            B_Extract.Enabled = false;

            if (CheckNSP())
            {
                B_TrimXCI.Enabled = false;
                B_ExportCert.Enabled = false;
                B_ImportCert.Enabled = false;
                B_ViewCert.Enabled = false;
                B_ClearCert.Enabled = false;

                LoadNSP();
            }
            else if (CheckXCI())
            {
                B_TrimXCI.Enabled = true;
                B_ExportCert.Enabled = true;
                B_ImportCert.Enabled = true;
                B_ViewCert.Enabled = true;
                B_ClearCert.Enabled = true;

                LoadXCI();
            }
            else
            {
                TB_File.Text = null;
                MessageBox.Show("Unsupported file.");
            }
        }

        private void B_LoadROM_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Switch Game File (*.xci, *.nsp, *.nsz)|*.xci;*.nsp;*.nsz|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                TB_File.Text = openFileDialog.FileName;
                ProcessFile();
            }
        }

        private void LoadXCI()
        {
            string[] array = new string[5]
            {
                "B",
                "KB",
                "MB",
                "GB",
                "TB"
            };
            double num = (double)new FileInfo(TB_File.Text).Length;
            TB_ROMExactSize.Text = "(" + num.ToString() + " bytes)";
            int num2 = 0;
            while (num >= 1024.0 && num2 < array.Length - 1)
            {
                num2++;
                num /= 1024.0;
            }
            TB_ROMSize.Text = $"{num:0.##} {array[num2]}";
            double num3 = UsedSize = (double)(XCI.XCI_Headers[0].CardSize2 * 512 + 512);
            TB_ExactUsedSpace.Text = "(" + num3.ToString() + " bytes)";
            num2 = 0;
            while (num3 >= 1024.0 && num2 < array.Length - 1)
            {
                num2++;
                num3 /= 1024.0;
            }
            TB_UsedSpace.Text = $"{num3:0.##} {array[num2]}";
            TB_Capacity.Text = Util.GetCapacity(XCI.XCI_Headers[0].CardSize1);
            LoadPartitions();
            LoadNCAData();
            LoadGameInfos();
        }

        // Giba's better implementation (more native)
        public void LoadNSP()
        {
            CB_RegionName.Items.Clear();
            CB_RegionName.Enabled = true;
            TB_TID.Text = "";
            TB_Capacity.Text = "";
            TB_MKeyRev.Text = "";
            TB_SDKVer.Text = "";
            TB_GameRev.Text = "";
            TB_ProdCode.Text = "";
            TB_Name.Text = "";
            TB_Dev.Text = "";
            PB_GameIcon.BackgroundImage = null;
            Array.Clear(Icons, 0, Icons.Length);
            TV_Partitions.Nodes.Clear();
            FileInfo fi = new FileInfo(TB_File.Text);
            string contentType = "";

            // Maximum number of files in NSP to read in
            const int MAXFILES = 250;

            //Get File Size
            string[] array_fs = new string[5] { "B", "KB", "MB", "GB", "TB" };
            double num_fs = (double)fi.Length;
            int num2_fs = 0;
            TB_ROMExactSize.Text = "(" + num_fs.ToString() + " bytes)";
            TB_ExactUsedSpace.Text = TB_ROMExactSize.Text;

            while (num_fs >= 1024.0 && num2_fs < array_fs.Length - 1)
            {
                num2_fs++;
                num_fs /= 1024.0;
            }
            TB_ROMSize.Text = $"{num_fs:0.##} {array_fs[num2_fs]}";
            TB_UsedSpace.Text = TB_ROMSize.Text;

            Process process = new Process();
            try
            {
                FileStream fileStream = File.OpenRead(TB_File.Text);
                string ncaTarget = "";
                string xmlVersion = "";

                List<char> chars = new List<char>();
                byte[] array = new byte[16];
                byte[] array2 = new byte[24];
                fileStream.Read(array, 0, 16);
                PFS0.PFS0_Headers[0] = new PFS0.PFS0_Header(array);
                if (!PFS0.PFS0_Headers[0].Magic.Contains("PFS0"))
                {
                    return;
                }
                PFS0.PFS0_Entry[] array3;
                array3 = new PFS0.PFS0_Entry[Math.Max(PFS0.PFS0_Headers[0].FileCount, MAXFILES)]; //Dump of TitleID 01009AA000FAA000 reports more than 10000000 files here, so it breaks the program. Standard is to have only 20 files

                for (int m = 0; m < PFS0.PFS0_Headers[0].FileCount; m++)
                {
                    fileStream.Position = 16 + 24 * m;
                    fileStream.Read(array2, 0, 24);
                    array3[m] = new PFS0.PFS0_Entry(array2);

                    if (m == MAXFILES - 1) //Dump of TitleID 01009AA000FAA000 reports more than 10000000 files here, so it breaks the program. Standard is to have only 20 files
                    {
                        break;
                    }
                }
                for (int n = 0; n < PFS0.PFS0_Headers[0].FileCount; n++)
                {
                    fileStream.Position = 16 + 24 * PFS0.PFS0_Headers[0].FileCount + array3[n].Name_ptr;
                    int num4;
                    while ((num4 = fileStream.ReadByte()) != 0 && num4 != 0)
                    {
                        chars.Add((char)num4);
                    }
                    array3[n].Name = new string(chars.ToArray());
                    chars.Clear();

                    // Console.WriteLine("FC: " + PFS0.PFS0_Headers[0].FileCount.ToString() + " Name: " + array3[n].Name);

                    if (array3[n].Name.EndsWith(".cnmt.xml"))
                    {
                        byte[] array4 = new byte[array3[n].Size];
                        fileStream.Position = 16 + 24 * PFS0.PFS0_Headers[0].FileCount + PFS0.PFS0_Headers[0].StringTableSize + array3[n].Offset;
                        fileStream.Read(array4, 0, (int)array3[n].Size);

                        XDocument xml = XDocument.Parse(Encoding.UTF8.GetString(array4));
                        TB_TID.Text = xml.Element("ContentMeta").Element("Id").Value.Remove(1, 2).ToUpper();
                        contentType = xml.Element("ContentMeta").Element("Type").Value;
                        if (contentType == "Patch")
                            xmlVersion = "v" + xml.Element("ContentMeta").Element("Version").Value;

                        /*string titleIDBaseGame = TB_TID.Text;
                        if (contentType != "Application") {
                            string titleIdBase = TB_TID.Text.Substring(0, 13);
                            if (contentType == "Patch") //UPDATE
                            {
                                titleIDBaseGame = titleIdBase + "000";
                            }
                            else //DLC
                            {
                                long tmp = long.Parse(titleIdBase, System.Globalization.NumberStyles.HexNumber) - 1;
                                titleIDBaseGame = string.Format("0{0:X8}", tmp) + "000";
                            }
                        }*/
                        //data.TitleIDBaseGame = titleIDBaseGame;

                        if (contentType != "AddOnContent")
                        {
                            foreach (XElement xe in xml.Descendants("Content"))
                            {
                                if (xe.Element("Type").Value != "Control")
                                {
                                    continue;
                                }

                                ncaTarget = xe.Element("Id").Value + ".nca";
                                break;
                            }
                        }
                        else //This is a DLC
                        {
                            foreach (XElement xe in xml.Descendants("Content"))
                            {
                                if (xe.Element("Type").Value != "Meta")
                                {
                                    continue;
                                }

                                ncaTarget = xe.Element("Id").Value + ".cnmt.nca";
                                break;
                            }
                        }
                    }

                    if (n == MAXFILES - 1) //Dump of TitleID 01009AA000FAA000 reports more than 10000000 files here, so it breaks the program. Standard is to have only 20 files
                    {
                        break;
                    }
                }

                if (String.IsNullOrEmpty(ncaTarget))
                {
                    //Missing content metadata xml. Read from content metadata nca instead
                    for (int n = 0; n < PFS0.PFS0_Headers[0].FileCount; n++)
                    {
                        if (array3[n].Name.EndsWith(".cnmt.nca"))
                        {
                            try
                            {
                                File.Delete("meta");
                                Directory.Delete("data", true);
                            }
                            catch { }

                            using (FileStream fileStream2 = File.OpenWrite("meta"))
                            {
                                fileStream.Position = 16 + 24 * PFS0.PFS0_Headers[0].FileCount + PFS0.PFS0_Headers[0].StringTableSize + array3[n].Offset;
                                byte[] buffer = new byte[8192];
                                long num = array3[n].Size;
                                int num4;
                                while ((num4 = fileStream.Read(buffer, 0, 8192)) > 0 && num > 0)
                                {
                                    fileStream2.Write(buffer, 0, num4);
                                    num -= num4;
                                }
                                fileStream2.Close();
                            }

                            process = new Process();
                            process.StartInfo = new ProcessStartInfo
                            {
                                WindowStyle = ProcessWindowStyle.Hidden,
                                FileName = $"tools{Path.DirectorySeparatorChar}hactool.exe",
                                Arguments = "-k keys.txt --section0dir=data meta",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                CreateNoWindow = true
                            };
                            process.Start();

                            string masterkey = "";
                            while (!process.StandardOutput.EndOfStream)
                            {
                                string output = process.StandardOutput.ReadLine();
                                if (output.StartsWith("Master Key Revision"))
                                {
                                    masterkey = Regex.Replace(output, @"\s+", " ");
                                }
                            }
                            process.WaitForExit();

                            if (!Directory.Exists("data"))
                            {
                                MessageBox.Show(masterkey + " is missing!");
                            }
                            else
                            {
                                try
                                {
                                    string[] cnmt = Directory.GetFiles("data", "*.cnmt");
                                    if (cnmt.Length != 0)
                                    {
                                        using (FileStream fileStream3 = File.OpenRead(cnmt[0]))
                                        {
                                            byte[] buffer = new byte[32];
                                            byte[] buffer2 = new byte[56];
                                            CNMT.CNMT_Header[] array7 = new CNMT.CNMT_Header[1];

                                            fileStream3.Read(buffer, 0, 32);
                                            array7[0] = new CNMT.CNMT_Header(buffer);

                                            byte[] TitleID = BitConverter.GetBytes(array7[0].TitleID);
                                            Array.Reverse(TitleID);
                                            TB_TID.Text = BitConverter.ToString(TitleID).Replace("-", "");
                                            xmlVersion = "v" + array7[0].TitleVersion.ToString();

                                            if (array7[0].Type == (byte)CNMT.CNMT_Header.TitleType.REGULAR_APPLICATION)
                                            {
                                                contentType = "Application";
                                            }
                                            else if (array7[0].Type == (byte)CNMT.CNMT_Header.TitleType.UPDATE_TITLE)
                                            {
                                                contentType = "Patch";
                                            }
                                            else if (array7[0].Type == (byte)CNMT.CNMT_Header.TitleType.ADD_ON_CONTENT)
                                            {
                                                contentType = "AddOnContent";
                                            }

                                            fileStream3.Position = array7[0].Offset + 32;
                                            CNMT.CNMT_Entry[] array9 = new CNMT.CNMT_Entry[array7[0].ContentCount];
                                            for (int k = 0; k < array7[0].ContentCount; k++)
                                            {
                                                fileStream3.Read(buffer2, 0, 56);
                                                array9[k] = new CNMT.CNMT_Entry(buffer2);
                                                if (array9[k].Type == (byte)CNMT.CNMT_Entry.ContentType.CONTROL || array9[k].Type == (byte)CNMT.CNMT_Entry.ContentType.DATA)
                                                {
                                                    ncaTarget = BitConverter.ToString(array9[k].NcaId).ToLower().Replace("-", "") + ".nca";
                                                    break;
                                                }
                                            }

                                            fileStream3.Close();
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }

                for (int n = 0; n < PFS0.PFS0_Headers[0].FileCount; n++)
                {
                    if (array3[n].Name.Equals(ncaTarget))
                    {
                        Directory.CreateDirectory("tmp");

                        byte[] array5 = new byte[64 * 1024];
                        fileStream.Position = 16 + 24 * PFS0.PFS0_Headers[0].FileCount + PFS0.PFS0_Headers[0].StringTableSize + array3[n].Offset;

                        using (Stream output = File.Create($"tmp{Path.DirectorySeparatorChar}" + ncaTarget))
                        {
                            long Size = array3[n].Size;
                            int result = 0;
                            while ((result = fileStream.Read(array5, 0, (int)Math.Min(array5.Length, Size))) > 0)
                            {
                                output.Write(array5, 0, result);
                                Size -= result;
                            }
                        }

                        break;
                    }

                    if (n == MAXFILES - 1) //Dump of TitleID 01009AA000FAA000 reports more than 10000000 files here, so it breaks the program. Standard is to have only 20 files
                    {
                        break;
                    }
                }

                fileStream.Close();

                if (contentType != "AddOnContent")
                {
                    process = new Process();
                    process.StartInfo = new ProcessStartInfo
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = $"tools{Path.DirectorySeparatorChar}hactool.exe",
                        Arguments = "-k keys.txt --romfsdir=tmp tmp/" + ncaTarget
                    };

                    process.Start();
                    process.WaitForExit();
                    process.Close();
                    byte[] flux = new byte[200];

                    try
                    {
                        byte[] source = File.ReadAllBytes($"tmp{Path.DirectorySeparatorChar}control.nacp");
                        NACP.NACP_Datas[0] = new NACP.NACP_Data(source.Skip(0x3000).Take(0x1000).ToArray());

                        for (int i = 0; i < NACP.NACP_Strings.Length; i++)
                        {
                            NACP.NACP_Strings[i] = new NACP.NACP_String(source.Skip(i * 0x300).Take(0x300).ToArray());

                            if (NACP.NACP_Strings[i].Check != 0)
                            {
                                CB_RegionName.Items.Add(Language[i]);
                                string icon_filename = $"tmp{Path.DirectorySeparatorChar}icon_" + Language[i].Replace(" ", "") + ".dat";
                                if (File.Exists(icon_filename))
                                {
                                    using (Bitmap original = new Bitmap(icon_filename))
                                    {
                                        Icons[i] = new Bitmap(original);
                                        PB_GameIcon.BackgroundImage = Icons[i];
                                    }
                                }
                            }
                        }
                        if (xmlVersion.Trim() == "")
                        {
                            TB_GameRev.Text = NACP.NACP_Datas[0].GameVer.Replace("\0", "");
                        }
                        else
                        {
                            TB_GameRev.Text = NACP.NACP_Datas[0].GameVer.Replace("\0", "") + " (" + xmlVersion + ")";
                        }
                        TB_ProdCode.Text = NACP.NACP_Datas[0].GameProd.Replace("\0", "");
                        if (TB_ProdCode.Text == "")
                        {
                            TB_ProdCode.Text = "No Prod. ID";
                        }

                        for (int z = 0; z < NACP.NACP_Strings.Length; z++)
                        {
                            if (NACP.NACP_Strings[z].GameName.Replace("\0", "") != "")
                            {
                                TB_Name.Text = NACP.NACP_Strings[z].GameName.Replace("\0", "");
                                break;
                            }
                        }
                        for (int z = 0; z < NACP.NACP_Strings.Length; z++)
                        {
                            if (NACP.NACP_Strings[z].GameAuthor.Replace("\0", "") != "")
                            {
                                TB_Dev.Text = NACP.NACP_Strings[z].GameAuthor.Replace("\0", "");
                                break;
                            }
                        }
                    }
                    catch { }

                    /*if (contentType == "Patch") {
                    }*/
                }
                else
                {
                    TB_GameRev.Text = "";
                    TB_ProdCode.Text = "No Prod. ID";
                }

                // Lets get SDK Version, Distribution Type and Masterkey revision
                // This is far from the best aproach, but it's what we have for now
                process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = $"tools{Path.DirectorySeparatorChar}hactool.exe",
                    Arguments = "-k keys.txt tmp/" + ncaTarget,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                process.Start();
                StreamReader sr = process.StandardOutput;

                while (sr.Peek() >= 0)
                {
                    string str;
                    string[] strArray;
                    str = sr.ReadLine();
                    strArray = str.Split(':');
                    if (strArray[0] == "SDK Version")
                    {
                        TB_SDKVer.Text = strArray[1].Trim();
                    }
                    else if (strArray[0] == "Master Key Revision")
                    {
                        string MasterKey = strArray[1].Trim();
                        if (MasterKey.Contains("Unknown"))
                        {
                            int keyblob;
                            if (int.TryParse(new string(MasterKey.TakeWhile(Char.IsDigit).ToArray()), out keyblob))
                            {
                                MasterKey = Util.GetMkey((byte)(keyblob + 1)).Replace("MasterKey", "");
                            }
                        }
                        TB_MKeyRev.Text = "MasterKey" + MasterKey;
                        break;
                    }
                }
                process.WaitForExit();
                process.Close();
            }
            catch { }
            if (Directory.Exists("tmp"))
            {
                Directory.Delete("tmp", true);
            }

            TB_Capacity.Text = "eShop";

            if (TB_Name.Text.Trim() != "")
            {
                CB_RegionName.SelectedIndex = 0;
            }
        }

        private void LoadGameInfos()
        {
            CB_RegionName.Items.Clear();
            CB_RegionName.Enabled = true;
            TB_Name.Text = "";
            TB_Dev.Text = "";
            PB_GameIcon.BackgroundImage = null;
            Array.Clear(Icons, 0, Icons.Length);
            if (getMKey())
            {
                using (FileStream fileStream = File.OpenRead(TB_File.Text))
                {
                    List<string> ncaTarget = new List<string>();
                    string GameRevision = "";

                    for (int si = 0; si < SecureSize.Length; si++)
                    {
                        if (SecureSize[si] > 0x4E20000) continue;

                        if (SecureName[si].EndsWith(".cnmt.nca"))
                        {
                            if (File.Exists("meta"))
                            {
                                File.Delete("meta");
                            }

                            if (Directory.Exists("data"))
                            {
                                Directory.Delete("data", true);
                            }

                            using (FileStream fileStream2 = File.OpenWrite("meta"))
                            {
                                fileStream.Position = SecureOffset[si];
                                byte[] buffer = new byte[8192];
                                long num = SecureSize[si];
                                int num4;
                                while ((num4 = fileStream.Read(buffer, 0, 8192)) > 0 && num > 0)
                                {
                                    fileStream2.Write(buffer, 0, num4);
                                    num -= num4;
                                }
                                fileStream2.Close();
                            }

                            Process process = new Process();
                            process.StartInfo = new ProcessStartInfo
                            {
                                WindowStyle = ProcessWindowStyle.Hidden,
                                FileName = $"tools{Path.DirectorySeparatorChar}hactool.exe",
                                Arguments = "-k keys.txt --section0dir=data meta"
                            };
                            process.Start();
                            process.WaitForExit();

                            string[] cnmt = Directory.GetFiles("data", "*.cnmt");
                            if (cnmt.Length != 0)
                            {
                                using (FileStream fileStream3 = File.OpenRead(cnmt[0]))
                                {
                                    byte[] buffer = new byte[32];
                                    byte[] buffer2 = new byte[56];
                                    CNMT.CNMT_Header[] array7 = new CNMT.CNMT_Header[1];

                                    fileStream3.Read(buffer, 0, 32);
                                    array7[0] = new CNMT.CNMT_Header(buffer);

                                    fileStream3.Position = array7[0].Offset + 32;
                                    CNMT.CNMT_Entry[] array9 = new CNMT.CNMT_Entry[array7[0].ContentCount];
                                    for (int k = 0; k < array7[0].ContentCount; k++)
                                    {
                                        fileStream3.Read(buffer2, 0, 56);
                                        array9[k] = new CNMT.CNMT_Entry(buffer2);
                                        if (array9[k].Type == (byte)CNMT.CNMT_Entry.ContentType.CONTROL)
                                        {
                                            ncaTarget.Add(BitConverter.ToString(array9[k].NcaId).ToLower().Replace("-", "") + ".nca");
                                            break;
                                        }
                                    }

                                    fileStream3.Close();
                                }
                            }
                        }
                    }

                    for (int si = 0; si < SecureSize.Length; si++)
                    {
                        if (SecureSize[si] > 0x4E20000) continue;

                        if (ncaTarget.Contains(SecureName[si]))
                        {
                            if (File.Exists("meta"))
                            {
                                File.Delete("meta");
                            }

                            if (Directory.Exists("data"))
                            {
                                Directory.Delete("data", true);
                            }

                            using (FileStream fileStream2 = File.OpenWrite("meta"))
                            {
                                fileStream.Position = SecureOffset[si];
                                byte[] buffer = new byte[8192];
                                long num = SecureSize[si];
                                int num2;
                                while ((num2 = fileStream.Read(buffer, 0, 8192)) > 0 && num > 0)
                                {
                                    fileStream2.Write(buffer, 0, num2);
                                    num -= num2;
                                }
                                fileStream2.Close();
                            }

                            Process process = new Process();
                            process.StartInfo = new ProcessStartInfo
                            {
                                WindowStyle = ProcessWindowStyle.Hidden,
                                FileName = $"tools{Path.DirectorySeparatorChar}hactool.exe",
                                Arguments = "-k keys.txt --romfsdir=data meta"
                            };
                            process.Start();
                            process.WaitForExit();

                            if (File.Exists($"data{Path.DirectorySeparatorChar}control.nacp"))
                            {
                                byte[] source = File.ReadAllBytes($"data{Path.DirectorySeparatorChar}control.nacp");
                                NACP.NACP_Datas[0] = new NACP.NACP_Data(source.Skip(0x3000).Take(0x1000).ToArray());

                                string GameVer = NACP.NACP_Datas[0].GameVer.Replace("\0", "");
                                Version version1, version2;
                                if (!Version.TryParse(Regex.Replace(GameRevision, @"[^\d.].*$", ""), out version1))
                                {
                                    version1 = new Version();
                                }
                                if (!Version.TryParse(Regex.Replace(GameVer, @"[^\d.].*$", ""), out version2))
                                {
                                    version2 = new Version();
                                }
                                if (version2.CompareTo(version1) > 0)
                                {
                                    GameRevision = GameVer;

                                    for (int i = 0; i < NACP.NACP_Strings.Length; i++)
                                    {
                                        NACP.NACP_Strings[i] = new NACP.NACP_String(source.Skip(i * 0x300).Take(0x300).ToArray());
                                        if (NACP.NACP_Strings[i].Check != 0 && !CB_RegionName.Items.Contains(Language[i]))
                                        {
                                            CB_RegionName.Items.Add(Language[i]);
                                            string icon_filename = $"data{Path.DirectorySeparatorChar}icon_" + Language[i].Replace(" ", "") + ".dat";
                                            if (File.Exists(icon_filename))
                                            {
                                                using (Bitmap original = new Bitmap(icon_filename))
                                                {
                                                    Icons[i] = new Bitmap(original);
                                                    PB_GameIcon.BackgroundImage = Icons[i];
                                                }
                                            }
                                        }
                                    }
                                    TB_ProdCode.Text = NACP.NACP_Datas[0].GameProd;
                                    if (TB_ProdCode.Text == "")
                                    {
                                        TB_ProdCode.Text = "No Prod. ID";
                                    }
                                    try
                                    {
                                        File.Delete("meta");
                                        Directory.Delete("data", true);
                                    }
                                    catch { }
                                }
                            }
                        }
                    }

                    TB_GameRev.Text = GameRevision;
                    CB_RegionName.SelectedIndex = 0;

                    fileStream.Close();
                }
            }
            else
            {
                TB_Dev.Text = Mkey + " not found";
                TB_Name.Text = Mkey + " not found";
            }
        }

        private void LoadNCAData()
        {
            NCA.NCA_Headers[0] = new NCA.NCA_Header(DecryptNCAHeader(gameNcaOffset));
            TB_TID.Text = "0" + NCA.NCA_Headers[0].TitleID.ToString("X");
            TB_SDKVer.Text = $"{NCA.NCA_Headers[0].SDKVersion4}.{NCA.NCA_Headers[0].SDKVersion3}.{NCA.NCA_Headers[0].SDKVersion2}.{NCA.NCA_Headers[0].SDKVersion1}";
            TB_MKeyRev.Text = Util.GetMkey(NCA.NCA_Headers[0].MasterKeyRev);
        }

        //https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2 + 2);
            hex.Append("0x");
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static string SHA256Bytes(byte[] ba)
        {
            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] hashValue;
            hashValue = mySHA256.ComputeHash(ba);
            return ByteArrayToString(hashValue);
        }

        private void LoadPartitions()
        {
            string actualHash;
            byte[] hashBuffer;
            long offset;

            TV_Partitions.Nodes.Clear();
            TV_Parti = new TreeViewFileSystem(TV_Partitions);
            rootNode = new BetterTreeNode("root");
            rootNode.Offset = -1L;
            rootNode.Size = -1L;
            TV_Partitions.Nodes.Add(rootNode);
            bool LogoPartition = false;
            FileStream fileStream = new FileStream(TB_File.Text, FileMode.Open, FileAccess.Read);
            HFS0.HSF0_Entry[] array = new HFS0.HSF0_Entry[HFS0.HFS0_Headers[0].FileCount];
            fileStream.Position = XCI.XCI_Headers[0].HFS0OffsetPartition + 16 + 64 * HFS0.HFS0_Headers[0].FileCount;
            long num = XCI.XCI_Headers[0].HFS0OffsetPartition + XCI.XCI_Headers[0].HFS0SizeParition;
            byte[] array2 = new byte[64];
            byte[] array3 = new byte[16];
            byte[] array4 = new byte[24];
            for (int i = 0; i < HFS0.HFS0_Headers[0].FileCount; i++)
            {
                fileStream.Position = XCI.XCI_Headers[0].HFS0OffsetPartition + 16 + 64 * i;
                fileStream.Read(array2, 0, 64);
                array[i] = new HFS0.HSF0_Entry(array2);
                fileStream.Position = XCI.XCI_Headers[0].HFS0OffsetPartition + 16 + 64 * HFS0.HFS0_Headers[0].FileCount + array[i].Name_ptr;
                int num2;
                while ((num2 = fileStream.ReadByte()) != 0 && num2 != 0)
                {
                    chars.Add((char)num2);
                }
                array[i].Name = new string(chars.ToArray());
                chars.Clear();
                offset = num + array[i].Offset;
                hashBuffer = new byte[array[i].HashedRegionSize];
                fileStream.Position = offset;
                fileStream.Read(hashBuffer, 0, array[i].HashedRegionSize);
                actualHash = SHA256Bytes(hashBuffer);

                TV_Parti.AddFile(array[i].Name + ".hfs0", rootNode, offset, array[i].Size, array[i].HashedRegionSize, ByteArrayToString(array[i].Hash), actualHash);
                BetterTreeNode betterTreeNode = TV_Parti.AddDir(array[i].Name, rootNode);
                HFS0.HFS0_Header[] array5 = new HFS0.HFS0_Header[1];
                fileStream.Position = array[i].Offset + num;
                fileStream.Read(array3, 0, 16);
                array5[0] = new HFS0.HFS0_Header(array3);
                if (array[i].Name == "secure")
                {
                    SecureSize = new long[array5[0].FileCount];
                    SecureOffset = new long[array5[0].FileCount];
                    SecureName = new string[array5[0].FileCount];
                }
                if (array[i].Name == "normal")
                {
                    NormalSize = new long[array5[0].FileCount];
                    NormalOffset = new long[array5[0].FileCount];
                }
                if (array[i].Name == "logo")
                {
                    if (array5[0].FileCount > 0)
                    {
                        LogoPartition = true;
                    }
                }
                HFS0.HSF0_Entry[] array6 = new HFS0.HSF0_Entry[array5[0].FileCount];
                for (int j = 0; j < array5[0].FileCount; j++)
                {
                    fileStream.Position = array[i].Offset + num + 16 + 64 * j;
                    fileStream.Read(array2, 0, 64);
                    array6[j] = new HFS0.HSF0_Entry(array2);
                    fileStream.Position = array[i].Offset + num + 16 + 64 * array5[0].FileCount + array6[j].Name_ptr;
                    while ((num2 = fileStream.ReadByte()) != 0 && num2 != 0)
                    {
                        chars.Add((char)num2);
                    }
                    array6[j].Name = new string(chars.ToArray());
                    chars.Clear();
                    if (array[i].Name == "secure")
                    {
                        SecureSize[j] = array6[j].Size;
                        SecureOffset[j] = array[i].Offset + array6[j].Offset + num + 16 + array5[0].StringTableSize + array5[0].FileCount * 64;
                        SecureName[j] = array6[j].Name;
                    }
                    if (array[i].Name == "normal")
                    {
                        NormalSize[j] = array6[j].Size;
                        NormalOffset[j] = array[i].Offset + array6[j].Offset + num + 16 + array5[0].StringTableSize + array5[0].FileCount * 64;
                    }
                    offset = array[i].Offset + array6[j].Offset + num + 16 + array5[0].StringTableSize + array5[0].FileCount * 64;
                    hashBuffer = new byte[array6[j].HashedRegionSize];
                    fileStream.Position = offset;
                    fileStream.Read(hashBuffer, 0, array6[j].HashedRegionSize);
                    actualHash = SHA256Bytes(hashBuffer);

                    TV_Parti.AddFile(array6[j].Name, betterTreeNode, offset, array6[j].Size, array6[j].HashedRegionSize, ByteArrayToString(array6[j].Hash), actualHash);
                    TreeNode[] array7 = TV_Partitions.Nodes.Find(betterTreeNode.Text, true);
                    if (array7.Length != 0)
                    {
                        TV_Parti.AddFile(array6[j].Name, (BetterTreeNode)array7[0], 0L, 0L);
                    }
                }
            }
            long num3 = -9223372036854775808L;
            for (int k = 0; k < SecureSize.Length; k++)
            {
                if (SecureSize[k] > num3)
                {
                    gameNcaSize = SecureSize[k];
                    gameNcaOffset = SecureOffset[k];
                    num3 = SecureSize[k];
                }
            }
            PFS0Offset = gameNcaOffset + 32768;
            fileStream.Position = PFS0Offset;
            fileStream.Read(array3, 0, 16);
            PFS0.PFS0_Headers[0] = new PFS0.PFS0_Header(array3);
            if (PFS0.PFS0_Headers[0].FileCount == 2 || !LogoPartition)
            {
                PFS0.PFS0_Entry[] array8;
                try
                {
                    array8 = new PFS0.PFS0_Entry[PFS0.PFS0_Headers[0].FileCount];
                }
                catch (Exception ex)
                {
                    array8 = new PFS0.PFS0_Entry[0];
                    Debug.WriteLine("Partitions Error: " + ex.Message);
                }
                for (int m = 0; m < PFS0.PFS0_Headers[0].FileCount; m++)
                {
                    fileStream.Position = PFS0Offset + 16 + 24 * m;
                    fileStream.Read(array4, 0, 24);
                    array8[m] = new PFS0.PFS0_Entry(array4);
                    PFS0Size += array8[m].Size;
                }
                TV_Parti.AddFile("boot.psf0", rootNode, PFS0Offset, 16 + 24 * PFS0.PFS0_Headers[0].FileCount + 64 + PFS0Size);
                BetterTreeNode betterTreeNode2 = TV_Parti.AddDir("boot", rootNode);
                for (int n = 0; n < PFS0.PFS0_Headers[0].FileCount; n++)
                {
                    fileStream.Position = PFS0Offset + 16 + 24 * PFS0.PFS0_Headers[0].FileCount + array8[n].Name_ptr;
                    int num4;
                    while ((num4 = fileStream.ReadByte()) != 0 && num4 != 0)
                    {
                        chars.Add((char)num4);
                    }
                    array8[n].Name = new string(chars.ToArray());
                    chars.Clear();
                    TV_Parti.AddFile(array8[n].Name, betterTreeNode2, PFS0Offset + array8[n].Offset + 16 + PFS0.PFS0_Headers[0].StringTableSize + PFS0.PFS0_Headers[0].FileCount * 24, array8[n].Size);
                    TreeNode[] array9 = TV_Partitions.Nodes.Find(betterTreeNode2.Text, true);
                    if (array9.Length != 0)
                    {
                        TV_Parti.AddFile(array8[n].Name, (BetterTreeNode)array9[0], 0L, 0L);
                    }
                }
            }
            fileStream.Close();
        }

        private void TV_Partitions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            BetterTreeNode betterTreeNode = (BetterTreeNode)TV_Partitions.SelectedNode;
            if (betterTreeNode.Offset != -1)
            {
                selectedOffset = betterTreeNode.Offset;
                selectedSize = betterTreeNode.Size;
                string expectedHash = betterTreeNode.ExpectedHash;
                string actualHash = betterTreeNode.ActualHash;
                long HashedRegionSize = betterTreeNode.HashedRegionSize;

                LB_DataOffset.Text = "Offset: 0x" + selectedOffset.ToString("X");
                LB_SelectedData.Text = e.Node.Text;
                if (backgroundWorker1.IsBusy != true)
                {
                    B_Extract.Enabled = true;
                }
                string[] array = new string[5]
                {
                    "B",
                    "KB",
                    "MB",
                    "GB",
                    "TB"
                };
                double num = (double)selectedSize;
                int num2 = 0;
                while (num >= 1024.0 && num2 < array.Length - 1)
                {
                    num2++;
                    num /= 1024.0;
                }
                LB_DataSize.Text = "Size:   0x" + selectedSize.ToString("X") + " (" + num.ToString() + array[num2] + ")";

                if (HashedRegionSize != 0)
                {
                    LB_HashedRegionSize.Text = "HashedRegionSize: 0x" + HashedRegionSize.ToString("X");
                }
                else
                {
                    LB_HashedRegionSize.Text = "";
                }

                if (!string.IsNullOrEmpty(expectedHash))
                {
                    LB_ExpectedHash.Text = "Header Hash: " + expectedHash.Substring(0, 32);
                }
                else
                {
                    LB_ExpectedHash.Text = "";
                }

                if (!string.IsNullOrEmpty(actualHash))
                {
                    LB_ActualHash.Text = "Actual Hash: " + actualHash.Substring(0, 32);
                    if (actualHash == expectedHash)
                    {
                        LB_ActualHash.ForeColor = System.Drawing.Color.Green;
                    }
                    else
                    {
                        LB_ActualHash.ForeColor = System.Drawing.Color.Red;
                    }
                }
                else
                {
                    LB_ActualHash.Text = "";
                }

            }
            else
            {
                LB_SelectedData.Text = "";
                LB_DataOffset.Text = "";
                LB_DataSize.Text = "";
                LB_HashedRegionSize.Text = "";
                LB_ExpectedHash.Text = "";
                LB_ActualHash.Text = "";
                B_Extract.Enabled = false;
            }
        }

        public bool CheckXCI()
        {
            FileStream fileStream = new FileStream(TB_File.Text, FileMode.Open, FileAccess.Read);
            byte[] array = new byte[61440];
            byte[] array2 = new byte[16];
            fileStream.Read(array, 0, 61440);
            XCI.XCI_Headers[0] = new XCI.XCI_Header(array);
            if (!XCI.XCI_Headers[0].Magic.Contains("HEAD"))
            {
                return false;
            }
            fileStream.Position = XCI.XCI_Headers[0].HFS0OffsetPartition;
            fileStream.Read(array2, 0, 16);
            HFS0.HFS0_Headers[0] = new HFS0.HFS0_Header(array2);
            fileStream.Close();
            return true;
        }

        public bool CheckNSP()
        {
            FileStream fileStream = File.OpenRead(TB_File.Text);
            byte[] array = new byte[16];
            fileStream.Read(array, 0, 16);
            PFS0.PFS0_Headers[0] = new PFS0.PFS0_Header(array);
            fileStream.Close();
            if (!PFS0.PFS0_Headers[0].Magic.Contains("PFS0"))
            {
                return false;
            }
            return true;
        }

        private void B_ExportCert_Click(object sender, EventArgs e)
        {
            if (Util.checkFile(TB_File.Text))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "gamecard_cert.dat (*.dat)|*.dat";
                saveFileDialog.FileName = Path.GetFileName("gamecard_cert.dat");
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    FileStream fileStream = new FileStream(TB_File.Text, FileMode.Open, FileAccess.Read);
                    byte[] array = new byte[512];
                    fileStream.Position = 28672L;
                    fileStream.Read(array, 0, 512);
                    File.WriteAllBytes(saveFileDialog.FileName, array);
                    fileStream.Close();
                    MessageBox.Show("cert successfully exported to:\n\n" + saveFileDialog.FileName);
                }
            }
            else
            {
                MessageBox.Show("File not found");
            }
        }

        private void B_ImportCert_Click(object sender, EventArgs e)
        {
            if (Util.checkFile(TB_File.Text))
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "gamecard_cert (*.dat)|*.dat|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK && new FileInfo(openFileDialog.FileName).Length == 512)
                {
                    using (Stream stream = File.Open(TB_File.Text, FileMode.Open))
                    {
                        stream.Position = 28672L;
                        stream.Write(File.ReadAllBytes(openFileDialog.FileName), 0, 512);
                    }
                    MessageBox.Show("Cert successfully imported from:\n\n" + openFileDialog.FileName);
                }
            }
            else
            {
                MessageBox.Show("File not found");
            }
        }

        private void B_ViewCert_Click(object sender, EventArgs e)
        {
            if (Util.checkFile(TB_File.Text))
            {
                CertForm cert = new CertForm(this);
                cert.Text = "Cert Data - " + TB_File.Text;
                cert.Show();
            }
            else
            {
                MessageBox.Show("File not found");
            }
        }

        private void B_ClearCert_Click(object sender, EventArgs e)
        {
            if (Util.checkFile(TB_File.Text))
            {
                if (MessageBox.Show("The cert will be deleted permanently.\nContinue?", "XCI Explorer", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    using (Stream stream = File.Open(TB_File.Text, FileMode.Open))
                    {
                        byte[] array = new byte[512];
                        for (int i = 0; i < array.Length; i++)
                        {
                            array[i] = byte.MaxValue;
                        }
                        stream.Position = 28672L;
                        stream.Write(array, 0, array.Length);
                        MessageBox.Show("Cert deleted.");
                    }
                }
            }
            else
            {
                MessageBox.Show("File not found");
            }
        }

        private void B_Extract_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = LB_SelectedData.Text;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (backgroundWorker1.IsBusy != true)
                {
                    B_Extract.Enabled = false;
                    B_LoadROM.Enabled = false;
                    B_TrimXCI.Enabled = false;
                    B_ImportCert.Enabled = false;
                    B_ClearCert.Enabled = false;

                    // Start the asynchronous operation.
                    backgroundWorker1.RunWorkerAsync(saveFileDialog.FileName);

                    MessageBox.Show("Extracting NCA\nPlease wait...");
                }
            }
        }

        public byte[] DecryptNCAHeader(long offset)
        {
            byte[] array = new byte[3072];
            if (File.Exists(TB_File.Text))
            {
                FileStream fileStream = new FileStream(TB_File.Text, FileMode.Open, FileAccess.Read);
                fileStream.Position = offset;
                fileStream.Read(array, 0, 3072);
                File.WriteAllBytes(TB_File.Text + ".tmp", array);
                Xts xts = XtsAes128.Create(NcaHeaderEncryptionKey1_Prod, NcaHeaderEncryptionKey2_Prod);
                using (BinaryReader binaryReader = new BinaryReader(File.OpenRead(TB_File.Text + ".tmp")))
                {
                    using (XtsStream xtsStream = new XtsStream(binaryReader.BaseStream, xts, 512))
                    {
                        xtsStream.Read(array, 0, 3072);
                    }
                }
                File.Delete(TB_File.Text + ".tmp");
                fileStream.Close();
            }
            return array;
        }

        private void CB_RegionName_SelectedIndexChanged(object sender, EventArgs e)
        {
            int num = Array.FindIndex(Language, (string element) => element.StartsWith(CB_RegionName.Text, StringComparison.Ordinal));
            // Icons for 1-2 Switch in some languages are "missing"
            // This just shows the first real icon instead of a blank
            if (Icons[num] != null)
            {
                PB_GameIcon.BackgroundImage = Icons[num];
            }
            else
            {
                for (int i = 0; i < CB_RegionName.Items.Count; i++)
                {
                    if (Icons[i] != null)
                    {
                        PB_GameIcon.BackgroundImage = Icons[i];
                        break;
                    }
                }
            }
            TB_Name.Text = NACP.NACP_Strings[num].GameName;
            TB_Dev.Text = NACP.NACP_Strings[num].GameAuthor;
        }

        private void B_TrimXCI_Click(object sender, EventArgs e)
        {
            if (Util.checkFile(TB_File.Text))
            {
                if (MessageBox.Show("Trim XCI?", "XCI Explorer", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (!TB_ROMExactSize.Text.Equals(TB_ExactUsedSpace.Text))
                    {
                        FileStream fileStream = new FileStream(TB_File.Text, FileMode.Open, FileAccess.Write);
                        fileStream.SetLength((long)UsedSize);
                        fileStream.Close();
                        MessageBox.Show("Done.");
                        string[] array = new string[5]
                        {
                            "B",
                            "KB",
                            "MB",
                            "GB",
                            "TB"
                        };
                        double num = (double)new FileInfo(TB_File.Text).Length;
                        TB_ROMExactSize.Text = "(" + num.ToString() + " bytes)";
                        int num2 = 0;
                        while (num >= 1024.0 && num2 < array.Length - 1)
                        {
                            num2++;
                            num /= 1024.0;
                        }
                        TB_ROMSize.Text = $"{num:0.##} {array[num2]}";
                        double num3 = UsedSize = (double)(XCI.XCI_Headers[0].CardSize2 * 512 + 512);
                        TB_ExactUsedSpace.Text = "(" + num3.ToString() + " bytes)";
                        num2 = 0;
                        while (num3 >= 1024.0 && num2 < array.Length - 1)
                        {
                            num2++;
                            num3 /= 1024.0;
                        }
                        TB_UsedSpace.Text = $"{num3:0.##} {array[num2]}";
                    }
                    else
                    {
                        MessageBox.Show("No trimming needed!");
                    }
                }
            }
            else
            {
                MessageBox.Show("File not found");
            }
        }

        private void LB_ExpectedHash_DoubleClick(object sender, EventArgs e)
        {
            BetterTreeNode betterTreeNode = (BetterTreeNode)TV_Partitions.SelectedNode;
            if (betterTreeNode.Offset != -1)
            {
                Clipboard.SetText(betterTreeNode.ExpectedHash);
            }
        }

        private void LB_ActualHash_DoubleClick(object sender, EventArgs e)
        {
            BetterTreeNode betterTreeNode = (BetterTreeNode)TV_Partitions.SelectedNode;
            if (betterTreeNode.Offset != -1)
            {
                Clipboard.SetText(betterTreeNode.ActualHash);
            }
        }

        private void TB_File_DragDrop(object sender, DragEventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                TB_File.Text = files[0];
                ProcessFile();
            }
        }

        private void TB_File_DragEnter(object sender, DragEventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string fileName = (string)e.Argument;

            using (FileStream fileStream = File.OpenRead(TB_File.Text))
            {
                using (FileStream fileStream2 = File.OpenWrite(fileName))
                {
                    new BinaryReader(fileStream);
                    new BinaryWriter(fileStream2);
                    fileStream.Position = selectedOffset;
                    byte[] buffer = new byte[8192];
                    long num = selectedSize;
                    int num2;
                    while ((num2 = fileStream.Read(buffer, 0, 8192)) > 0 && num > 0)
                    {
                        fileStream2.Write(buffer, 0, num2);
                        num -= num2;
                    }
                    fileStream.Close();
                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            B_Extract.Enabled = true;
            B_LoadROM.Enabled = true;
            B_TrimXCI.Enabled = true;
            B_ImportCert.Enabled = true;
            B_ClearCert.Enabled = true;

            if (e.Error != null)
            {
                MessageBox.Show("Error: " + e.Error.Message);
            }
            else
            {
                MessageBox.Show("Done extracting NCA!");
            }
        }
    }
}
