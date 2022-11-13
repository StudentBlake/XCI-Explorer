using System.Windows.Forms;
using XCI_Explorer.Helpers;

namespace XCI_Explorer;

public class TreeViewFileSystem
{
    public TreeView treeView;

    public TreeViewFileSystem(TreeView tv)
    {
    }

    public BetterTreeNode AddDir(string name, BetterTreeNode parent = null)
    {
        BetterTreeNode betterTreeNode = new(name)
        {
            Offset = -1L,
            Size = -1L
        };
        parent.Nodes.Add(betterTreeNode);
        return betterTreeNode;
    }

    public BetterTreeNode AddFile(string name, BetterTreeNode parent, long offset, long size) => AddFile(name, parent, offset, size, 0, "", "");

    public BetterTreeNode AddFile(string name, BetterTreeNode parent, long offset, long size, long HashedRegionSize, string ExpectedHash, string ActualHash)
    {
        BetterTreeNode betterTreeNode = new(name)
        {
            Offset = offset,
            Size = size,
            ExpectedHash = ExpectedHash,
            ActualHash = ActualHash,
            HashedRegionSize = HashedRegionSize
        };
        parent.Nodes.Add(betterTreeNode);
        return betterTreeNode;
    }
}
