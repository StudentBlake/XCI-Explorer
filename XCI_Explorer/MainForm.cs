using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using XCI_Explorer.Helpers;
using XTSSharp;

namespace XCI_Explorer
{
	public class MainForm : Form
	{
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

		private long metadataNcaOffset;

		private long metadataNcaSize;

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

		private Button B_TrimXCI;

		public MainForm()
		{
			InitializeComponent();
			LB_SelectedData.Text = "";
			LB_DataOffset.Text = "";
			LB_DataSize.Text = "";
			if (!File.Exists("keys.txt"))
			{
				MessageBox.Show("keys.txt is missing.");
				Environment.Exit(0);
			}
			if (!File.Exists("hactool.exe"))
			{
				MessageBox.Show("hactool is missing.");
				Environment.Exit(0);
			}
			getKey();
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

		private void B_LoadROM_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Switch XCI (*.xci)|*.xci|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				TB_File.Text = openFileDialog.FileName;
				if (CheckXCI())
				{
					LoadXCI();
				}
				else
				{
					TB_File.Text = null;
					MessageBox.Show("Unsupported file.");
				}
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
			LoadPartitons();
			LoadNCAData();
			LoadGameInfos();
		}

		private void LoadGameInfos()
		{
            bool msgFlag = false;
            CB_RegionName.Items.Clear();
			TB_Name.Text = "";
			TB_Dev.Text = "";
			PB_GameIcon.BackgroundImage = null;
			if (getMKey())
			{
				using (FileStream fileStream = File.OpenRead(TB_File.Text))
				{
					using (FileStream fileStream2 = File.OpenWrite("meta"))
					{
						BinaryReader binaryReader = new BinaryReader(fileStream);
						BinaryWriter binaryWriter = new BinaryWriter(fileStream2);
						fileStream.Position = metadataNcaOffset;
						byte[] buffer = new byte[8192];
						long num = metadataNcaSize;
						int num2;
						while ((num2 = fileStream.Read(buffer, 0, 8192)) > 0 && num > 0)
						{
							fileStream2.Write(buffer, 0, num2);
							num -= num2;
						}
						fileStream.Close();
						binaryReader.Close();
						binaryWriter.Close();
					}
				}
				if (File.Exists("meta"))
				{
					Process process = new Process();
					process.StartInfo = new ProcessStartInfo
					{
						WindowStyle = ProcessWindowStyle.Hidden,
						FileName = "hactool.exe",
						Arguments = "-k keys.txt --romfsdir=data meta"
					};
					process.Start();
					process.WaitForExit();
					if (File.Exists("data\\control.nacp"))
					{
						byte[] source = File.ReadAllBytes("data\\control.nacp");
						NACP.NACP_Datas[0] = new NACP.NACP_Data(source.Skip(12288).Take(4096).ToArray());
						for (int i = 0; i < NACP.NACP_Strings.Length; i++)
						{
							NACP.NACP_Strings[i] = new NACP.NACP_String(source.Skip(i * 768).Take(768).ToArray());
							if (NACP.NACP_Strings[i].Check != 0)
							{
								CB_RegionName.Items.Add(Language[i]);
                                try
                                {
                                    using (Bitmap original = new Bitmap("data\\icon_" + Language[i].Replace(" ", "") + ".dat"))
                                    {
                                        Icons[i] = new Bitmap(original);
                                    }
                                }
                                catch
                                {
                                    // using bad coding coding practices as a temporary fix until someone can figure out the problem
                                    msgFlag = true;
                                }
								PB_GameIcon.BackgroundImage = Icons[i];
							}
						}
						TB_GameRev.Text = NACP.NACP_Datas[0].GameVer;
						TB_ProdCode.Text = NACP.NACP_Datas[0].GameProd;
						if (TB_ProdCode.Text == "")
						{
							TB_ProdCode.Text = "No Prod. ID";
						}
						File.Delete("meta");
						Directory.Delete("data", true);
					}
					CB_RegionName.SelectedIndex = 0;
				}
			}
			else
			{
				TB_Dev.Text = Mkey + " not found";
				TB_Name.Text = Mkey + " not found";
			}
            if (msgFlag)
            {
                MessageBox.Show("This XCI may not support trim/extract functions [LOGO]");
            }
        }

		private void LoadNCAData()
		{
			NCA.NCA_Headers[0] = new NCA.NCA_Header(DecryptNCAHeader(gameNcaOffset));
			TB_TID.Text = NCA.NCA_Headers[0].TitleID.ToString("X");
			TB_SDKVer.Text = $"{NCA.NCA_Headers[0].SDKVersion4}.{NCA.NCA_Headers[0].SDKVersion3}.{NCA.NCA_Headers[0].SDKVersion2}.{NCA.NCA_Headers[0].SDKVersion1}";
			TB_MKeyRev.Text = Util.GetMkey(NCA.NCA_Headers[0].MasterKeyRev);
		}

		private void LoadPartitons()
		{
            bool msgFlag = false;
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
				TV_Parti.AddFile(array[i].Name + ".hfs0", rootNode, num + array[i].Offset, array[i].Size);
				BetterTreeNode betterTreeNode = TV_Parti.AddDir(array[i].Name, rootNode);
				HFS0.HFS0_Header[] array5 = new HFS0.HFS0_Header[1];
				fileStream.Position = array[i].Offset + num;
				fileStream.Read(array3, 0, 16);
				array5[0] = new HFS0.HFS0_Header(array3);
				if (array[i].Name == "secure")
				{
					SecureSize = new long[array5[0].FileCount];
					SecureOffset = new long[array5[0].FileCount];
				}
				if (array[i].Name == "normal")
				{
					NormalSize = new long[array5[0].FileCount];
					NormalOffset = new long[array5[0].FileCount];
				}
				HFS0.HSF0_Entry[] array6 = new HFS0.HSF0_Entry[array5[0].FileCount];
				for (int j = 0; j < array5[0].FileCount; j++)
				{
					fileStream.Position = array[i].Offset + num + 16 + 64 * j;
					fileStream.Read(array2, 0, 64);
					array6[i] = new HFS0.HSF0_Entry(array2);
					fileStream.Position = array[i].Offset + num + 16 + 64 * array5[0].FileCount + array6[i].Name_ptr;
					if (array[i].Name == "secure")
					{
						SecureSize[j] = array6[i].Size;
						SecureOffset[j] = array[i].Offset + array6[i].Offset + num + 16 + array5[0].StringTableSize + array5[0].FileCount * 64;
					}
					if (array[i].Name == "normal")
					{
						NormalSize[j] = array6[i].Size;
						NormalOffset[j] = array[i].Offset + array6[i].Offset + num + 16 + array5[0].StringTableSize + array5[0].FileCount * 64;
					}
					while ((num2 = fileStream.ReadByte()) != 0 && num2 != 0)
					{
						chars.Add((char)num2);
					}
					array6[i].Name = new string(chars.ToArray());
					chars.Clear();
					TV_Parti.AddFile(array6[i].Name, betterTreeNode, array[i].Offset + array6[i].Offset + num + 16 + array5[0].StringTableSize + array5[0].FileCount * 64, array6[i].Size);
					TreeNode[] array7 = TV_Partitions.Nodes.Find(betterTreeNode.Text, true);
					if (array7.Length != 0)
					{
						TV_Parti.AddFile(array6[i].Name, (BetterTreeNode)array7[0], 0L, 0L);
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
					num3 = SecureOffset[k];
				}
			}
			num3 = -9223372036854775808L;
			for (int l = 0; l < NormalSize.Length; l++)
			{
				if (NormalSize[l] > num3)
				{
					metadataNcaSize = NormalSize[l];
					metadataNcaOffset = NormalOffset[l];
					num3 = NormalSize[l];
				}
			}
			PFS0Offset = gameNcaOffset + 32768;
			fileStream.Position = PFS0Offset;
			fileStream.Read(array3, 0, 16);
			PFS0.PFS0_Headers[0] = new PFS0.PFS0_Header(array3);
            PFS0.PFS0_Entry[] array8;
            try
            {
                array8 = new PFS0.PFS0_Entry[PFS0.PFS0_Headers[0].FileCount];
            }
            catch
            {
                // another temporary fix until someone can understand the real problem
                // I am just taking shots in the dark using any method to get it not to crash
                array8 = new PFS0.PFS0_Entry[0];
                msgFlag = true;
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
			fileStream.Close();
            if (msgFlag)
            {
                MessageBox.Show("This XCI may not support trim/extract functions [PARTITION]");
            }
        }

		private void TV_Partitions_AfterSelect(object sender, TreeViewEventArgs e)
		{
			BetterTreeNode betterTreeNode = (BetterTreeNode)TV_Partitions.SelectedNode;
			if (betterTreeNode.Offset != -1)
			{
				selectedOffset = betterTreeNode.Offset;
				selectedSize = betterTreeNode.Size;
				LB_DataOffset.Text = "Offset: 0x" + selectedOffset.ToString("X");
				LB_SelectedData.Text = e.Node.Text;
				B_Extract.Enabled = true;
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
			}
			else
			{
				LB_SelectedData.Text = "";
				LB_DataOffset.Text = "";
				LB_DataSize.Text = "";
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
					MessageBox.Show("cert successfully imported from:\n\n" + openFileDialog.FileName);
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
				new CertForm(this).Show();
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
						MessageBox.Show("cert deleted.");
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
				using (FileStream fileStream = File.OpenRead(TB_File.Text))
				{
					using (FileStream fileStream2 = File.OpenWrite(saveFileDialog.FileName))
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
			PB_GameIcon.BackgroundImage = Icons[num];
			TB_Name.Text = NACP.NACP_Strings[num].GameName;
			TB_Dev.Text = NACP.NACP_Strings[num].GameAuthor;
		}

		private void B_TrimXCI_Click(object sender, EventArgs e)
		{
			if (Util.checkFile(TB_File.Text))
			{
				if (MessageBox.Show("Trim XCI ?", "XCI Explorer", MessageBoxButtons.YesNo) == DialogResult.Yes)
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

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			B_LoadROM = new Button();
			TB_File = new TextBox();
			TABC_Main = new TabControl();
			TABP_XCI = new TabPage();
			B_TrimXCI = new Button();
			TB_ProdCode = new TextBox();
			label8 = new Label();
			groupBox2 = new GroupBox();
			TB_Dev = new TextBox();
			label10 = new Label();
			TB_Name = new TextBox();
			label9 = new Label();
			PB_GameIcon = new PictureBox();
			CB_RegionName = new ComboBox();
			TB_GameRev = new TextBox();
			label7 = new Label();
			groupBox1 = new GroupBox();
			B_ViewCert = new Button();
			B_ClearCert = new Button();
			B_ImportCert = new Button();
			B_ExportCert = new Button();
			TB_ExactUsedSpace = new TextBox();
			TB_ROMExactSize = new TextBox();
			TB_UsedSpace = new TextBox();
			TB_ROMSize = new TextBox();
			label6 = new Label();
			label5 = new Label();
			TB_MKeyRev = new TextBox();
			label4 = new Label();
			TB_SDKVer = new TextBox();
			label3 = new Label();
			TB_Capacity = new TextBox();
			label2 = new Label();
			label1 = new Label();
			TB_TID = new TextBox();
			tabPage2 = new TabPage();
			B_Extract = new Button();
			LB_DataSize = new Label();
			LB_DataOffset = new Label();
			LB_SelectedData = new Label();
			TV_Partitions = new TreeView();
			TABC_Main.SuspendLayout();
			TABP_XCI.SuspendLayout();
			groupBox2.SuspendLayout();
			((ISupportInitialize)PB_GameIcon).BeginInit();
			groupBox1.SuspendLayout();
			tabPage2.SuspendLayout();
			SuspendLayout();
			B_LoadROM.Location = new Point(4, 12);
			B_LoadROM.Name = "B_LoadROM";
			B_LoadROM.Size = new Size(75, 23);
			B_LoadROM.TabIndex = 0;
			B_LoadROM.Text = "Load XCI";
			B_LoadROM.UseVisualStyleBackColor = true;
			B_LoadROM.Click += B_LoadROM_Click;
			TB_File.Location = new Point(85, 13);
			TB_File.Name = "TB_File";
			TB_File.ReadOnly = true;
			TB_File.Size = new Size(258, 20);
			TB_File.TabIndex = 1;
			TABC_Main.Controls.Add(TABP_XCI);
			TABC_Main.Controls.Add(tabPage2);
			TABC_Main.Location = new Point(4, 41);
			TABC_Main.Name = "TABC_Main";
			TABC_Main.SelectedIndex = 0;
			TABC_Main.Size = new Size(355, 485);
			TABC_Main.TabIndex = 2;
			TABP_XCI.Controls.Add(B_TrimXCI);
			TABP_XCI.Controls.Add(TB_ProdCode);
			TABP_XCI.Controls.Add(label8);
			TABP_XCI.Controls.Add(groupBox2);
			TABP_XCI.Controls.Add(TB_GameRev);
			TABP_XCI.Controls.Add(label7);
			TABP_XCI.Controls.Add(groupBox1);
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
			TABP_XCI.Text = "XCI";
			TABP_XCI.UseVisualStyleBackColor = true;
			B_TrimXCI.Location = new Point(90, 207);
			B_TrimXCI.Name = "B_TrimXCI";
			B_TrimXCI.Size = new Size(70, 23);
			B_TrimXCI.TabIndex = 21;
			B_TrimXCI.Text = "Trim XCI";
			B_TrimXCI.UseVisualStyleBackColor = true;
			B_TrimXCI.Click += B_TrimXCI_Click;
			TB_ProdCode.Location = new Point(238, 115);
			TB_ProdCode.Name = "TB_ProdCode";
			TB_ProdCode.ReadOnly = true;
			TB_ProdCode.Size = new Size(69, 20);
			TB_ProdCode.TabIndex = 20;
			label8.AutoSize = true;
			label8.Location = new Point(235, 99);
			label8.Name = "label8";
			label8.Size = new Size(75, 13);
			label8.TabIndex = 19;
			label8.Text = "Product Code:";
			groupBox2.Controls.Add(TB_Dev);
			groupBox2.Controls.Add(label10);
			groupBox2.Controls.Add(TB_Name);
			groupBox2.Controls.Add(label9);
			groupBox2.Controls.Add(PB_GameIcon);
			groupBox2.Controls.Add(CB_RegionName);
			groupBox2.Location = new Point(22, 296);
			groupBox2.Name = "groupBox2";
			groupBox2.Size = new Size(301, 154);
			groupBox2.TabIndex = 18;
			groupBox2.TabStop = false;
			groupBox2.Text = "Game Infos";
			TB_Dev.Location = new Point(6, 117);
			TB_Dev.Name = "TB_Dev";
			TB_Dev.ReadOnly = true;
			TB_Dev.Size = new Size(145, 20);
			TB_Dev.TabIndex = 24;
			label10.AutoSize = true;
			label10.Location = new Point(3, 101);
			label10.Name = "label10";
			label10.Size = new Size(59, 13);
			label10.TabIndex = 23;
			label10.Text = "Developer:";
			TB_Name.Location = new Point(6, 66);
			TB_Name.Name = "TB_Name";
			TB_Name.ReadOnly = true;
			TB_Name.Size = new Size(145, 20);
			TB_Name.TabIndex = 22;
			label9.AutoSize = true;
			label9.Location = new Point(3, 50);
			label9.Name = "label9";
			label9.Size = new Size(38, 13);
			label9.TabIndex = 21;
			label9.Text = "Name:";
			PB_GameIcon.BackgroundImageLayout = ImageLayout.Zoom;
			PB_GameIcon.Location = new Point(190, 43);
			PB_GameIcon.Name = "PB_GameIcon";
			PB_GameIcon.Size = new Size(105, 105);
			PB_GameIcon.TabIndex = 18;
			PB_GameIcon.TabStop = false;
			CB_RegionName.DropDownStyle = ComboBoxStyle.DropDownList;
			CB_RegionName.FormattingEnabled = true;
			CB_RegionName.Location = new Point(77, 14);
			CB_RegionName.Name = "CB_RegionName";
			CB_RegionName.Size = new Size(138, 21);
			CB_RegionName.TabIndex = 17;
			CB_RegionName.SelectedIndexChanged += CB_RegionName_SelectedIndexChanged;
			TB_GameRev.Location = new Point(24, 115);
			TB_GameRev.Name = "TB_GameRev";
			TB_GameRev.ReadOnly = true;
			TB_GameRev.Size = new Size(124, 20);
			TB_GameRev.TabIndex = 16;
			label7.AutoSize = true;
			label7.Location = new Point(21, 99);
			label7.Name = "label7";
			label7.Size = new Size(82, 13);
			label7.TabIndex = 15;
			label7.Text = "Game Revision:";
			groupBox1.Controls.Add(B_ViewCert);
			groupBox1.Controls.Add(B_ClearCert);
			groupBox1.Controls.Add(B_ImportCert);
			groupBox1.Controls.Add(B_ExportCert);
			groupBox1.Location = new Point(22, 234);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(301, 52);
			groupBox1.TabIndex = 14;
			groupBox1.TabStop = false;
			groupBox1.Text = "Cert";
			B_ViewCert.Location = new Point(156, 19);
			B_ViewCert.Name = "B_ViewCert";
			B_ViewCert.Size = new Size(66, 23);
			B_ViewCert.TabIndex = 3;
			B_ViewCert.Text = "View Cert";
			B_ViewCert.UseVisualStyleBackColor = true;
			B_ViewCert.Click += B_ViewCert_Click;
			B_ClearCert.Location = new Point(229, 19);
			B_ClearCert.Name = "B_ClearCert";
			B_ClearCert.Size = new Size(66, 23);
			B_ClearCert.TabIndex = 2;
			B_ClearCert.Text = "Clear Cert";
			B_ClearCert.UseVisualStyleBackColor = true;
			B_ClearCert.Click += B_ClearCert_Click;
			B_ImportCert.Location = new Point(83, 19);
			B_ImportCert.Name = "B_ImportCert";
			B_ImportCert.Size = new Size(67, 23);
			B_ImportCert.TabIndex = 1;
			B_ImportCert.Text = "Import Cert";
			B_ImportCert.UseVisualStyleBackColor = true;
			B_ImportCert.Click += B_ImportCert_Click;
			B_ExportCert.Location = new Point(7, 19);
			B_ExportCert.Name = "B_ExportCert";
			B_ExportCert.Size = new Size(70, 23);
			B_ExportCert.TabIndex = 0;
			B_ExportCert.Text = "Export Cert";
			B_ExportCert.UseVisualStyleBackColor = true;
			B_ExportCert.Click += B_ExportCert_Click;
			TB_ExactUsedSpace.Location = new Point(166, 181);
			TB_ExactUsedSpace.Name = "TB_ExactUsedSpace";
			TB_ExactUsedSpace.ReadOnly = true;
			TB_ExactUsedSpace.Size = new Size(157, 20);
			TB_ExactUsedSpace.TabIndex = 13;
			TB_ROMExactSize.Location = new Point(166, 154);
			TB_ROMExactSize.Name = "TB_ROMExactSize";
			TB_ROMExactSize.ReadOnly = true;
			TB_ROMExactSize.Size = new Size(157, 20);
			TB_ROMExactSize.TabIndex = 12;
			TB_UsedSpace.Location = new Point(91, 181);
			TB_UsedSpace.Name = "TB_UsedSpace";
			TB_UsedSpace.ReadOnly = true;
			TB_UsedSpace.Size = new Size(69, 20);
			TB_UsedSpace.TabIndex = 11;
			TB_ROMSize.Location = new Point(91, 154);
			TB_ROMSize.Name = "TB_ROMSize";
			TB_ROMSize.ReadOnly = true;
			TB_ROMSize.Size = new Size(69, 20);
			TB_ROMSize.TabIndex = 10;
			label6.AutoSize = true;
			label6.Location = new Point(18, 181);
			label6.Name = "label6";
			label6.Size = new Size(67, 13);
			label6.TabIndex = 9;
			label6.Text = "Used space:";
			label5.AutoSize = true;
			label5.Location = new Point(27, 157);
			label5.Name = "label5";
			label5.Size = new Size(58, 13);
			label5.TabIndex = 8;
			label5.Text = "ROM Size:";
			TB_MKeyRev.Location = new Point(24, 68);
			TB_MKeyRev.Name = "TB_MKeyRev";
			TB_MKeyRev.ReadOnly = true;
			TB_MKeyRev.Size = new Size(124, 20);
			TB_MKeyRev.TabIndex = 7;
			label4.AutoSize = true;
			label4.Location = new Point(21, 52);
			label4.Name = "label4";
			label4.Size = new Size(104, 13);
			label4.TabIndex = 6;
			label4.Text = "MasterKey Revision:";
			TB_SDKVer.Location = new Point(238, 68);
			TB_SDKVer.Name = "TB_SDKVer";
			TB_SDKVer.ReadOnly = true;
			TB_SDKVer.Size = new Size(69, 20);
			TB_SDKVer.TabIndex = 5;
			label3.AutoSize = true;
			label3.Location = new Point(235, 52);
			label3.Name = "label3";
			label3.Size = new Size(70, 13);
			label3.TabIndex = 4;
			label3.Text = "SDK Version:";
			TB_Capacity.Location = new Point(238, 23);
			TB_Capacity.Name = "TB_Capacity";
			TB_Capacity.ReadOnly = true;
			TB_Capacity.Size = new Size(69, 20);
			TB_Capacity.TabIndex = 3;
			label2.AutoSize = true;
			label2.Location = new Point(235, 7);
			label2.Name = "label2";
			label2.Size = new Size(51, 13);
			label2.TabIndex = 2;
			label2.Text = "Capacity:";
			label1.AutoSize = true;
			label1.Location = new Point(21, 7);
			label1.Name = "label1";
			label1.Size = new Size(44, 13);
			label1.TabIndex = 1;
			label1.Text = "Title ID:";
			TB_TID.Location = new Point(24, 23);
			TB_TID.Name = "TB_TID";
			TB_TID.ReadOnly = true;
			TB_TID.Size = new Size(124, 20);
			TB_TID.TabIndex = 0;
			tabPage2.Controls.Add(B_Extract);
			tabPage2.Controls.Add(LB_DataSize);
			tabPage2.Controls.Add(LB_DataOffset);
			tabPage2.Controls.Add(LB_SelectedData);
			tabPage2.Controls.Add(TV_Partitions);
			tabPage2.Location = new Point(4, 22);
			tabPage2.Name = "tabPage2";
			tabPage2.Padding = new Padding(3);
			tabPage2.Size = new Size(347, 459);
			tabPage2.TabIndex = 1;
			tabPage2.Text = "Partitions";
			tabPage2.UseVisualStyleBackColor = true;
			B_Extract.Enabled = false;
			B_Extract.Location = new Point(296, 367);
			B_Extract.Name = "B_Extract";
			B_Extract.Size = new Size(48, 23);
			B_Extract.TabIndex = 4;
			B_Extract.Text = "Extract";
			B_Extract.UseVisualStyleBackColor = true;
			B_Extract.Click += B_Extract_Click;
			LB_DataSize.AutoSize = true;
			LB_DataSize.Location = new Point(6, 403);
			LB_DataSize.Name = "LB_DataSize";
			LB_DataSize.Size = new Size(30, 13);
			LB_DataSize.TabIndex = 3;
			LB_DataSize.Text = "Size:";
			LB_DataOffset.AutoSize = true;
			LB_DataOffset.Location = new Point(6, 390);
			LB_DataOffset.Name = "LB_DataOffset";
			LB_DataOffset.Size = new Size(38, 13);
			LB_DataOffset.TabIndex = 2;
			LB_DataOffset.Text = "Offset:";
			LB_SelectedData.AutoSize = true;
			LB_SelectedData.Location = new Point(6, 367);
			LB_SelectedData.Name = "LB_SelectedData";
			LB_SelectedData.Size = new Size(51, 13);
			LB_SelectedData.TabIndex = 1;
			LB_SelectedData.Text = "FileName";
			TV_Partitions.Dock = DockStyle.Top;
			TV_Partitions.Location = new Point(3, 3);
			TV_Partitions.Name = "TV_Partitions";
			TV_Partitions.Size = new Size(341, 361);
			TV_Partitions.TabIndex = 0;
			TV_Partitions.AfterSelect += TV_Partitions_AfterSelect;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(362, 529);
			base.Controls.Add(TABC_Main);
			base.Controls.Add(TB_File);
			base.Controls.Add(B_LoadROM);
			base.FormBorderStyle = FormBorderStyle.FixedSingle;
			base.Name = "MainForm";
			base.ShowIcon = false;
			Text = "XCI Explorer";
			TABC_Main.ResumeLayout(false);
			TABP_XCI.ResumeLayout(false);
			TABP_XCI.PerformLayout();
			groupBox2.ResumeLayout(false);
			groupBox2.PerformLayout();
			((ISupportInitialize)PB_GameIcon).EndInit();
			groupBox1.ResumeLayout(false);
			tabPage2.ResumeLayout(false);
			tabPage2.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
