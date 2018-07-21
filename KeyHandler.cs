using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using XCI.Explorer.Helpers;
using XCI.Explorer.Properties;
using XCI.Model;

namespace XCI.Explorer
{
    public class KeyHandler
    {
        public byte[] NcaHeaderEncryptionKey1Prod => Util.HexStringToByteArray(HeaderKey.Remove(32, 32));

        public byte[] NcaHeaderEncryptionKey2Prod => Util.HexStringToByteArray(HeaderKey.Remove(0, 32));

        public string MasterKey = "master_key_";

        private static string HeaderKey => Keys["header_key"].Replace(" ", "");

        private static Dictionary<string,string> Keys
        {
            get
            {
                return (from x in File.ReadAllLines("keys.txt")
                    select x.Split('=')
                    into x
                    where x.Length > 1
                    select x).ToDictionary(x => x[0].Trim(), x => x[1]);
            }
        }

        public KeyHandler()
        {
            CheckRequirements();
        }

        private static void CheckRequirements()
        {
            CheckForKeys();
            CheckForHactool();
        }

        private static void CheckForKeys()
        {
            if (!File.Exists("keys.txt"))
                if (MessageBox.Show(Resources.KeysTxtDownloadPrompt,"XCI Explorer", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    using (var client = new WebClient())
                    {
                        var keys = Encoding.UTF8.GetString(Convert.FromBase64String(Resources.keyUrlAsBase64String));
                        client.DownloadFile(keys, "keys.txt");
                        client.DownloadDataCompleted += Client_DownloadDataCompleted;
                    }
        }

        private static void Client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(Resources.KeysTxtDownloadFailed);
                Environment.Exit(0);
            }

            MessageBox.Show(Resources.DownloadSuccessful, "keys.txt");
        }

        private static void CheckForHactool()
        {
            if (!File.Exists("hactool.exe"))
            {
                MessageBox.Show(Resources.HacToolMissing);
                Environment.Exit(0);
            }
        }


        public bool GetMasterKey(byte masterKeyRev)
        {
            var adjustedRevision = (masterKeyRev - 1).ToString();
            if (masterKeyRev == 0 || masterKeyRev == 1)
            {
                MasterKey += "00";
            }
            else if (masterKeyRev < 17)
            {
                MasterKey = MasterKey + "0" + adjustedRevision;
            }
            else if (masterKeyRev >= 17)
            {
                MasterKey += adjustedRevision;
            }
            try
            {
                MasterKey = Keys[MasterKey].Replace(" ", "");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}