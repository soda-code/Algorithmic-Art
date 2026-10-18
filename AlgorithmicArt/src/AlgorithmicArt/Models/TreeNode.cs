namespace AlgorithmicArt.Models;

/// <summary>二叉搜索树节点。</summary>
public class TreeNode
{
    public int Value { get; set; }
    public TreeNode? Left { get; set; }
    public TreeNode? Right { get; set; }

    public TreeNode(int value) => Value = value;
}
