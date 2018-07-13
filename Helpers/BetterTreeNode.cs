using System.Windows.Forms;

namespace Helpers
{
    public class BetterTreeNode : TreeNode
    {
        public string ActualHash;
        public string ExpectedHash;
        public long HashedRegionSize;
        public long Offset;
        public long Size;

        public BetterTreeNode(string t)
        {
            Text = t;
        }
    }
}