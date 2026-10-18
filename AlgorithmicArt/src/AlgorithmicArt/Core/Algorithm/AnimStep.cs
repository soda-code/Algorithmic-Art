namespace AlgorithmicArt.Core.Algorithm;

/// <summary>动画步骤（统一，兼容多类算法）。</summary>
public class AnimStep
{
    public AnimOp Op { get; set; }
    public int[] Indices { get; set; } = Array.Empty<int>();
    public PointF[] Points { get; set; } = Array.Empty<PointF>();
    public object? Extra { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Sequence { get; set; }
    /// <summary>对应伪代码行号（-1 表示无对应行）。</summary>
    public int CodeLine { get; set; } = -1;
    /// <summary>已排序区间起点（-1 表示无）。</summary>
    public int SortedLow { get; set; } = -1;
    /// <summary>已排序区间终点（闭区间）。</summary>
    public int SortedHigh { get; set; } = -1;
}
