using System.Windows.Forms;

namespace Helpers
{
    public class TreeViewFileSystem
    {
        public TreeView TreeView;

        public TreeViewFileSystem(TreeView tv)
        {
        }

        public BetterTreeNode AddDir(string name, BetterTreeNode parent = null)
        {
            var betterTreeNode = new BetterTreeNode(name)
            {
                Offset = -1L,
                Size = -1L
            };
            parent?.Nodes.Add(betterTreeNode);
            return betterTreeNode;
        }

        public BetterTreeNode AddFile(string name, BetterTreeNode parent, long offset, long size)
        {
            return AddFile(name, parent, offset, size, 0, "", "");
        }

        public BetterTreeNode AddFile(string name, BetterTreeNode parent, long offset, long size, long hashedRegionSize,
            string expectedHash, string actualHash)
        {
            var betterTreeNode = new BetterTreeNode(name)
            {
                Offset = offset,
                Size = size,
                ExpectedHash = expectedHash,
                ActualHash = actualHash,
                HashedRegionSize = hashedRegionSize
            };
            parent.Nodes.Add(betterTreeNode);
            return betterTreeNode;
        }
    }
}