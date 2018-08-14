using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using XCI.Explorer.DTO;
using XCI.Explorer.Helpers;
using XCI.Explorer.Loader;
using XCI.Explorer.Properties;
using XCI.Model;

namespace XCI.Explorer.Forms
{
    public class MainForm : Form
    {
        private readonly Image[] _icons = new Image[16];
        private readonly IContainer components;
        private BackgroundWorker _backgroundWorker;
        private Button _btnClearCertificate;
        private Button _btnExportCertificate;
        private Button _btnExtract;
        private Button _btnImportCertificate;
        private Button _btnLoadRom;
        private Button _btnTrimXci;
        private Button _btnViewCertificate;
        private ComboBox _cbRegionName;
        private GameDto _gameDto;
        private long _gameNcaOffset;
        private GroupBox _groupBox1;
        private GroupBox _groupBox2;

        private KeyHandler _keyhandler;
        public List<char> Chars = new List<char>();
        private Label label1;
        private Label label10;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;


        private Label LB_ActualHash;
        private Label LB_DataOffset;
        private Label LB_DataSize;
        private Label LB_ExpectedHash;
        private Label LB_HashedRegionSize;
        private Label LB_SelectedData;
        private long[] NormalOffset;
        private long[] NormalSize;
        private PictureBox PB_GameIcon;
        private long PFS0Offset;
        private long PFS0Size;
        private BetterTreeNode rootNode;
        private long[] SecureOffset;
        private long[] SecureSize;
        private long selectedOffset;
        private long selectedSize;
        private TabControl TABC_Main;
        private TabPage TABP_XCI;
        private TabPage tabPage2;
        private TextBox TB_Capacity;
        private TextBox TB_Dev;
        private TextBox TB_ExactUsedSpace;
        private TextBox TB_GameRev;
        private TextBox TB_MKeyRev;
        private TextBox TB_Name;
        private TextBox TB_ProdCode;
        private TextBox TB_ROMExactSize;
        private TextBox TB_ROMSize;
        private TextBox TB_SDKVer;
        private TextBox TB_TID;
        private TextBox TB_UsedSpace;
        public TextBox tbFile;
        private TreeViewFileSystem tvFileSystem;
        private TreeView tvPartitions;
        public double UsedSize;

        public MainForm()
        {
            InitializeComponent();
            DisplayVersionNumber();
            MacSupport();
            ProcessFile();
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        public long GameNcaSize { get; private set; }


        private void GenerateMainForm()
        {
            _btnLoadRom = new Button();
            _btnTrimXci = new Button();
            _btnViewCertificate = new Button();
            _btnClearCertificate = new Button();
            _btnImportCertificate = new Button();
            _btnExportCertificate = new Button();
            _btnExtract = new Button();
            tbFile = new TextBox();
            TB_ProdCode = new TextBox();
            TB_Dev = new TextBox();
            TB_Name = new TextBox();
            TB_GameRev = new TextBox();
            TB_ExactUsedSpace = new TextBox();
            TB_ROMExactSize = new TextBox();
            TB_UsedSpace = new TextBox();
            TB_ROMSize = new TextBox();
            TB_MKeyRev = new TextBox();
            TB_SDKVer = new TextBox();
            TB_Capacity = new TextBox();
            TB_TID = new TextBox();
            label8 = new Label();
            label10 = new Label();
            label9 = new Label();
            label7 = new Label();
            label6 = new Label();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            LB_HashedRegionSize = new Label();
            LB_ActualHash = new Label();
            LB_ExpectedHash = new Label();
            LB_DataSize = new Label();
            LB_DataOffset = new Label();
            LB_SelectedData = new Label();
            TABC_Main = new TabControl();
            TABP_XCI = new TabPage();
            tabPage2 = new TabPage();
            _groupBox2 = new GroupBox();
            _groupBox1 = new GroupBox();
            _cbRegionName = new ComboBox();
            PB_GameIcon = new PictureBox();
            tvPartitions = new TreeView();
            _backgroundWorker = new BackgroundWorker();

            TABC_Main.SuspendLayout();
            TABP_XCI.SuspendLayout();
            _groupBox2.SuspendLayout();
            ((ISupportInitialize) PB_GameIcon).BeginInit();
            _groupBox1.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // B_LoadROM
            // 
            _btnLoadRom.Location = new Point(4, 12);
            _btnLoadRom.Name = "_btnLoadRom";
            _btnLoadRom.Size = new Size(75, 23);
            _btnLoadRom.TabIndex = 0;
            _btnLoadRom.Text = "Load XCI";
            _btnLoadRom.UseVisualStyleBackColor = true;
            _btnLoadRom.Click += BtnLoadRomClick;
            // 
            // tbFile
            // 
            tbFile.AllowDrop = true;
            tbFile.Location = new Point(85, 13);
            tbFile.Name = "tbFile";
            tbFile.ReadOnly = true;
            tbFile.Size = new Size(258, 20);
            tbFile.TabIndex = 1;
            tbFile.DragDrop += TB_File_DragDrop;
            tbFile.DragEnter += TB_File_DragEnter;
            // 
            // TABC_Main
            // 
            TABC_Main.Controls.Add(TABP_XCI);
            TABC_Main.Controls.Add(tabPage2);
            TABC_Main.Location = new Point(4, 41);
            TABC_Main.Name = "TABC_Main";
            TABC_Main.SelectedIndex = 0;
            TABC_Main.Size = new Size(355, 485);
            TABC_Main.TabIndex = 2;
            // 
            // TABP_XCI
            // 
            TABP_XCI.Controls.Add(_btnTrimXci);
            TABP_XCI.Controls.Add(TB_ProdCode);
            TABP_XCI.Controls.Add(label8);
            TABP_XCI.Controls.Add(_groupBox2);
            TABP_XCI.Controls.Add(TB_GameRev);
            TABP_XCI.Controls.Add(label7);
            TABP_XCI.Controls.Add(_groupBox1);
            TABP_XCI.Controls.Add(TB_ExactUsedSpace);
            TABP_XCI.Controls.Add(TB_ROMExactSize);
            TABP_XCI.Controls.Add(TB_UsedSpace);
            TABP_XCI.Controls.Add(TB_ROMSize);
            TABP_XCI.Controls.Add(label6);
            TABP_XCI.Controls.Add(label5);
            TABP_XCI.Controls.Add(TB_MKeyRev);
            TABP_XCI.Controls.Add(label4);
            TABP_XCI.Controls.Add(TB_SDKVer);
            TABP_XCI.Controls.Add(label3);
            TABP_XCI.Controls.Add(TB_Capacity);
            TABP_XCI.Controls.Add(label2);
            TABP_XCI.Controls.Add(label1);
            TABP_XCI.Controls.Add(TB_TID);
            TABP_XCI.Location = new Point(4, 22);
            TABP_XCI.Name = "TABP_XCI";
            TABP_XCI.Padding = new Padding(3);
            TABP_XCI.Size = new Size(347, 459);
            TABP_XCI.TabIndex = 0;
            TABP_XCI.Text = "Main";
            TABP_XCI.UseVisualStyleBackColor = true;
            // 
            // _btnTrimXCI
            // 
            _btnTrimXci.Location = new Point(90, 207);
            _btnTrimXci.Name = "_btnTrimXci";
            _btnTrimXci.Size = new Size(70, 23);
            _btnTrimXci.TabIndex = 21;
            _btnTrimXci.Text = "Trim XCI";
            _btnTrimXci.UseVisualStyleBackColor = true;
            _btnTrimXci.Click += BtnTrimXciClick;
            // 
            // TB_ProdCode
            // 
            TB_ProdCode.Location = new Point(238, 115);
            TB_ProdCode.Name = "TB_ProdCode";
            TB_ProdCode.ReadOnly = true;
            TB_ProdCode.Size = new Size(69, 20);
            TB_ProdCode.TabIndex = 20;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(235, 99);
            label8.Name = "label8";
            label8.Size = new Size(75, 13);
            label8.TabIndex = 19;
            label8.Text = "Product Code:";
            // 
            // groupBox2
            // 
            _groupBox2.Controls.Add(TB_Dev);
            _groupBox2.Controls.Add(label10);
            _groupBox2.Controls.Add(TB_Name);
            _groupBox2.Controls.Add(label9);
            _groupBox2.Controls.Add(PB_GameIcon);
            _groupBox2.Controls.Add(_cbRegionName);
            _groupBox2.Location = new Point(22, 296);
            _groupBox2.Name = "_groupBox2";
            _groupBox2.Size = new Size(301, 154);
            _groupBox2.TabIndex = 18;
            _groupBox2.TabStop = false;
            _groupBox2.Text = "Game Infos";
            // 
            // TB_Dev
            // 
            TB_Dev.Location = new Point(6, 117);
            TB_Dev.Name = "TB_Dev";
            TB_Dev.ReadOnly = true;
            TB_Dev.Size = new Size(145, 20);
            TB_Dev.TabIndex = 24;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(3, 101);
            label10.Name = "label10";
            label10.Size = new Size(59, 13);
            label10.TabIndex = 23;
            label10.Text = "Developer:";
            // 
            // TB_Name
            // 
            TB_Name.Location = new Point(6, 66);
            TB_Name.Name = "TB_Name";
            TB_Name.ReadOnly = true;
            TB_Name.Size = new Size(145, 20);
            TB_Name.TabIndex = 22;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(3, 50);
            label9.Name = "label9";
            label9.Size = new Size(38, 13);
            label9.TabIndex = 21;
            label9.Text = "Name:";
            // 
            // PB_GameIcon
            // 
            PB_GameIcon.BackgroundImageLayout = ImageLayout.Zoom;
            PB_GameIcon.Location = new Point(190, 43);
            PB_GameIcon.Name = "PB_GameIcon";
            PB_GameIcon.Size = new Size(105, 105);
            PB_GameIcon.TabIndex = 18;
            PB_GameIcon.TabStop = false;
            // 
            // _cbRegionName
            // 
            _cbRegionName.DropDownStyle = ComboBoxStyle.DropDownList;
            _cbRegionName.FormattingEnabled = true;
            _cbRegionName.Location = new Point(77, 14);
            _cbRegionName.Name = "_cbRegionName";
            _cbRegionName.Size = new Size(138, 21);
            _cbRegionName.TabIndex = 17;
            _cbRegionName.SelectedIndexChanged += CB_RegionName_SelectedIndexChanged;
            // 
            // TB_GameRev
            // 
            TB_GameRev.Location = new Point(24, 115);
            TB_GameRev.Name = "TB_GameRev";
            TB_GameRev.ReadOnly = true;
            TB_GameRev.Size = new Size(124, 20);
            TB_GameRev.TabIndex = 16;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(21, 99);
            label7.Name = "label7";
            label7.Size = new Size(82, 13);
            label7.TabIndex = 15;
            label7.Text = "Game Revision:";
            // 
            // groupBox1
            // 
            _groupBox1.Controls.Add(_btnViewCertificate);
            _groupBox1.Controls.Add(_btnClearCertificate);
            _groupBox1.Controls.Add(_btnImportCertificate);
            _groupBox1.Controls.Add(_btnExportCertificate);
            _groupBox1.Location = new Point(22, 234);
            _groupBox1.Name = "_groupBox1";
            _groupBox1.Size = new Size(301, 52);
            _groupBox1.TabIndex = 14;
            _groupBox1.TabStop = false;
            _groupBox1.Text = "Cert";
            // 
            // _btnViewCertificate
            // 
            _btnViewCertificate.Location = new Point(156, 19);
            _btnViewCertificate.Name = "_btnViewCertificate";
            _btnViewCertificate.Size = new Size(66, 23);
            _btnViewCertificate.TabIndex = 3;
            _btnViewCertificate.Text = "View Cert";
            _btnViewCertificate.UseVisualStyleBackColor = true;
            _btnViewCertificate.Click += BtnViewCertificateClick;
            // 
            // B_ClearCert
            // 
            _btnClearCertificate.Location = new Point(229, 19);
            _btnClearCertificate.Name = "_btnClearCertificate";
            _btnClearCertificate.Size = new Size(66, 23);
            _btnClearCertificate.TabIndex = 2;
            _btnClearCertificate.Text = "Clear Cert";
            _btnClearCertificate.UseVisualStyleBackColor = true;
            _btnClearCertificate.Click += BtnClearCertificateClick;
            // 
            // B_ImportCert
            // 
            _btnImportCertificate.Location = new Point(83, 19);
            _btnImportCertificate.Name = "_btnImportCertificate";
            _btnImportCertificate.Size = new Size(67, 23);
            _btnImportCertificate.TabIndex = 1;
            _btnImportCertificate.Text = "Import Cert";
            _btnImportCertificate.UseVisualStyleBackColor = true;
            _btnImportCertificate.Click += BtnImportCertificateClick;
            // 
            // B_ExportCert
            // 
            _btnExportCertificate.Location = new Point(7, 19);
            _btnExportCertificate.Name = "_btnExportCertificate";
            _btnExportCertificate.Size = new Size(70, 23);
            _btnExportCertificate.TabIndex = 0;
            _btnExportCertificate.Text = "Export Cert";
            _btnExportCertificate.UseVisualStyleBackColor = true;
            _btnExportCertificate.Click += BtnExportCertificateClick;
            // 
            // TB_ExactUsedSpace
            // 
            TB_ExactUsedSpace.Location = new Point(166, 181);
            TB_ExactUsedSpace.Name = "TB_ExactUsedSpace";
            TB_ExactUsedSpace.ReadOnly = true;
            TB_ExactUsedSpace.Size = new Size(157, 20);
            TB_ExactUsedSpace.TabIndex = 13;
            // 
            // TB_ROMExactSize
            // 
            TB_ROMExactSize.Location = new Point(166, 154);
            TB_ROMExactSize.Name = "TB_ROMExactSize";
            TB_ROMExactSize.ReadOnly = true;
            TB_ROMExactSize.Size = new Size(157, 20);
            TB_ROMExactSize.TabIndex = 12;
            // 
            // TB_UsedSpace
            // 
            TB_UsedSpace.Location = new Point(91, 181);
            TB_UsedSpace.Name = "TB_UsedSpace";
            TB_UsedSpace.ReadOnly = true;
            TB_UsedSpace.Size = new Size(69, 20);
            TB_UsedSpace.TabIndex = 11;
            // 
            // TB_ROMSize
            // 
            TB_ROMSize.Location = new Point(91, 154);
            TB_ROMSize.Name = "TB_ROMSize";
            TB_ROMSize.ReadOnly = true;
            TB_ROMSize.Size = new Size(69, 20);
            TB_ROMSize.TabIndex = 10;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(18, 181);
            label6.Name = "label6";
            label6.Size = new Size(67, 13);
            label6.TabIndex = 9;
            label6.Text = "Used space:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(27, 157);
            label5.Name = "label5";
            label5.Size = new Size(58, 13);
            label5.TabIndex = 8;
            label5.Text = "ROM Size:";
            // 
            // TB_MKeyRev
            // 
            TB_MKeyRev.Location = new Point(24, 68);
            TB_MKeyRev.Name = "TB_MKeyRev";
            TB_MKeyRev.ReadOnly = true;
            TB_MKeyRev.Size = new Size(124, 20);
            TB_MKeyRev.TabIndex = 7;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(21, 52);
            label4.Name = "label4";
            label4.Size = new Size(104, 13);
            label4.TabIndex = 6;
            label4.Text = "MasterKey Revision:";
            // 
            // TB_SDKVer
            // 
            TB_SDKVer.Location = new Point(238, 68);
            TB_SDKVer.Name = "TB_SDKVer";
            TB_SDKVer.ReadOnly = true;
            TB_SDKVer.Size = new Size(69, 20);
            TB_SDKVer.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(235, 52);
            label3.Name = "label3";
            label3.Size = new Size(70, 13);
            label3.TabIndex = 4;
            label3.Text = "SDK Version:";
            // 
            // TB_Capacity
            // 
            TB_Capacity.Location = new Point(238, 23);
            TB_Capacity.Name = "TB_Capacity";
            TB_Capacity.ReadOnly = true;
            TB_Capacity.Size = new Size(69, 20);
            TB_Capacity.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(235, 7);
            label2.Name = "label2";
            label2.Size = new Size(51, 13);
            label2.TabIndex = 2;
            label2.Text = "Capacity:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(21, 7);
            label1.Name = "label1";
            label1.Size = new Size(44, 13);
            label1.TabIndex = 1;
            label1.Text = "Title ID:";
            // 
            // TB_TID
            // 
            TB_TID.Location = new Point(24, 23);
            TB_TID.Name = "TB_TID";
            TB_TID.ReadOnly = true;
            TB_TID.Size = new Size(124, 20);
            TB_TID.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(LB_HashedRegionSize);
            tabPage2.Controls.Add(LB_ActualHash);
            tabPage2.Controls.Add(LB_ExpectedHash);
            tabPage2.Controls.Add(_btnExtract);
            tabPage2.Controls.Add(LB_DataSize);
            tabPage2.Controls.Add(LB_DataOffset);
            tabPage2.Controls.Add(LB_SelectedData);
            tabPage2.Controls.Add(tvPartitions);
            tabPage2.Location = new Point(4, 22);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(347, 459);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Partitions";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // LB_HashedRegionSize
            // 
            LB_HashedRegionSize.AutoSize = true;
            LB_HashedRegionSize.Location = new Point(6, 416);
            LB_HashedRegionSize.Name = "LB_HashedRegionSize";
            LB_HashedRegionSize.Size = new Size(101, 13);
            LB_HashedRegionSize.TabIndex = 7;
            LB_HashedRegionSize.Text = "HashedRegionSize:";
            // 
            // LB_ActualHash
            // 
            LB_ActualHash.AutoSize = true;
            LB_ActualHash.Location = new Point(6, 443);
            LB_ActualHash.Name = "LB_ActualHash";
            LB_ActualHash.Size = new Size(68, 13);
            LB_ActualHash.TabIndex = 6;
            LB_ActualHash.Text = "Actual Hash:";
            LB_ActualHash.DoubleClick += LB_ActualHash_DoubleClick;
            // 
            // LB_ExpectedHash
            // 
            LB_ExpectedHash.AutoSize = true;
            LB_ExpectedHash.Location = new Point(6, 430);
            LB_ExpectedHash.Name = "LB_ExpectedHash";
            LB_ExpectedHash.Size = new Size(73, 13);
            LB_ExpectedHash.TabIndex = 5;
            LB_ExpectedHash.Text = "Header Hash:";
            LB_ExpectedHash.DoubleClick += LB_ExpectedHash_DoubleClick;
            // 
            // B_Extract
            // 
            _btnExtract.Enabled = false;
            _btnExtract.Location = new Point(296, 367);
            _btnExtract.Name = "_btnExtract";
            _btnExtract.Size = new Size(48, 23);
            _btnExtract.TabIndex = 4;
            _btnExtract.Text = "Extract";
            _btnExtract.UseVisualStyleBackColor = true;
            _btnExtract.Click += BtnExtractClick;
            // 
            // LB_DataSize
            // 
            LB_DataSize.AutoSize = true;
            LB_DataSize.Location = new Point(6, 403);
            LB_DataSize.Name = "LB_DataSize";
            LB_DataSize.Size = new Size(30, 13);
            LB_DataSize.TabIndex = 3;
            LB_DataSize.Text = "Size:";
            // 
            // LB_DataOffset
            // 
            LB_DataOffset.AutoSize = true;
            LB_DataOffset.Location = new Point(6, 390);
            LB_DataOffset.Name = "LB_DataOffset";
            LB_DataOffset.Size = new Size(38, 13);
            LB_DataOffset.TabIndex = 2;
            LB_DataOffset.Text = "Offset:";
            // 
            // LB_SelectedData
            // 
            LB_SelectedData.AutoSize = true;
            LB_SelectedData.Location = new Point(6, 367);
            LB_SelectedData.Name = "LB_SelectedData";
            LB_SelectedData.Size = new Size(51, 13);
            LB_SelectedData.TabIndex = 1;
            LB_SelectedData.Text = "FileName";
            // 
            // tvPartitions
            // 
            tvPartitions.Dock = DockStyle.Top;
            tvPartitions.HideSelection = false;
            tvPartitions.Location = new Point(3, 3);
            tvPartitions.Name = "tvPartitions";
            tvPartitions.Size = new Size(341, 361);
            tvPartitions.TabIndex = 0;
            tvPartitions.AfterSelect += TV_Partitions_AfterSelect;
            // 
            // backgroundWorker
            // 
            _backgroundWorker.DoWork += BackgroundWorkerDoWork;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorkerRunWorkerCompleted;
            // 
            // MainForm
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(362, 529);
            Controls.Add(TABC_Main);
            Controls.Add(tbFile);
            Controls.Add(_btnLoadRom);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "MainForm";
            ShowIcon = false;
            Text = "XCI Explorer";
            TABC_Main.ResumeLayout(false);
            TABP_XCI.ResumeLayout(false);
            TABP_XCI.PerformLayout();
            _groupBox2.ResumeLayout(false);
            _groupBox2.PerformLayout();
            ((ISupportInitialize) PB_GameIcon).EndInit();
            _groupBox1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private void InitializeComponent()
        {
            GenerateMainForm();
            _keyhandler = new KeyHandler();
            TreeViewBuilder.CreateTreeViewFileSystem(tvPartitions, tvFileSystem);
        }

        private void DisplayVersionNumber()
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var versionArray = assemblyVersion.Split('.');
            assemblyVersion = string.Join(".", versionArray.Take(3));
            Text = $"XCI Explorer v{assemblyVersion}";
        }

        private void MacSupport()
        {
            SetCurrentDirectoryToApplicationDirectory();
            GetDoubleClickedFileName();
        }

        private void GetDoubleClickedFileName()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length <= 1) return;
            tbFile.Text = args[1];
            Application.DoEvents();
        }

        private static void SetCurrentDirectoryToApplicationDirectory()
        {
            var startupPath = Application.StartupPath;
            Directory.SetCurrentDirectory(startupPath);
        }

        private void ProcessFile()
        {
            if (CheckXci())
            {
                LoadXci();
            }
            else
            {
                tbFile.Text = null;
                MessageBox.Show("Unsupported file.");
            }
        }

        private void LoadXci()
        {
            _gameDto = new GameDto();
            var loader = new XciLoader(_gameDto, _keyhandler);

            CreateTreeViewFileSystem();

            loader.GetRomSize(tbFile.Text);
            loader.LoadPartitions(tbFile.Text, tvFileSystem, rootNode, tvPartitions);
            loader.LoadNca(TB_TID, TB_SDKVer,TB_MKeyRev, tbFile.Text);
            loader.LoadInfos(TB_Name,TB_Dev,PB_GameIcon,tbFile.Text,_cbRegionName, _icons, TB_GameRev, TB_ProdCode);

            PopulateForm(_gameDto);
        }

        private void BtnLoadRomClick(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog {Filter = Resources.OpenFileDialogFilters};
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            tbFile.Text = openFileDialog.FileName;
            ProcessFile();
        }

        private void PopulateForm(GameDto gameDto)
        {
            TB_ROMExactSize.Text = gameDto.ExactSize;
            TB_ROMSize.Text = gameDto.Size;
            TB_ExactUsedSpace.Text = gameDto.ExactUsedSpace;
            TB_UsedSpace.Text = gameDto.UsedSpace;
            TB_Capacity.Text = gameDto.Capacity;
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

        private void CreateTreeViewFileSystem()
        {
            tvPartitions.Nodes.Clear();
            tvFileSystem = new TreeViewFileSystem(tvPartitions);
            rootNode = new BetterTreeNode("root")
            {
                Offset = -1L,
                Size = -1L
            };
            tvPartitions.Nodes.Add(rootNode);
        }

        private void TV_Partitions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var betterTreeNode = (BetterTreeNode) tvPartitions.SelectedNode;
            if (betterTreeNode.Offset != -1)
            {
                selectedOffset = betterTreeNode.Offset;
                selectedSize = betterTreeNode.Size;
                var expectedHash = betterTreeNode.ExpectedHash;
                var actualHash = betterTreeNode.ActualHash;
                var hashedRegionSize = betterTreeNode.HashedRegionSize;

                LB_DataOffset.Text = "Offset: 0x" + selectedOffset.ToString("X");
                LB_SelectedData.Text = e.Node.Text;
                if (_backgroundWorker.IsBusy != true) _btnExtract.Enabled = true;
                var array = new string[5]
                {
                    "Bytes",
                    "Kilobytes",
                    "Megabytes",
                    "Gigabytes",
                    "TB"
                };
                double num = selectedSize;
                var num2 = 0;
                while (num >= 1024.0 && num2 < array.Length - 1)
                {
                    num2++;
                    num /= 1024.0;
                }
                LB_DataSize.Text = "Size:   0x" + selectedSize.ToString("X") + " (" + num + array[num2] + ")";

                if (hashedRegionSize != 0)
                    LB_HashedRegionSize.Text = "HashedRegionSize: 0x" + hashedRegionSize.ToString("X");
                else LB_HashedRegionSize.Text = "";

                if (!string.IsNullOrEmpty(expectedHash))
                    LB_ExpectedHash.Text = "Header Hash: " + expectedHash.Substring(0, 32);
                else LB_ExpectedHash.Text = "";

                if (!string.IsNullOrEmpty(actualHash))
                {
                    LB_ActualHash.Text = "Actual Hash: " + actualHash.Substring(0, 32);
                    LB_ActualHash.ForeColor = actualHash == expectedHash ? Color.Green : Color.Red;
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
                _btnExtract.Enabled = false;
            }
        }

        public bool CheckXci()
        {
            if (string.IsNullOrEmpty(tbFile.Text)) return false;
            var fileStream = new FileStream(tbFile.Text, FileMode.Open, FileAccess.Read);
            var array = new byte[61440];
            var array2 = new byte[16];
            fileStream.Read(array, 0, 61440);
            Xci.XciHeaders[0] = new Xci.XciHeader(array);
            if (!Xci.XciHeaders[0].Magic.Contains("HEAD")) return false;
            fileStream.Position = Xci.XciHeaders[0].Hfs0OffsetPartition;
            fileStream.Read(array2, 0, 16);
            Hfs0.Hfs0Headers[0] = new Hfs0.Hfs0Header(array2);
            fileStream.Close();
            return true;
        }

        private void BtnExportCertificateClick(object sender, EventArgs e)
        {
            if (Util.CheckFile(tbFile.Text))
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "gamecard_cert.dat (*.dat)|*.dat",
                    FileName = Path.GetFileName("gamecard_cert.dat")
                };
                if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
                var fileStream = new FileStream(tbFile.Text, FileMode.Open, FileAccess.Read);
                var array = new byte[512];
                fileStream.Position = 28672L;
                fileStream.Read(array, 0, 512);
                File.WriteAllBytes(saveFileDialog.FileName, array);
                fileStream.Close();
                MessageBox.Show("cert successfully exported to:\n\n" + saveFileDialog.FileName);
            }
            else
            {
                MessageBox.Show("File not found");
            }
        }

        private void BtnImportCertificateClick(object sender, EventArgs e)
        {
            if (Util.CheckFile(tbFile.Text))
            {
                var openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "gamecard_cert (*.dat)|*.dat|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK &&
                    new FileInfo(openFileDialog.FileName).Length == 512)
                {
                    using (Stream stream = File.Open(tbFile.Text, FileMode.Open))
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

        private void BtnViewCertificateClick(object sender, EventArgs e)
        {
            if (Util.CheckFile(tbFile.Text)) new CertificateView().Show();
            else MessageBox.Show("File not found");
        }

        private void BtnClearCertificateClick(object sender, EventArgs e)
        {
            if (Util.CheckFile(tbFile.Text))
            {
                if (MessageBox.Show("The cert will be deleted permanently.\nContinue?", "XCI Explorer",
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                    using (Stream stream = File.Open(tbFile.Text, FileMode.Open))
                    {
                        var array = new byte[512];
                        for (var i = 0; i < array.Length; i++) array[i] = byte.MaxValue;
                        stream.Position = 28672L;
                        stream.Write(array, 0, array.Length);
                        MessageBox.Show("Cert deleted.");
                    }
            }
            else
            {
                MessageBox.Show("File not found");
            }
        }

        private void BtnExtractClick(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog {FileName = LB_SelectedData.Text};
            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
            if (_backgroundWorker.IsBusy) return;
            _btnExtract.Enabled = false;
            _btnLoadRom.Enabled = false;
            _btnTrimXci.Enabled = false;
            _btnImportCertificate.Enabled = false;
            _btnClearCertificate.Enabled = false;

            // Start the asynchronous operation.
            _backgroundWorker.RunWorkerAsync(saveFileDialog.FileName);

            MessageBox.Show("Extracting NCA\nPlease wait...");
        }

        private void CB_RegionName_SelectedIndexChanged(object sender, EventArgs e)
        {
            var num = Array.FindIndex(Util.Language,
                element => element.StartsWith(_cbRegionName.Text, StringComparison.Ordinal));
            PB_GameIcon.BackgroundImage = _icons[num];
            TB_Name.Text = Nacp.NacpStrings[num].GameName;
            TB_Dev.Text = Nacp.NacpStrings[num].GameAuthor;
        }

        private void BtnTrimXciClick(object sender, EventArgs e)
        {
            if (Util.CheckFile(tbFile.Text))
            {
                if (MessageBox.Show("Trim XCI?", "XCI Explorer", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                if (!TB_ROMExactSize.Text.Equals(TB_ExactUsedSpace.Text))
                {
                    var fileStream = new FileStream(tbFile.Text, FileMode.Open, FileAccess.Write);
                    fileStream.SetLength((long) UsedSize);
                    fileStream.Close();
                    MessageBox.Show("Done.");
                    var array = new string[5]
                    {
                        "Bytes",
                        "Kilobytes",
                        "Megabytes",
                        "Gigabytes",
                        "TB"
                    };
                    double num = new FileInfo(tbFile.Text).Length;
                    TB_ROMExactSize.Text = "(" + num + " bytes)";
                    var num2 = 0;
                    while (num >= 1024.0 && num2 < array.Length - 1)
                    {
                        num2++;
                        num /= 1024.0;
                    }
                    TB_ROMSize.Text = $"{num:0.##} {array[num2]}";
                    var num3 = UsedSize = Xci.XciHeaders[0].CardSize2 * 512 + 512;
                    TB_ExactUsedSpace.Text = "(" + num3 + " bytes)";
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
            else
            {
                MessageBox.Show("File not found");
            }
        }

        private void LB_ExpectedHash_DoubleClick(object sender, EventArgs e)
        {
            var betterTreeNode = (BetterTreeNode) tvPartitions.SelectedNode;
            if (betterTreeNode.Offset != -1) Clipboard.SetText(betterTreeNode.ExpectedHash);
        }

        private void LB_ActualHash_DoubleClick(object sender, EventArgs e)
        {
            var betterTreeNode = (BetterTreeNode) tvPartitions.SelectedNode;
            if (betterTreeNode.Offset != -1) Clipboard.SetText(betterTreeNode.ActualHash);
        }

        private void TB_File_DragDrop(object sender, DragEventArgs e)
        {
            if (_backgroundWorker.IsBusy != true)
            {
                var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                tbFile.Text = files[0];
                ProcessFile();
            }
        }

        private void TB_File_DragEnter(object sender, DragEventArgs e)
        {
            if (_backgroundWorker.IsBusy) return;
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var fileName = (string) e.Argument;

            using (var fileStream = File.OpenRead(tbFile.Text))
            {
                using (var fileStream2 = File.OpenWrite(fileName))
                {
                    fileStream.Position = selectedOffset;
                    var buffer = new byte[8192];
                    var num = selectedSize;
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

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _btnExtract.Enabled = true;
            _btnLoadRom.Enabled = true;
            _btnTrimXci.Enabled = true;
            _btnImportCertificate.Enabled = true;
            _btnClearCertificate.Enabled = true;

            if (e.Error != null) MessageBox.Show("Error: " + e.Error.Message);
            else MessageBox.Show("Done extracting NCA!");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }
    }
}