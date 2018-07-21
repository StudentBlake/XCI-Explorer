using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Be.Windows.Forms;

namespace XCI.Explorer.Forms
{
    public class CertificateView : Form
    {
        private readonly IContainer components;
        private HexBox _hbxHexView;

        public CertificateView()
        {
        }

        public CertificateView(MainForm mainForm, IContainer components)
        {
            this.components = components;
            InitializeComponent();
            var fileStream = new FileStream(mainForm.TB_File.Text, FileMode.Open, FileAccess.Read);
            var array = new byte[512];
            fileStream.Position = 28672L;
            fileStream.Read(array, 0, 512);
            _hbxHexView.ByteProvider = new DynamicByteProvider(array);
            fileStream.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _hbxHexView = new HexBox();
            SuspendLayout();
            _hbxHexView.Dock = DockStyle.Fill;
            _hbxHexView.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            _hbxHexView.Location = new Point(0, 0);
            _hbxHexView.Margin = new Padding(4);
            _hbxHexView.Name = "_hbxHexView";
            _hbxHexView.ReadOnly = true;
            _hbxHexView.ShadowSelectionColor = Color.FromArgb(100, 60, 188, 255);
            _hbxHexView.Size = new Size(573, 256);
            _hbxHexView.StringViewVisible = true;
            _hbxHexView.TabIndex = 7;
            _hbxHexView.UseFixedBytesPerLine = true;
            _hbxHexView.VScrollBarVisible = true;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(573, 256);
            Controls.Add(_hbxHexView);
            Name = "CertificateView";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Certificate Data";
            ResumeLayout(false);
        }
    }
}