using Be.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace XCI_Explorer {
    partial class CertForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;
        private HexBox hbxHexView;

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

        #endregion
    }
}