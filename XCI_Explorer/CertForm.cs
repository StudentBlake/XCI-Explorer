using Be.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace XCI_Explorer {
    public class CertForm : Form {
        private IContainer components;
        private HexBox hbxHexView;

        public CertForm(MainForm mainForm) {
            InitializeComponent();
            FileStream fileStream = new FileStream(mainForm.TB_File.Text, FileMode.Open, FileAccess.Read);
            byte[] array = new byte[512];
            fileStream.Position = 28672L;
            fileStream.Read(array, 0, 512);
            hbxHexView.ByteProvider = new DynamicByteProvider(array);
            fileStream.Close();
        }

        protected override void Dispose(bool disposing) {
            if (disposing && components != null) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            hbxHexView = new HexBox();
            SuspendLayout();
            hbxHexView.Dock = DockStyle.Fill;
            hbxHexView.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            hbxHexView.Location = new Point(0, 0);
            hbxHexView.Margin = new Padding(4);
            hbxHexView.Name = "hbxHexView";
            hbxHexView.ReadOnly = true;
            hbxHexView.ShadowSelectionColor = Color.FromArgb(100, 60, 188, 255);
            hbxHexView.Size = new Size(573, 256);
            hbxHexView.StringViewVisible = true;
            hbxHexView.TabIndex = 7;
            hbxHexView.UseFixedBytesPerLine = true;
            hbxHexView.VScrollBarVisible = true;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(573, 256);
            base.Controls.Add(hbxHexView);
            base.Name = "CertForm";
            base.ShowIcon = false;
            base.StartPosition = FormStartPosition.CenterScreen;
            Text = "Cert Data";
            ResumeLayout(false);
        }
    }
}
