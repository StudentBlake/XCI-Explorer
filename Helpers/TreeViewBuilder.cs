using System.Windows.Forms;

namespace XCI.Explorer.Helpers
{
    public static class TreeViewBuilder
    {
        public static TreeViewFileSystem TreeViewFileSystem;
        public static TreeView TreeView;
        public static readonly BetterTreeNode RootNode;

        static TreeViewBuilder()
        {
            RootNode = new BetterTreeNode("root")
            {
                Offset = -1L,
                Size = -1L
            };
        }

        public static void CreateTreeViewFileSystem(TreeView treeView, TreeViewFileSystem treeViewFileSystem)
        {
            treeView.Nodes.Clear();
            TreeViewFileSystem = new TreeViewFileSystem(treeView);
            treeView.Nodes.Add(RootNode);
            TreeView = treeView;
        }

        public static BetterTreeNode AddNode(string nodeName, BetterTreeNode parent = null)
        {
            var betterTreeNode = new BetterTreeNode(nodeName)
            {
                Offset = -1L,
                Size = -1L
            };
            parent?.Nodes.Add(betterTreeNode);
            return betterTreeNode;
        }

        public static BetterTreeNode AddNode(string nodeName, long offset, long size,
            long hashedRegionSize, string expectedHash, string actualHash)
        {
            return AddNode(nodeName, RootNode, offset, size, hashedRegionSize, expectedHash, actualHash);
        }

        public static BetterTreeNode AddNode(string nodeName, BetterTreeNode parentNode, long offset, long size,
            long hashedRegionSize, string expectedHash, string actualHash)
        {
            return TreeViewFileSystem.AddFile(nodeName, parentNode, offset, size, hashedRegionSize, expectedHash, actualHash);
        }
    }
}