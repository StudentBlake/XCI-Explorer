using System.Drawing;
using System.Windows.Forms;
using XCI.Explorer.Helpers;

namespace XCI.Explorer.Loader
{
    internal interface ILoader
    {
        void GetRomSize(string filePath);
        void LoadPartitions(string filePath, TreeViewFileSystem tvFileSystem, BetterTreeNode rootNode, TreeView tvPartitions);
        void LoadNca(TextBox TB_TID, TextBox TB_SDKVer, TextBox TB_MKeyRev, string filepath);
        void LoadInfos(TextBox TB_Name, TextBox TB_Dev, PictureBox PB_GameIcon, string filepath, ComboBox _cbRegionName, Image[] _icons, TextBox TB_GameRev, TextBox TB_ProdCode);
    }
}