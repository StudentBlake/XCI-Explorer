using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Security.Cryptography;
using System.Reflection;
using XCI_Explorer.Helpers;
using XTSSharp;
using System.Net;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace XCI_Explorer {
    public class MainForm : Form {
        private string[] Language = new string[16]
        {
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

        private Image[] Icons = new Image[16];
        private long[] SecureSize;
        private long[] NormalSize;
        private long[] SecureOffset;
        private long[] NormalOffset;
        private string[] SecureName = { };
        private long gameNcaOffset;
        private long gameNcaSize;
        private long PFS0Offset;
        private long PFS0Size;
        private long selectedOffset;
        private long selectedSize;
        public List<char> chars = new List<char>();
        public byte[] NcaHeaderEncryptionKey1_Prod;
        public byte[] NcaHeaderEncryptionKey2_Prod;
        public string Mkey;
        public double UsedSize;
        private TreeViewFileSystem TV_Parti;
        private BetterTreeNode rootNode;
        private IContainer components;
        private Button B_LoadROM;
        private TabControl TABC_Main;
        private TabPage TABP_XCI;
        private TabPage tabPage2;
        private TreeView TV_Partitions;
        private TextBox TB_SDKVer;
        private Label label3;
        private TextBox TB_Capacity;
        private Label label2;
        private Label label1;
        private TextBox TB_TID;
        private TextBox TB_MKeyRev;
        private Label label4;
        private TextBox TB_ExactUsedSpace;
        private TextBox TB_ROMExactSize;
        private TextBox TB_UsedSpace;
        private TextBox TB_ROMSize;
        private Label label6;
        private Label label5;
        private TextBox TB_GameRev;
        private Label label7;
        private GroupBox groupBox1;
        private Button B_ClearCert;
        private Button B_ImportCert;
        private Button B_ExportCert;
        private ComboBox CB_RegionName;
        private TextBox TB_ProdCode;
        private Label label8;
        private GroupBox groupBox2;
        private TextBox TB_Dev;
        private Label label10;
        private TextBox TB_Name;
        private Label label9;
        private PictureBox PB_GameIcon;
        private Button B_ViewCert;
        public TextBox TB_File;
        private Label LB_SelectedData;
        private Label LB_DataOffset;
        private Label LB_DataSize;
        private Button B_Extract;
        private Label LB_ExpectedHash;
        private Label LB_ActualHash;
        private Label LB_HashedRegionSize;
        private BackgroundWorker backgroundWorker1;
        private Button B_TrimXCI;

        public MainForm() {
            InitializeComponent();

            // Set number of numbers in version number
            const int NUMBERSINVERSION = 3;

            string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string[] versionArray = assemblyVersion.Split('.');
            assemblyVersion = string.Join(".", versionArray.Take(NUMBERSINVERSION));
            this.Text = "XCI Explorer v" + assemblyVersion;

            LB_SelectedData.Text = "";
            LB_DataOffset.Text = "";
            LB_DataSize.Text = "";
            LB_HashedRegionSize.Text = "";
            LB_ActualHash.Text = "";
            LB_ExpectedHash.Text = "";

            //MAC - Set Current Directory to application directory so it can find the keys
            String startupPath = Application.StartupPath;
            Directory.SetCurrentDirectory(startupPath);

            if (!File.Exists("keys.txt")) {
                if (MessageBox.Show("keys.txt is missing.\nDo you want to automatically download it now?\n\nBy pressing 'Yes' you agree that you own these keys.\n", "XCI Explorer", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                    using (var client = new WebClient()) {
                        client.DownloadFile(Util.Base64Decode("aHR0cHM6Ly9wYXN0ZWJpbi5jb20vcmF3LzFKQTl4MjB1"), "keys.txt");
                    }
                }

                if (!File.Exists("keys.txt")) {
                    MessageBox.Show("keys.txt failed to load.\nPlease include keys.txt in the root folder.");
                    Environment.Exit(0);
                }
            }

            if (!File.Exists("tools\\hactool.exe")) {
                Directory.CreateDirectory("tools");
                MessageBox.Show("hactool.exe is missing.\nPlease include hactool.exe in the 'tools' folder.");
                Environment.Exit(0);
            }

            getKey();

            //MAC - Set the double clicked file name into the UI and process file
            String[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1) {
                TB_File.Text = args[1];
                Application.DoEvents();
                ProcessFile();
            }
        }

        private void getKey() {
            string text = (from x in File.ReadAllLines("keys.txt")
                           select x.Split('=') into x
                           where x.Length > 1
                           select x).ToDictionary((string[] x) => x[0].Trim(), (string[] x) => x[1])["header_key"].Replace(" ", "");
            NcaHeaderEncryptionKey1_Prod = Util.StringToByteArray(text.Remove(32, 32));
            NcaHeaderEncryptionKey2_Prod = Util.StringToByteArray(text.Remove(0, 32));
        }

        public bool getMKey() {
            Dictionary<string, string> dictionary = (from x in File.ReadAllLines("keys.txt")
                                                     select x.Split('=') into x
                                                     where x.Length > 1
                                                     select x).ToDictionary((string[] x) => x[0].Trim(), (string[] x) => x[1]);
            Mkey = "master_key_";
            if (NCA.NCA_Headers[0].MasterKeyRev == 0 || NCA.NCA_Headers[0].MasterKeyRev == 1) {
                Mkey += "00";
            }
            else if (NCA.NCA_Headers[0].MasterKeyRev < 17) {
                int num = NCA.NCA_Headers[0].MasterKeyRev - 1;
                Mkey = Mkey + "0" + num.ToString();
            }
            else if (NCA.NCA_Headers[0].MasterKeyRev >= 17) {
                int num2 = NCA.NCA_Headers[0].MasterKeyRev - 1;
                Mkey += num2.ToString();
            }
            try {
                Mkey = dictionary[Mkey].Replace(" ", "");
                return true;
            }
            catch {
                return false;
            }
        }

        private void ProcessFile() {
            if (Path.GetExtension(TB_File.Text).ToLower() == ".nsp") {
                // Code needs refactoring 
                LB_SelectedData.Text = "";
                LB_DataOffset.Text = "";
                LB_DataSize.Text = "";
                LB_HashedRegionSize.Text = "";
                LB_ExpectedHash.Text = "";
                LB_ActualHash.Text = "";
                B_Extract.Enabled = false;

                B_TrimXCI.Enabled = false;
                B_ExportCert.Enabled = false;
                B_ImportCert.Enabled = false;
                B_ViewCert.Enabled = false;
                B_ClearCert.Enabled = false;
                LoadNSPMetadata();
            }
            else if (CheckXCI()) {
                B_TrimXCI.Enabled = true;
                B_ExportCert.Enabled = true;
                B_ImportCert.Enabled = true;
                B_ViewCert.Enabled = true;
                B_ClearCert.Enabled = true;
                LoadXCI();
            }
            else {
                TB_File.Text = null;
                MessageBox.Show("Unsupported file.");
            }
        }

        private void B_LoadROM_Click(object sender, EventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Switch XCI/NSP (*.xci, *.nsp)|*.xci;*.nsp|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                TB_File.Text = openFileDialog.FileName;
                ProcessFile();
            }
        }

        private void LoadXCI() {
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
            while (num >= 1024.0 && num2 < array.Length - 1) {
                num2++;
                num /= 1024.0;
            }
            TB_ROMSize.Text = $"{num:0.##} {array[num2]}";
            double num3 = UsedSize = (double)(XCI.XCI_Headers[0].CardSize2 * 512 + 512);
            TB_ExactUsedSpace.Text = "(" + num3.ToString() + " bytes)";
            num2 = 0;
            while (num3 >= 1024.0 && num2 < array.Length - 1) {
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
        public void LoadNSPMetadata() {
            CB_RegionName.Items.Clear();
            CB_RegionName.Enabled = true;
            TB_Name.Text = "";
            TB_Dev.Text = "";
            PB_GameIcon.BackgroundImage = null;
            Array.Clear(Icons, 0, Icons.Length);
            TV_Partitions.Nodes.Clear();
            FileInfo fi = new FileInfo(TB_File.Text);
            string contentType = "";

            // Maximum number of files in NSP to read in
            const int MAXFILES = 100;

            //Get File Size
            string[] array_fs = new string[5] { "B", "KB", "MB", "GB", "TB" };
            double num_fs = (double)fi.Length;
            int num2_fs = 0;
            TB_ROMExactSize.Text = "(" + num_fs.ToString() + " bytes)";
            TB_ExactUsedSpace.Text = TB_ROMExactSize.Text;

            while (num_fs >= 1024.0 && num2_fs < array_fs.Length - 1) {
                num2_fs++;
                num_fs /= 1024.0;
            }
            TB_ROMSize.Text = $"{num_fs:0.##} {array_fs[num2_fs]}";
            TB_UsedSpace.Text = TB_ROMSize.Text;

            Process process = new Process();
            try {
                FileStream fileStream = File.OpenRead(TB_File.Text);
                string ncaTarget = "";
                string xmlVersion = "";

                List<char> chars = new List<char>();
                byte[] array = new byte[16];
                byte[] array2 = new byte[24];
                fileStream.Read(array, 0, 16);
                PFS0.PFS0_Headers[0] = new PFS0.PFS0_Header(array);
                if (!PFS0.PFS0_Headers[0].Magic.Contains("PFS0")) {
                    return;
                }
                PFS0.PFS0_Entry[] array3;
                array3 = new PFS0.PFS0_Entry[Math.Max(PFS0.PFS0_Headers[0].FileCount, MAXFILES)]; //Dump of TitleID 01009AA000FAA000 reports more than 10000000 files here, so it breaks the program. Standard is to have only 20 files

                for (int m = 0; m < PFS0.PFS0_Headers[0].FileCount; m++) {
                    fileStream.Position = 16 + 24 * m;
                    fileStream.Read(array2, 0, 24);
                    array3[m] = new PFS0.PFS0_Entry(array2);

                    if (m == MAXFILES - 1) //Dump of TitleID 01009AA000FAA000 reports more than 10000000 files here, so it breaks the program. Standard is to have only 20 files
                    {
                        break;
                    }
                }
                for (int n = 0; n < PFS0.PFS0_Headers[0].FileCount; n++) {
                    fileStream.Position = 16 + 24 * PFS0.PFS0_Headers[0].FileCount + array3[n].Name_ptr;
                    int num4;
                    while ((num4 = fileStream.ReadByte()) != 0 && num4 != 0) {
                        chars.Add((char)num4);
                    }
                    array3[n].Name = new string(chars.ToArray());
                    chars.Clear();

                    //Console.WriteLine("FC: " + PFS0.PFS0_Headers[0].FileCount.ToString() + " Name: " + array3[n].Name);

                    if (array3[n].Name.EndsWith(".cnmt.xml")) {
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

                        if (contentType != "AddOnContent") {
                            foreach (XElement xe in xml.Descendants("Content")) {
                                if (xe.Element("Type").Value != "Control") {
                                    continue;
                                }

                                ncaTarget = xe.Element("Id").Value + ".nca";
                                break;
                            }
                        }
                        else //This is a DLC
                        {
                            foreach (XElement xe in xml.Descendants("Content")) {
                                if (xe.Element("Type").Value != "Meta") {
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

                for (int n = 0; n < PFS0.PFS0_Headers[0].FileCount; n++) {
                    if (array3[n].Name.Equals(ncaTarget)) {
                        if (!Directory.Exists("tmp")) {
                            Directory.CreateDirectory("tmp");
                        }

                        byte[] array5 = new byte[64 * 1024];
                        fileStream.Position = 16 + 24 * PFS0.PFS0_Headers[0].FileCount + PFS0.PFS0_Headers[0].StringTableSize + array3[n].Offset;

                        using (Stream output = File.Create("tmp\\" + ncaTarget)) {
                            long Size = array3[n].Size;
                            int result = 0;
                            while ((result = fileStream.Read(array5, 0, (int)Math.Min(array5.Length, Size))) > 0) {
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

                if (contentType != "AddOnContent") {
                    process = new Process();
                    process.StartInfo = new ProcessStartInfo {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = "tools\\hactool.exe",
                        Arguments = "-k keys.txt --romfsdir=tmp tmp/" + ncaTarget
                    };

                    process.Start();
                    process.WaitForExit();
                    process.Close();
                    byte[] flux = new byte[200];

                    try {
                        byte[] source = File.ReadAllBytes("tmp\\control.nacp");
                        NACP.NACP_Datas[0] = new NACP.NACP_Data(source.Skip(0x3000).Take(0x1000).ToArray());

                        for (int i = 0; i < NACP.NACP_Strings.Length; i++) {
                            NACP.NACP_Strings[i] = new NACP.NACP_String(source.Skip(i * 0x300).Take(0x300).ToArray());

                            if (NACP.NACP_Strings[i].Check != 0) {
                                CB_RegionName.Items.Add(Language[i]);
                                string icon_filename = "tmp\\icon_" + Language[i].Replace(" ", "") + ".dat";
                                if (File.Exists(icon_filename)) {
                                    using (Bitmap original = new Bitmap(icon_filename)) {
                                        Icons[i] = new Bitmap(original);
                                        PB_GameIcon.BackgroundImage = Icons[i];
                                    }
                                }
                            }
                        }
                        if(xmlVersion.Trim() == "") {
                            TB_GameRev.Text = NACP.NACP_Datas[0].GameVer.Replace("\0", "");
                        }
                        else {
                            TB_GameRev.Text = xmlVersion + " (" + NACP.NACP_Datas[0].GameVer.Replace("\0", "") + ")";
                        }
                        TB_ProdCode.Text = NACP.NACP_Datas[0].GameProd.Replace("\0", "");
                        if (TB_ProdCode.Text == "") {
                            TB_ProdCode.Text = "No Prod. ID";
                        }

                        for (int z = 0; z < NACP.NACP_Strings.Length; z++) {
                            if (NACP.NACP_Strings[z].GameName.Replace("\0", "") != "") {
                                TB_Name.Text = NACP.NACP_Strings[z].GameName.Replace("\0", "");
                                break;
                            }
                        }
                        for (int z = 0; z < NACP.NACP_Strings.Length; z++) {
                            if (NACP.NACP_Strings[z].GameAuthor.Replace("\0", "") != "") {
                                TB_Dev.Text = NACP.NACP_Strings[z].GameAuthor.Replace("\0", "");
                                break;
                            }
                        }
                    }
                    catch { }

                    /*if (contentType == "Patch") {
                    }*/
                }
                else {
                    TB_GameRev.Text = "";
                    TB_ProdCode.Text = "No Prod. ID";
                }

                // Lets get SDK Version, Distribution Type and Masterkey revision
                // This is far from the best aproach, but it's what we have for now
                process = new Process();
                process.StartInfo = new ProcessStartInfo {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "tools\\hactool.exe",
                    Arguments = "-k keys.txt tmp/" + ncaTarget,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                process.Start();
                StreamReader sr = process.StandardOutput;

                while (sr.Peek() >= 0) {
                    string str;
                    string[] strArray;
                    str = sr.ReadLine();
                    strArray = str.Split(':');
                    if (strArray[0] == "SDK Version") {
                        TB_SDKVer.Text = strArray[1].Trim();
                    }
                    else if (strArray[0] == "Master Key Revision") {
                        string MasterKey = strArray[1].Trim();
                        if (MasterKey.Contains("Unknown")) {
                            int keyblob;
                            if (int.TryParse(new string(MasterKey.TakeWhile(Char.IsDigit).ToArray()), out keyblob)) {
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
            if (Directory.Exists("tmp")) {
                Directory.Delete("tmp", true);
            }

            TB_Capacity.Text = "eShop";

            if (TB_Name.Text.Trim() != "") {
                CB_RegionName.SelectedIndex = 0;
            }
        }

        private void LoadGameInfos() {
            CB_RegionName.Items.Clear();
            CB_RegionName.Enabled = true;
            TB_Name.Text = "";
            TB_Dev.Text = "";
            PB_GameIcon.BackgroundImage = null;
            Array.Clear(Icons, 0, Icons.Length);
            if (getMKey()) {
                using (FileStream fileStream = File.OpenRead(TB_File.Text)) {
                    List<string> ncaTarget = new List<string>();
                    string GameRevision = "";

                    for (int si = 0; si < SecureSize.Length; si++) {
                        if (SecureSize[si] > 0x4E20000) continue;

                        if (SecureName[si].EndsWith(".cnmt.nca")) {
                            if (File.Exists("meta")) {
                                File.Delete("meta");
                            }

                            if (Directory.Exists("data")) {
                                Directory.Delete("data", true);
                            }

                            using (FileStream fileStream2 = File.OpenWrite("meta")) {
                                fileStream.Position = SecureOffset[si];
                                byte[] buffer = new byte[8192];
                                long num = SecureSize[si];
                                int num4;
                                while ((num4 = fileStream.Read(buffer, 0, 8192)) > 0 && num > 0) {
                                    fileStream2.Write(buffer, 0, num4);
                                    num -= num4;
                                }
                                fileStream2.Close();
                            }

                            Process process = new Process();
                            process.StartInfo = new ProcessStartInfo {
                                WindowStyle = ProcessWindowStyle.Hidden,
                                FileName = "tools\\hactool.exe",
                                Arguments = "-k keys.txt --section0dir=data meta"
                            };
                            process.Start();
                            process.WaitForExit();

                            string[] cnmt = Directory.GetFiles("data", "*.cnmt");
                            if (cnmt.Length != 0) {
                                using (FileStream fileStream3 = File.OpenRead(cnmt[0])) {
                                    byte[] buffer = new byte[32];
                                    byte[] buffer2 = new byte[56];
                                    CNMT.CNMT_Header[] array7 = new CNMT.CNMT_Header[1];

                                    fileStream3.Read(buffer, 0, 32);
                                    array7[0] = new CNMT.CNMT_Header(buffer);

                                    fileStream3.Position = array7[0].Offset + 32;
                                    CNMT.CNMT_Entry[] array9 = new CNMT.CNMT_Entry[array7[0].ContentCount];
                                    for (int k = 0; k < array7[0].ContentCount; k++) {
                                        fileStream3.Read(buffer2, 0, 56);
                                        array9[k] = new CNMT.CNMT_Entry(buffer2);
                                        if (array9[k].Type == (byte)CNMT.CNMT_Entry.ContentType.CONTROL) {
                                            ncaTarget.Add(BitConverter.ToString(array9[k].NcaId).ToLower().Replace("-", "") + ".nca");
                                            break;
                                        }
                                    }

                                    fileStream3.Close();
                                }
                            }
                        }
                    }

                    for (int si = 0; si < SecureSize.Length; si++) {
                        if (SecureSize[si] > 0x4E20000) continue;

                        if (ncaTarget.Contains(SecureName[si])) {
                            if (File.Exists("meta")) {
                                File.Delete("meta");
                            }

                            if (Directory.Exists("data")) {
                                Directory.Delete("data", true);
                            }

                            using (FileStream fileStream2 = File.OpenWrite("meta")) {
                                fileStream.Position = SecureOffset[si];
                                byte[] buffer = new byte[8192];
                                long num = SecureSize[si];
                                int num2;
                                while ((num2 = fileStream.Read(buffer, 0, 8192)) > 0 && num > 0) {
                                    fileStream2.Write(buffer, 0, num2);
                                    num -= num2;
                                }
                                fileStream2.Close();
                            }

                            Process process = new Process();
                            process.StartInfo = new ProcessStartInfo {
                                WindowStyle = ProcessWindowStyle.Hidden,
                                FileName = "tools\\hactool.exe",
                                Arguments = "-k keys.txt --romfsdir=data meta"
                            };
                            process.Start();
                            process.WaitForExit();

                            if (File.Exists("data\\control.nacp")) {
                                byte[] source = File.ReadAllBytes("data\\control.nacp");
                                NACP.NACP_Datas[0] = new NACP.NACP_Data(source.Skip(0x3000).Take(0x1000).ToArray());

                                string GameVer = NACP.NACP_Datas[0].GameVer.Replace("\0", "");
                                Version version1, version2;
                                if (!Version.TryParse(Regex.Replace(GameRevision, @"[^\d.].*$", ""), out version1)) {
                                    version1 = new Version();
                                }
                                if (!Version.TryParse(Regex.Replace(GameVer, @"[^\d.].*$", ""), out version2)) {
                                    version2 = new Version();
                                }
                                if (version2.CompareTo(version1) > 0) {
                                    GameRevision = GameVer;

                                    for (int i = 0; i < NACP.NACP_Strings.Length; i++) {
                                        NACP.NACP_Strings[i] = new NACP.NACP_String(source.Skip(i * 0x300).Take(0x300).ToArray());
                                        if (NACP.NACP_Strings[i].Check != 0 && !CB_RegionName.Items.Contains(Language[i])) {
                                            CB_RegionName.Items.Add(Language[i]);
                                            string icon_filename = "data\\icon_" + Language[i].Replace(" ", "") + ".dat";
                                            if (File.Exists(icon_filename)) {
                                                using (Bitmap original = new Bitmap(icon_filename)) {
                                                    Icons[i] = new Bitmap(original);
                                                    PB_GameIcon.BackgroundImage = Icons[i];
                                                }
                                            }
                                        }
                                    }
                                    TB_ProdCode.Text = NACP.NACP_Datas[0].GameProd;
                                    if (TB_ProdCode.Text == "") {
                                        TB_ProdCode.Text = "No Prod. ID";
                                    }
                                    try {
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
            else {
                TB_Dev.Text = Mkey + " not found";
                TB_Name.Text = Mkey + " not found";
            }
        }

        private void LoadNCAData() {
            NCA.NCA_Headers[0] = new NCA.NCA_Header(DecryptNCAHeader(gameNcaOffset));
            TB_TID.Text = "0" + NCA.NCA_Headers[0].TitleID.ToString("X");
            TB_SDKVer.Text = $"{NCA.NCA_Headers[0].SDKVersion4}.{NCA.NCA_Headers[0].SDKVersion3}.{NCA.NCA_Headers[0].SDKVersion2}.{NCA.NCA_Headers[0].SDKVersion1}";
            TB_MKeyRev.Text = Util.GetMkey(NCA.NCA_Headers[0].MasterKeyRev);
        }

        //https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa
        public static string ByteArrayToString(byte[] ba) {
            StringBuilder hex = new StringBuilder(ba.Length * 2 + 2);
            hex.Append("0x");
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static string SHA256Bytes(byte[] ba) {
            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] hashValue;
            hashValue = mySHA256.ComputeHash(ba);
            return ByteArrayToString(hashValue);
        }

        private void LoadPartitionsOld() {
            string actualHash;
            byte[] hashBuffer;
            long offset;

            TV_Partitions.Nodes.Clear();
            TV_Parti = new TreeViewFileSystem(TV_Partitions);
            rootNode = new BetterTreeNode("root");
            rootNode.Offset = -1L;
            rootNode.Size = -1L;
            TV_Partitions.Nodes.Add(rootNode);
            FileStream fileStream = new FileStream(TB_File.Text, FileMode.Open, FileAccess.Read);
            HFS0.HSF0_Entry[] array = new HFS0.HSF0_Entry[HFS0.HFS0_Headers[0].FileCount];
            fileStream.Position = XCI.XCI_Headers[0].HFS0OffsetPartition + 16 + 64 * HFS0.HFS0_Headers[0].FileCount;
            long num = XCI.XCI_Headers[0].HFS0OffsetPartition + XCI.XCI_Headers[0].HFS0SizeParition;
            byte[] array2 = new byte[64];
            byte[] array3 = new byte[16];
            byte[] array4 = new byte[24];
            for (int i = 0; i < HFS0.HFS0_Headers[0].FileCount; i++) {
                fileStream.Position = XCI.XCI_Headers[0].HFS0OffsetPartition + 16 + 64 * i;
                fileStream.Read(array2, 0, 64);
                array[i] = new HFS0.HSF0_Entry(array2);
                fileStream.Position = XCI.XCI_Headers[0].HFS0OffsetPartition + 16 + 64 * HFS0.HFS0_Headers[0].FileCount + array[i].Name_ptr;
                int num2;
                while ((num2 = fileStream.ReadByte()) != 0 && num2 != 0) {
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
                if (array[i].Name == "secure") {
                    SecureSize = new long[array5[0].FileCount];
                    SecureOffset = new long[array5[0].FileCount];
                    SecureName = new string[array5[0].FileCount];
                }
                if (array[i].Name == "normal") {
                    NormalSize = new long[array5[0].FileCount];
                    NormalOffset = new long[array5[0].FileCount];
                }
                HFS0.HSF0_Entry[] array6 = new HFS0.HSF0_Entry[array5[0].FileCount];
                for (int j = 0; j < array5[0].FileCount; j++) {
                    fileStream.Position = array[i].Offset + num + 16 + 64 * j;
                    fileStream.Read(array2, 0, 64);
                    array6[j] = new HFS0.HSF0_Entry(array2);
                    fileStream.Position = array[i].Offset + num + 16 + 64 * array5[0].FileCount + array6[j].Name_ptr;
                    while ((num2 = fileStream.ReadByte()) != 0 && num2 != 0) {
                        chars.Add((char)num2);
                    }
                    array6[j].Name = new string(chars.ToArray());
                    chars.Clear();
                    if (array[i].Name == "secure") {
                        SecureSize[j] = array6[j].Size;
                        SecureOffset[j] = array[i].Offset + array6[j].Offset + num + 16 + array5[0].StringTableSize + array5[0].FileCount * 64;
                        SecureName[j] = array6[j].Name;
                    }
                    if (array[i].Name == "normal") {
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
                    if (array7.Length != 0) {
                        TV_Parti.AddFile(array6[j].Name, (BetterTreeNode)array7[0], 0L, 0L);
                    }
                }
            }
            long num3 = -9223372036854775808L;
            for (int k = 0; k < SecureSize.Length; k++) {
                if (SecureSize[k] > num3) {
                    gameNcaSize = SecureSize[k];
                    gameNcaOffset = SecureOffset[k];
                    num3 = SecureSize[k];
                }
            }
            PFS0Offset = gameNcaOffset + 32768;
            fileStream.Position = PFS0Offset;
            fileStream.Read(array3, 0, 16);
            PFS0.PFS0_Headers[0] = new PFS0.PFS0_Header(array3);
            PFS0.PFS0_Entry[] array8;
            array8 = new PFS0.PFS0_Entry[PFS0.PFS0_Headers[0].FileCount];
            for (int m = 0; m < PFS0.PFS0_Headers[0].FileCount; m++) {
                fileStream.Position = PFS0Offset + 16 + 24 * m;
                fileStream.Read(array4, 0, 24);
                array8[m] = new PFS0.PFS0_Entry(array4);
                PFS0Size += array8[m].Size;
            }
            TV_Parti.AddFile("boot.psf0", rootNode, PFS0Offset, 16 + 24 * PFS0.PFS0_Headers[0].FileCount + 64 + PFS0Size);
            BetterTreeNode betterTreeNode2 = TV_Parti.AddDir("boot", rootNode);
            for (int n = 0; n < PFS0.PFS0_Headers[0].FileCount; n++) {
                fileStream.Position = PFS0Offset + 16 + 24 * PFS0.PFS0_Headers[0].FileCount + array8[n].Name_ptr;
                int num4;
                while ((num4 = fileStream.ReadByte()) != 0 && num4 != 0) {
                    chars.Add((char)num4);
                }
                array8[n].Name = new string(chars.ToArray());
                chars.Clear();
                TV_Parti.AddFile(array8[n].Name, betterTreeNode2, PFS0Offset + array8[n].Offset + 16 + PFS0.PFS0_Headers[0].StringTableSize + PFS0.PFS0_Headers[0].FileCount * 24, array8[n].Size);
                TreeNode[] array9 = TV_Partitions.Nodes.Find(betterTreeNode2.Text, true);
                if (array9.Length != 0) {
                    TV_Parti.AddFile(array8[n].Name, (BetterTreeNode)array9[0], 0L, 0L);
                }
            }
            fileStream.Close();
        }

        private void LoadPartitions() {
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
            for (int i = 0; i < HFS0.HFS0_Headers[0].FileCount; i++) {
                fileStream.Position = XCI.XCI_Headers[0].HFS0OffsetPartition + 16 + 64 * i;
                fileStream.Read(array2, 0, 64);
                array[i] = new HFS0.HSF0_Entry(array2);
                fileStream.Position = XCI.XCI_Headers[0].HFS0OffsetPartition + 16 + 64 * HFS0.HFS0_Headers[0].FileCount + array[i].Name_ptr;
                int num2;
                while ((num2 = fileStream.ReadByte()) != 0 && num2 != 0) {
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
                if (array[i].Name == "secure") {
                    SecureSize = new long[array5[0].FileCount];
                    SecureOffset = new long[array5[0].FileCount];
                    SecureName = new string[array5[0].FileCount];
                }
                if (array[i].Name == "normal") {
                    NormalSize = new long[array5[0].FileCount];
                    NormalOffset = new long[array5[0].FileCount];
                }
                if (array[i].Name == "logo") {
                    if (array5[0].FileCount > 0) {
                        LogoPartition = true;
                    }
                }
                HFS0.HSF0_Entry[] array6 = new HFS0.HSF0_Entry[array5[0].FileCount];
                for (int j = 0; j < array5[0].FileCount; j++) {
                    fileStream.Position = array[i].Offset + num + 16 + 64 * j;
                    fileStream.Read(array2, 0, 64);
                    array6[j] = new HFS0.HSF0_Entry(array2);
                    fileStream.Position = array[i].Offset + num + 16 + 64 * array5[0].FileCount + array6[j].Name_ptr;
                    while ((num2 = fileStream.ReadByte()) != 0 && num2 != 0) {
                        chars.Add((char)num2);
                    }
                    array6[j].Name = new string(chars.ToArray());
                    chars.Clear();
                    if (array[i].Name == "secure") {
                        SecureSize[j] = array6[j].Size;
                        SecureOffset[j] = array[i].Offset + array6[j].Offset + num + 16 + array5[0].StringTableSize + array5[0].FileCount * 64;
                        SecureName[j] = array6[j].Name;
                    }
                    if (array[i].Name == "normal") {
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
                    if (array7.Length != 0) {
                        TV_Parti.AddFile(array6[j].Name, (BetterTreeNode)array7[0], 0L, 0L);
                    }
                }
            }
            long num3 = -9223372036854775808L;
            for (int k = 0; k < SecureSize.Length; k++) {
                if (SecureSize[k] > num3) {
                    gameNcaSize = SecureSize[k];
                    gameNcaOffset = SecureOffset[k];
                    num3 = SecureSize[k];
                }
            }
            PFS0Offset = gameNcaOffset + 32768;
            fileStream.Position = PFS0Offset;
            fileStream.Read(array3, 0, 16);
            PFS0.PFS0_Headers[0] = new PFS0.PFS0_Header(array3);
            if (PFS0.PFS0_Headers[0].FileCount == 2 || !LogoPartition) {
                PFS0.PFS0_Entry[] array8;
                try {
                    array8 = new PFS0.PFS0_Entry[PFS0.PFS0_Headers[0].FileCount];
                }
                catch (Exception ex) {
                    array8 = new PFS0.PFS0_Entry[0];
                    Debug.WriteLine("Partitions Error: " + ex.Message);
                }
                for (int m = 0; m < PFS0.PFS0_Headers[0].FileCount; m++) {
                    fileStream.Position = PFS0Offset + 16 + 24 * m;
                    fileStream.Read(array4, 0, 24);
                    array8[m] = new PFS0.PFS0_Entry(array4);
                    PFS0Size += array8[m].Size;
                }
                TV_Parti.AddFile("boot.psf0", rootNode, PFS0Offset, 16 + 24 * PFS0.PFS0_Headers[0].FileCount + 64 + PFS0Size);
                BetterTreeNode betterTreeNode2 = TV_Parti.AddDir("boot", rootNode);
                for (int n = 0; n < PFS0.PFS0_Headers[0].FileCount; n++) {
                    fileStream.Position = PFS0Offset + 16 + 24 * PFS0.PFS0_Headers[0].FileCount + array8[n].Name_ptr;
                    int num4;
                    while ((num4 = fileStream.ReadByte()) != 0 && num4 != 0) {
                        chars.Add((char)num4);
                    }
                    array8[n].Name = new string(chars.ToArray());
                    chars.Clear();
                    TV_Parti.AddFile(array8[n].Name, betterTreeNode2, PFS0Offset + array8[n].Offset + 16 + PFS0.PFS0_Headers[0].StringTableSize + PFS0.PFS0_Headers[0].FileCount * 24, array8[n].Size);
                    TreeNode[] array9 = TV_Partitions.Nodes.Find(betterTreeNode2.Text, true);
                    if (array9.Length != 0) {
                        TV_Parti.AddFile(array8[n].Name, (BetterTreeNode)array9[0], 0L, 0L);
                    }
                }
            }
            fileStream.Close();
        }

        private void TV_Partitions_AfterSelect(object sender, TreeViewEventArgs e) {
            BetterTreeNode betterTreeNode = (BetterTreeNode)TV_Partitions.SelectedNode;
            if (betterTreeNode.Offset != -1) {
                selectedOffset = betterTreeNode.Offset;
                selectedSize = betterTreeNode.Size;
                string expectedHash = betterTreeNode.ExpectedHash;
                string actualHash = betterTreeNode.ActualHash;
                long HashedRegionSize = betterTreeNode.HashedRegionSize;

                LB_DataOffset.Text = "Offset: 0x" + selectedOffset.ToString("X");
                LB_SelectedData.Text = e.Node.Text;
                if (backgroundWorker1.IsBusy != true) {
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
                while (num >= 1024.0 && num2 < array.Length - 1) {
                    num2++;
                    num /= 1024.0;
                }
                LB_DataSize.Text = "Size:   0x" + selectedSize.ToString("X") + " (" + num.ToString() + array[num2] + ")";

                if (HashedRegionSize != 0) {
                    LB_HashedRegionSize.Text = "HashedRegionSize: 0x" + HashedRegionSize.ToString("X");
                }
                else {
                    LB_HashedRegionSize.Text = "";
                }

                if (!string.IsNullOrEmpty(expectedHash)) {
                    LB_ExpectedHash.Text = "Header Hash: " + expectedHash.Substring(0, 32);
                }
                else {
                    LB_ExpectedHash.Text = "";
                }

                if (!string.IsNullOrEmpty(actualHash)) {
                    LB_ActualHash.Text = "Actual Hash: " + actualHash.Substring(0, 32);
                    if (actualHash == expectedHash) {
                        LB_ActualHash.ForeColor = System.Drawing.Color.Green;
                    }
                    else {
                        LB_ActualHash.ForeColor = System.Drawing.Color.Red;
                    }
                }
                else {
                    LB_ActualHash.Text = "";
                }

            }
            else {
                LB_SelectedData.Text = "";
                LB_DataOffset.Text = "";
                LB_DataSize.Text = "";
                LB_HashedRegionSize.Text = "";
                LB_ExpectedHash.Text = "";
                LB_ActualHash.Text = "";
                B_Extract.Enabled = false;
            }
        }

        public bool CheckXCI() {
            FileStream fileStream = new FileStream(TB_File.Text, FileMode.Open, FileAccess.Read);
            byte[] array = new byte[61440];
            byte[] array2 = new byte[16];
            fileStream.Read(array, 0, 61440);
            XCI.XCI_Headers[0] = new XCI.XCI_Header(array);
            if (!XCI.XCI_Headers[0].Magic.Contains("HEAD")) {
                return false;
            }
            fileStream.Position = XCI.XCI_Headers[0].HFS0OffsetPartition;
            fileStream.Read(array2, 0, 16);
            HFS0.HFS0_Headers[0] = new HFS0.HFS0_Header(array2);
            fileStream.Close();
            return true;
        }

        private void B_ExportCert_Click(object sender, EventArgs e) {
            if (Util.checkFile(TB_File.Text)) {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "gamecard_cert.dat (*.dat)|*.dat";
                saveFileDialog.FileName = Path.GetFileName("gamecard_cert.dat");
                if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    FileStream fileStream = new FileStream(TB_File.Text, FileMode.Open, FileAccess.Read);
                    byte[] array = new byte[512];
                    fileStream.Position = 28672L;
                    fileStream.Read(array, 0, 512);
                    File.WriteAllBytes(saveFileDialog.FileName, array);
                    fileStream.Close();
                    MessageBox.Show("cert successfully exported to:\n\n" + saveFileDialog.FileName);
                }
            }
            else {
                MessageBox.Show("File not found");
            }
        }

        private void B_ImportCert_Click(object sender, EventArgs e) {
            if (Util.checkFile(TB_File.Text)) {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "gamecard_cert (*.dat)|*.dat|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK && new FileInfo(openFileDialog.FileName).Length == 512) {
                    using (Stream stream = File.Open(TB_File.Text, FileMode.Open)) {
                        stream.Position = 28672L;
                        stream.Write(File.ReadAllBytes(openFileDialog.FileName), 0, 512);
                    }
                    MessageBox.Show("Cert successfully imported from:\n\n" + openFileDialog.FileName);
                }
            }
            else {
                MessageBox.Show("File not found");
            }
        }

        private void B_ViewCert_Click(object sender, EventArgs e) {
            if (Util.checkFile(TB_File.Text)) {
                CertForm cert = new CertForm(this);
                cert.Text = "Cert Data - " + TB_File.Text;
                cert.Show();
            }
            else {
                MessageBox.Show("File not found");
            }
        }

        private void B_ClearCert_Click(object sender, EventArgs e) {
            if (Util.checkFile(TB_File.Text)) {
                if (MessageBox.Show("The cert will be deleted permanently.\nContinue?", "XCI Explorer", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                    using (Stream stream = File.Open(TB_File.Text, FileMode.Open)) {
                        byte[] array = new byte[512];
                        for (int i = 0; i < array.Length; i++) {
                            array[i] = byte.MaxValue;
                        }
                        stream.Position = 28672L;
                        stream.Write(array, 0, array.Length);
                        MessageBox.Show("Cert deleted.");
                    }
                }
            }
            else {
                MessageBox.Show("File not found");
            }
        }

        private void B_Extract_Click(object sender, EventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = LB_SelectedData.Text;
            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                if (backgroundWorker1.IsBusy != true) {
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

        public byte[] DecryptNCAHeader(long offset) {
            byte[] array = new byte[3072];
            if (File.Exists(TB_File.Text)) {
                FileStream fileStream = new FileStream(TB_File.Text, FileMode.Open, FileAccess.Read);
                fileStream.Position = offset;
                fileStream.Read(array, 0, 3072);
                File.WriteAllBytes(TB_File.Text + ".tmp", array);
                Xts xts = XtsAes128.Create(NcaHeaderEncryptionKey1_Prod, NcaHeaderEncryptionKey2_Prod);
                using (BinaryReader binaryReader = new BinaryReader(File.OpenRead(TB_File.Text + ".tmp"))) {
                    using (XtsStream xtsStream = new XtsStream(binaryReader.BaseStream, xts, 512)) {
                        xtsStream.Read(array, 0, 3072);
                    }
                }
                File.Delete(TB_File.Text + ".tmp");
                fileStream.Close();
            }
            return array;
        }

        private void CB_RegionName_SelectedIndexChanged(object sender, EventArgs e) {
            int num = Array.FindIndex(Language, (string element) => element.StartsWith(CB_RegionName.Text, StringComparison.Ordinal));
            // Icons for 1-2 Switch in some languages are "missing"
            // This just shows the first real icon instead of a blank
            if (Icons[num] != null) {
                PB_GameIcon.BackgroundImage = Icons[num];
            }
            else {
                for (int i = 0; i < CB_RegionName.Items.Count; i++) {
                    if (Icons[i] != null) {
                        PB_GameIcon.BackgroundImage = Icons[i];
                        break;
                    }
                }
            }
            TB_Name.Text = NACP.NACP_Strings[num].GameName;
            TB_Dev.Text = NACP.NACP_Strings[num].GameAuthor;
        }

        private void B_TrimXCI_Click(object sender, EventArgs e) {
            if (Util.checkFile(TB_File.Text)) {
                if (MessageBox.Show("Trim XCI?", "XCI Explorer", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                    if (!TB_ROMExactSize.Text.Equals(TB_ExactUsedSpace.Text)) {
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
                        while (num >= 1024.0 && num2 < array.Length - 1) {
                            num2++;
                            num /= 1024.0;
                        }
                        TB_ROMSize.Text = $"{num:0.##} {array[num2]}";
                        double num3 = UsedSize = (double)(XCI.XCI_Headers[0].CardSize2 * 512 + 512);
                        TB_ExactUsedSpace.Text = "(" + num3.ToString() + " bytes)";
                        num2 = 0;
                        while (num3 >= 1024.0 && num2 < array.Length - 1) {
                            num2++;
                            num3 /= 1024.0;
                        }
                        TB_UsedSpace.Text = $"{num3:0.##} {array[num2]}";
                    }
                    else {
                        MessageBox.Show("No trimming needed!");
                    }
                }
            }
            else {
                MessageBox.Show("File not found");
            }
        }

        private void LB_ExpectedHash_DoubleClick(object sender, EventArgs e) {
            BetterTreeNode betterTreeNode = (BetterTreeNode)TV_Partitions.SelectedNode;
            if (betterTreeNode.Offset != -1) {
                Clipboard.SetText(betterTreeNode.ExpectedHash);
            }
        }

        private void LB_ActualHash_DoubleClick(object sender, EventArgs e) {
            BetterTreeNode betterTreeNode = (BetterTreeNode)TV_Partitions.SelectedNode;
            if (betterTreeNode.Offset != -1) {
                Clipboard.SetText(betterTreeNode.ActualHash);
            }
        }

        private void TB_File_DragDrop(object sender, DragEventArgs e) {
            if (backgroundWorker1.IsBusy != true) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                TB_File.Text = files[0];
                ProcessFile();
            }
        }

        private void TB_File_DragEnter(object sender, DragEventArgs e) {
            if (backgroundWorker1.IsBusy != true) {
                if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                    e.Effect = DragDropEffects.Copy;
                }
                else {
                    e.Effect = DragDropEffects.None;
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;
            string fileName = (string)e.Argument;

            using (FileStream fileStream = File.OpenRead(TB_File.Text)) {
                using (FileStream fileStream2 = File.OpenWrite(fileName)) {
                    new BinaryReader(fileStream);
                    new BinaryWriter(fileStream2);
                    fileStream.Position = selectedOffset;
                    byte[] buffer = new byte[8192];
                    long num = selectedSize;
                    int num2;
                    while ((num2 = fileStream.Read(buffer, 0, 8192)) > 0 && num > 0) {
                        fileStream2.Write(buffer, 0, num2);
                        num -= num2;
                    }
                    fileStream.Close();
                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            B_Extract.Enabled = true;
            B_LoadROM.Enabled = true;
            B_TrimXCI.Enabled = true;
            B_ImportCert.Enabled = true;
            B_ClearCert.Enabled = true;

            if (e.Error != null) {
                MessageBox.Show("Error: " + e.Error.Message);
            }
            else {
                MessageBox.Show("Done extracting NCA!");
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing && components != null) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.B_LoadROM = new System.Windows.Forms.Button();
            this.TB_File = new System.Windows.Forms.TextBox();
            this.TABC_Main = new System.Windows.Forms.TabControl();
            this.TABP_XCI = new System.Windows.Forms.TabPage();
            this.B_TrimXCI = new System.Windows.Forms.Button();
            this.TB_ProdCode = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.TB_Dev = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.TB_Name = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.PB_GameIcon = new System.Windows.Forms.PictureBox();
            this.CB_RegionName = new System.Windows.Forms.ComboBox();
            this.TB_GameRev = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.B_ViewCert = new System.Windows.Forms.Button();
            this.B_ClearCert = new System.Windows.Forms.Button();
            this.B_ImportCert = new System.Windows.Forms.Button();
            this.B_ExportCert = new System.Windows.Forms.Button();
            this.TB_ExactUsedSpace = new System.Windows.Forms.TextBox();
            this.TB_ROMExactSize = new System.Windows.Forms.TextBox();
            this.TB_UsedSpace = new System.Windows.Forms.TextBox();
            this.TB_ROMSize = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.TB_MKeyRev = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TB_SDKVer = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TB_Capacity = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.TB_TID = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.LB_HashedRegionSize = new System.Windows.Forms.Label();
            this.LB_ActualHash = new System.Windows.Forms.Label();
            this.LB_ExpectedHash = new System.Windows.Forms.Label();
            this.B_Extract = new System.Windows.Forms.Button();
            this.LB_DataSize = new System.Windows.Forms.Label();
            this.LB_DataOffset = new System.Windows.Forms.Label();
            this.LB_SelectedData = new System.Windows.Forms.Label();
            this.TV_Partitions = new System.Windows.Forms.TreeView();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.TABC_Main.SuspendLayout();
            this.TABP_XCI.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PB_GameIcon)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // B_LoadROM
            // 
            this.B_LoadROM.Location = new System.Drawing.Point(4, 12);
            this.B_LoadROM.Name = "B_LoadROM";
            this.B_LoadROM.Size = new System.Drawing.Size(75, 23);
            this.B_LoadROM.TabIndex = 0;
            this.B_LoadROM.Text = "Load Game";
            this.B_LoadROM.UseVisualStyleBackColor = true;
            this.B_LoadROM.Click += new System.EventHandler(this.B_LoadROM_Click);
            // 
            // TB_File
            // 
            this.TB_File.AllowDrop = true;
            this.TB_File.Location = new System.Drawing.Point(85, 13);
            this.TB_File.Name = "TB_File";
            this.TB_File.ReadOnly = true;
            this.TB_File.Size = new System.Drawing.Size(258, 20);
            this.TB_File.TabIndex = 1;
            this.TB_File.DragDrop += new System.Windows.Forms.DragEventHandler(this.TB_File_DragDrop);
            this.TB_File.DragEnter += new System.Windows.Forms.DragEventHandler(this.TB_File_DragEnter);
            // 
            // TABC_Main
            // 
            this.TABC_Main.Controls.Add(this.TABP_XCI);
            this.TABC_Main.Controls.Add(this.tabPage2);
            this.TABC_Main.Location = new System.Drawing.Point(4, 41);
            this.TABC_Main.Name = "TABC_Main";
            this.TABC_Main.SelectedIndex = 0;
            this.TABC_Main.Size = new System.Drawing.Size(355, 485);
            this.TABC_Main.TabIndex = 2;
            // 
            // TABP_XCI
            // 
            this.TABP_XCI.Controls.Add(this.B_TrimXCI);
            this.TABP_XCI.Controls.Add(this.TB_ProdCode);
            this.TABP_XCI.Controls.Add(this.label8);
            this.TABP_XCI.Controls.Add(this.groupBox2);
            this.TABP_XCI.Controls.Add(this.TB_GameRev);
            this.TABP_XCI.Controls.Add(this.label7);
            this.TABP_XCI.Controls.Add(this.groupBox1);
            this.TABP_XCI.Controls.Add(this.TB_ExactUsedSpace);
            this.TABP_XCI.Controls.Add(this.TB_ROMExactSize);
            this.TABP_XCI.Controls.Add(this.TB_UsedSpace);
            this.TABP_XCI.Controls.Add(this.TB_ROMSize);
            this.TABP_XCI.Controls.Add(this.label6);
            this.TABP_XCI.Controls.Add(this.label5);
            this.TABP_XCI.Controls.Add(this.TB_MKeyRev);
            this.TABP_XCI.Controls.Add(this.label4);
            this.TABP_XCI.Controls.Add(this.TB_SDKVer);
            this.TABP_XCI.Controls.Add(this.label3);
            this.TABP_XCI.Controls.Add(this.TB_Capacity);
            this.TABP_XCI.Controls.Add(this.label2);
            this.TABP_XCI.Controls.Add(this.label1);
            this.TABP_XCI.Controls.Add(this.TB_TID);
            this.TABP_XCI.Location = new System.Drawing.Point(4, 22);
            this.TABP_XCI.Name = "TABP_XCI";
            this.TABP_XCI.Padding = new System.Windows.Forms.Padding(3);
            this.TABP_XCI.Size = new System.Drawing.Size(347, 459);
            this.TABP_XCI.TabIndex = 0;
            this.TABP_XCI.Text = "Main";
            this.TABP_XCI.UseVisualStyleBackColor = true;
            // 
            // B_TrimXCI
            // 
            this.B_TrimXCI.Location = new System.Drawing.Point(90, 207);
            this.B_TrimXCI.Name = "B_TrimXCI";
            this.B_TrimXCI.Size = new System.Drawing.Size(70, 23);
            this.B_TrimXCI.TabIndex = 21;
            this.B_TrimXCI.Text = "Trim XCI";
            this.B_TrimXCI.UseVisualStyleBackColor = true;
            this.B_TrimXCI.Click += new System.EventHandler(this.B_TrimXCI_Click);
            // 
            // TB_ProdCode
            // 
            this.TB_ProdCode.Location = new System.Drawing.Point(238, 115);
            this.TB_ProdCode.Name = "TB_ProdCode";
            this.TB_ProdCode.ReadOnly = true;
            this.TB_ProdCode.Size = new System.Drawing.Size(69, 20);
            this.TB_ProdCode.TabIndex = 20;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(235, 99);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(75, 13);
            this.label8.TabIndex = 19;
            this.label8.Text = "Product Code:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.TB_Dev);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.TB_Name);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.PB_GameIcon);
            this.groupBox2.Controls.Add(this.CB_RegionName);
            this.groupBox2.Location = new System.Drawing.Point(22, 296);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(301, 154);
            this.groupBox2.TabIndex = 18;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Game Infos";
            // 
            // TB_Dev
            // 
            this.TB_Dev.Location = new System.Drawing.Point(6, 117);
            this.TB_Dev.Name = "TB_Dev";
            this.TB_Dev.ReadOnly = true;
            this.TB_Dev.Size = new System.Drawing.Size(145, 20);
            this.TB_Dev.TabIndex = 24;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 101);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(59, 13);
            this.label10.TabIndex = 23;
            this.label10.Text = "Developer:";
            // 
            // TB_Name
            // 
            this.TB_Name.Location = new System.Drawing.Point(6, 66);
            this.TB_Name.Name = "TB_Name";
            this.TB_Name.ReadOnly = true;
            this.TB_Name.Size = new System.Drawing.Size(145, 20);
            this.TB_Name.TabIndex = 22;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 50);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(38, 13);
            this.label9.TabIndex = 21;
            this.label9.Text = "Name:";
            // 
            // PB_GameIcon
            // 
            this.PB_GameIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.PB_GameIcon.Location = new System.Drawing.Point(190, 43);
            this.PB_GameIcon.Name = "PB_GameIcon";
            this.PB_GameIcon.Size = new System.Drawing.Size(105, 105);
            this.PB_GameIcon.TabIndex = 18;
            this.PB_GameIcon.TabStop = false;
            // 
            // CB_RegionName
            // 
            this.CB_RegionName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CB_RegionName.FormattingEnabled = true;
            this.CB_RegionName.Location = new System.Drawing.Point(77, 14);
            this.CB_RegionName.Name = "CB_RegionName";
            this.CB_RegionName.Size = new System.Drawing.Size(138, 21);
            this.CB_RegionName.TabIndex = 17;
            this.CB_RegionName.SelectedIndexChanged += new System.EventHandler(this.CB_RegionName_SelectedIndexChanged);
            // 
            // TB_GameRev
            // 
            this.TB_GameRev.Location = new System.Drawing.Point(24, 115);
            this.TB_GameRev.Name = "TB_GameRev";
            this.TB_GameRev.ReadOnly = true;
            this.TB_GameRev.Size = new System.Drawing.Size(124, 20);
            this.TB_GameRev.TabIndex = 16;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(21, 99);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(82, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Game Revision:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.B_ViewCert);
            this.groupBox1.Controls.Add(this.B_ClearCert);
            this.groupBox1.Controls.Add(this.B_ImportCert);
            this.groupBox1.Controls.Add(this.B_ExportCert);
            this.groupBox1.Location = new System.Drawing.Point(22, 234);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(301, 52);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cert";
            // 
            // B_ViewCert
            // 
            this.B_ViewCert.Location = new System.Drawing.Point(156, 19);
            this.B_ViewCert.Name = "B_ViewCert";
            this.B_ViewCert.Size = new System.Drawing.Size(66, 23);
            this.B_ViewCert.TabIndex = 3;
            this.B_ViewCert.Text = "View Cert";
            this.B_ViewCert.UseVisualStyleBackColor = true;
            this.B_ViewCert.Click += new System.EventHandler(this.B_ViewCert_Click);
            // 
            // B_ClearCert
            // 
            this.B_ClearCert.Location = new System.Drawing.Point(229, 19);
            this.B_ClearCert.Name = "B_ClearCert";
            this.B_ClearCert.Size = new System.Drawing.Size(66, 23);
            this.B_ClearCert.TabIndex = 2;
            this.B_ClearCert.Text = "Clear Cert";
            this.B_ClearCert.UseVisualStyleBackColor = true;
            this.B_ClearCert.Click += new System.EventHandler(this.B_ClearCert_Click);
            // 
            // B_ImportCert
            // 
            this.B_ImportCert.Location = new System.Drawing.Point(83, 19);
            this.B_ImportCert.Name = "B_ImportCert";
            this.B_ImportCert.Size = new System.Drawing.Size(67, 23);
            this.B_ImportCert.TabIndex = 1;
            this.B_ImportCert.Text = "Import Cert";
            this.B_ImportCert.UseVisualStyleBackColor = true;
            this.B_ImportCert.Click += new System.EventHandler(this.B_ImportCert_Click);
            // 
            // B_ExportCert
            // 
            this.B_ExportCert.Location = new System.Drawing.Point(7, 19);
            this.B_ExportCert.Name = "B_ExportCert";
            this.B_ExportCert.Size = new System.Drawing.Size(70, 23);
            this.B_ExportCert.TabIndex = 0;
            this.B_ExportCert.Text = "Export Cert";
            this.B_ExportCert.UseVisualStyleBackColor = true;
            this.B_ExportCert.Click += new System.EventHandler(this.B_ExportCert_Click);
            // 
            // TB_ExactUsedSpace
            // 
            this.TB_ExactUsedSpace.Location = new System.Drawing.Point(166, 181);
            this.TB_ExactUsedSpace.Name = "TB_ExactUsedSpace";
            this.TB_ExactUsedSpace.ReadOnly = true;
            this.TB_ExactUsedSpace.Size = new System.Drawing.Size(157, 20);
            this.TB_ExactUsedSpace.TabIndex = 13;
            // 
            // TB_ROMExactSize
            // 
            this.TB_ROMExactSize.Location = new System.Drawing.Point(166, 154);
            this.TB_ROMExactSize.Name = "TB_ROMExactSize";
            this.TB_ROMExactSize.ReadOnly = true;
            this.TB_ROMExactSize.Size = new System.Drawing.Size(157, 20);
            this.TB_ROMExactSize.TabIndex = 12;
            // 
            // TB_UsedSpace
            // 
            this.TB_UsedSpace.Location = new System.Drawing.Point(91, 181);
            this.TB_UsedSpace.Name = "TB_UsedSpace";
            this.TB_UsedSpace.ReadOnly = true;
            this.TB_UsedSpace.Size = new System.Drawing.Size(69, 20);
            this.TB_UsedSpace.TabIndex = 11;
            // 
            // TB_ROMSize
            // 
            this.TB_ROMSize.Location = new System.Drawing.Point(91, 154);
            this.TB_ROMSize.Name = "TB_ROMSize";
            this.TB_ROMSize.ReadOnly = true;
            this.TB_ROMSize.Size = new System.Drawing.Size(69, 20);
            this.TB_ROMSize.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(18, 181);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Used space:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(27, 157);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "ROM Size:";
            // 
            // TB_MKeyRev
            // 
            this.TB_MKeyRev.Location = new System.Drawing.Point(24, 68);
            this.TB_MKeyRev.Name = "TB_MKeyRev";
            this.TB_MKeyRev.ReadOnly = true;
            this.TB_MKeyRev.Size = new System.Drawing.Size(124, 20);
            this.TB_MKeyRev.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(21, 52);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(104, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "MasterKey Revision:";
            // 
            // TB_SDKVer
            // 
            this.TB_SDKVer.Location = new System.Drawing.Point(238, 68);
            this.TB_SDKVer.Name = "TB_SDKVer";
            this.TB_SDKVer.ReadOnly = true;
            this.TB_SDKVer.Size = new System.Drawing.Size(69, 20);
            this.TB_SDKVer.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(235, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "SDK Version:";
            // 
            // TB_Capacity
            // 
            this.TB_Capacity.Location = new System.Drawing.Point(238, 23);
            this.TB_Capacity.Name = "TB_Capacity";
            this.TB_Capacity.ReadOnly = true;
            this.TB_Capacity.Size = new System.Drawing.Size(69, 20);
            this.TB_Capacity.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(235, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Capacity:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Title ID:";
            // 
            // TB_TID
            // 
            this.TB_TID.Location = new System.Drawing.Point(24, 23);
            this.TB_TID.Name = "TB_TID";
            this.TB_TID.ReadOnly = true;
            this.TB_TID.Size = new System.Drawing.Size(124, 20);
            this.TB_TID.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.LB_HashedRegionSize);
            this.tabPage2.Controls.Add(this.LB_ActualHash);
            this.tabPage2.Controls.Add(this.LB_ExpectedHash);
            this.tabPage2.Controls.Add(this.B_Extract);
            this.tabPage2.Controls.Add(this.LB_DataSize);
            this.tabPage2.Controls.Add(this.LB_DataOffset);
            this.tabPage2.Controls.Add(this.LB_SelectedData);
            this.tabPage2.Controls.Add(this.TV_Partitions);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(347, 459);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Partitions";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // LB_HashedRegionSize
            // 
            this.LB_HashedRegionSize.AutoSize = true;
            this.LB_HashedRegionSize.Location = new System.Drawing.Point(6, 416);
            this.LB_HashedRegionSize.Name = "LB_HashedRegionSize";
            this.LB_HashedRegionSize.Size = new System.Drawing.Size(101, 13);
            this.LB_HashedRegionSize.TabIndex = 7;
            this.LB_HashedRegionSize.Text = "HashedRegionSize:";
            // 
            // LB_ActualHash
            // 
            this.LB_ActualHash.AutoSize = true;
            this.LB_ActualHash.Location = new System.Drawing.Point(6, 443);
            this.LB_ActualHash.Name = "LB_ActualHash";
            this.LB_ActualHash.Size = new System.Drawing.Size(68, 13);
            this.LB_ActualHash.TabIndex = 6;
            this.LB_ActualHash.Text = "Actual Hash:";
            this.LB_ActualHash.DoubleClick += new System.EventHandler(this.LB_ActualHash_DoubleClick);
            // 
            // LB_ExpectedHash
            // 
            this.LB_ExpectedHash.AutoSize = true;
            this.LB_ExpectedHash.Location = new System.Drawing.Point(6, 430);
            this.LB_ExpectedHash.Name = "LB_ExpectedHash";
            this.LB_ExpectedHash.Size = new System.Drawing.Size(73, 13);
            this.LB_ExpectedHash.TabIndex = 5;
            this.LB_ExpectedHash.Text = "Header Hash:";
            this.LB_ExpectedHash.DoubleClick += new System.EventHandler(this.LB_ExpectedHash_DoubleClick);
            // 
            // B_Extract
            // 
            this.B_Extract.Enabled = false;
            this.B_Extract.Location = new System.Drawing.Point(296, 367);
            this.B_Extract.Name = "B_Extract";
            this.B_Extract.Size = new System.Drawing.Size(48, 23);
            this.B_Extract.TabIndex = 4;
            this.B_Extract.Text = "Extract";
            this.B_Extract.UseVisualStyleBackColor = true;
            this.B_Extract.Click += new System.EventHandler(this.B_Extract_Click);
            // 
            // LB_DataSize
            // 
            this.LB_DataSize.AutoSize = true;
            this.LB_DataSize.Location = new System.Drawing.Point(6, 403);
            this.LB_DataSize.Name = "LB_DataSize";
            this.LB_DataSize.Size = new System.Drawing.Size(30, 13);
            this.LB_DataSize.TabIndex = 3;
            this.LB_DataSize.Text = "Size:";
            // 
            // LB_DataOffset
            // 
            this.LB_DataOffset.AutoSize = true;
            this.LB_DataOffset.Location = new System.Drawing.Point(6, 390);
            this.LB_DataOffset.Name = "LB_DataOffset";
            this.LB_DataOffset.Size = new System.Drawing.Size(38, 13);
            this.LB_DataOffset.TabIndex = 2;
            this.LB_DataOffset.Text = "Offset:";
            // 
            // LB_SelectedData
            // 
            this.LB_SelectedData.AutoSize = true;
            this.LB_SelectedData.Location = new System.Drawing.Point(6, 367);
            this.LB_SelectedData.Name = "LB_SelectedData";
            this.LB_SelectedData.Size = new System.Drawing.Size(51, 13);
            this.LB_SelectedData.TabIndex = 1;
            this.LB_SelectedData.Text = "FileName";
            // 
            // TV_Partitions
            // 
            this.TV_Partitions.Dock = System.Windows.Forms.DockStyle.Top;
            this.TV_Partitions.HideSelection = false;
            this.TV_Partitions.Location = new System.Drawing.Point(3, 3);
            this.TV_Partitions.Name = "TV_Partitions";
            this.TV_Partitions.Size = new System.Drawing.Size(341, 361);
            this.TV_Partitions.TabIndex = 0;
            this.TV_Partitions.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TV_Partitions_AfterSelect);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(362, 529);
            this.Controls.Add(this.TABC_Main);
            this.Controls.Add(this.TB_File);
            this.Controls.Add(this.B_LoadROM);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "XCI Explorer";
            this.TABC_Main.ResumeLayout(false);
            this.TABP_XCI.ResumeLayout(false);
            this.TABP_XCI.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PB_GameIcon)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
