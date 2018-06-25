using System.Windows.Forms;
using XCI_Explorer.Helpers;

namespace XCI_Explorer
{
    public class TreeViewFileSystem
    {
        public TreeView treeView;

        public TreeViewFileSystem(TreeView tv)
        {
        }

        public BetterTreeNode AddDir(string name, BetterTreeNode parent = null)
        {
            BetterTreeNode betterTreeNode = new BetterTreeNode(name);
            betterTreeNode.Offset = -1L;
            betterTreeNode.Size = -1L;
            parent.Nodes.Add(betterTreeNode);
            return betterTreeNode;
        }

        public BetterTreeNode AddFile(string name, BetterTreeNode parent, long offset, long size)
        {
            return AddFile(name, parent, offset, size, 0, "", "");
        }

        public BetterTreeNode AddFile(string name, BetterTreeNode parent, long offset, long size, long HashedRegionSize, string ExpectedHash, string ActualHash)
        {
            BetterTreeNode betterTreeNode = new BetterTreeNode(name);
            betterTreeNode.Offset = offset;
            betterTreeNode.Size = size;
            betterTreeNode.ExpectedHash = ExpectedHash;
            betterTreeNode.ActualHash = ActualHash;
            betterTreeNode.HashedRegionSize = HashedRegionSize;
            parent.Nodes.Add(betterTreeNode);
            return betterTreeNode;
        }
    }
}
