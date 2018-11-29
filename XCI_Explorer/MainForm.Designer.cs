using System.ComponentModel;
using System.Windows.Forms;
using XCI_Explorer.Helpers;

namespace XCI_Explorer {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
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
            this.MaximizeBox = false;
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

        #endregion

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
        private TreeViewFileSystem TV_Parti;
        private BetterTreeNode rootNode;
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
    }
}