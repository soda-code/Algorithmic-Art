namespace AlgorithmicArt.Core.Algorithm;

/// <summary>机器学习场景中的二维样本点（坐标范围 0~100）。</summary>
public class MlPoint
{
    public float X { get; set; }
    public float Y { get; set; }
    /// <summary>真实类别标签（分类场景为 0/1，其余为 -1）。</summary>
    public int Label { get; set; } = -1;

    public MlPoint() { }

    public MlPoint(float x, float y, int label = -1)
    {
        X = x;
        Y = y;
        Label = label;
    }
}

/// <summary>机器学习动画帧：散点 + 簇分配 + 质心 + 拟合直线 + 损失曲线。</summary>
public class MlFrame
{
    public PointF[] Points { get; set; } = Array.Empty<PointF>();
    /// <summary>每个点的真实标签（分类场景使用）。</summary>
    public int[] Labels { get; set; } = Array.Empty<int>();
    /// <summary>每个点当前所属簇（-1 表示未分配）。</summary>
    public int[] ClusterOf { get; set; } = Array.Empty<int>();
    /// <summary>质心坐标（聚类）。</summary>
    public PointF[] Centroids { get; set; } = Array.Empty<PointF>();
    /// <summary>拟合直线两端点（回归）。</summary>
    public PointF[] Line { get; set; } = Array.Empty<PointF>();
    /// <summary>需要高亮的点索引。</summary>
    public int[] Highlight { get; set; } = Array.Empty<int>();
    /// <summary>损失值历史（MSE）。</summary>
    public float[] Loss { get; set; } = Array.Empty<float>();
    /// <summary>待分类查询点。</summary>
    public PointF? Query { get; set; }

    /// <summary>生成一帧快照（每步调用，保证后退时画面正确）。</summary>
    public static MlFrame Snapshot(
        IReadOnlyList<MlPoint> points,
        int[]? clusterOf = null,
        PointF[]? centroids = null,
        PointF[]? line = null,
        int[]? highlight = null,
        float[]? loss = null,
        PointF? query = null)
    {
        var pts = new PointF[points.Count];
        var labels = new int[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            pts[i] = new PointF(points[i].X, points[i].Y);
            labels[i] = points[i].Label;
        }

        return Snapshot(pts, labels, clusterOf, centroids, line, highlight, loss, query);
    }

    /// <summary>
    /// 高效快照：点集与标签可复用同一数组实例（算法运行期间不变），
    /// 只复制每步会变化的簇分配 / 质心等数据，显著降低内存占用。
    /// </summary>
    public static MlFrame Snapshot(
        PointF[] points,
        int[] labels,
        int[]? clusterOf = null,
        PointF[]? centroids = null,
        PointF[]? line = null,
        int[]? highlight = null,
        float[]? loss = null,
        PointF? query = null)
    {
        return new MlFrame
        {
            Points = points,
            Labels = labels,
            ClusterOf = clusterOf ?? Unassigned(points.Length),
            Centroids = centroids ?? Array.Empty<PointF>(),
            Line = line ?? Array.Empty<PointF>(),
            Highlight = highlight ?? Array.Empty<int>(),
            Loss = loss ?? Array.Empty<float>(),
            Query = query
        };
    }

    private static int[] Unassigned(int count)
    {
        if (count <= 0) return Array.Empty<int>();
        var arr = new int[count];
        Array.Fill(arr, -1);
        return arr;
    }
}
