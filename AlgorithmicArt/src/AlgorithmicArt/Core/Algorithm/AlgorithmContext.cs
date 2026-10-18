namespace AlgorithmicArt.Core.Algorithm;

/// <summary>算法场景上下文（不同算法携带不同场景数据）。</summary>
public class AlgorithmContext
{
    public int[]? Array { get; set; }
    public double[]? Signal { get; set; }
    public Models.GridMap? Map { get; set; }
    public Models.Graph? Graph { get; set; }
    public Models.TreeNode? Tree { get; set; }
    public int[][]? DpTable { get; set; }
    public Models.RobotPose Pose { get; set; }

    /// <summary>参考路径（汽车）。</summary>
    public List<PointF>? Path { get; set; }
    /// <summary>实际轨迹（汽车）。</summary>
    public List<PointF>? Trace { get; set; }

    /// <summary>机器学习样本点（0~100 坐标系）。</summary>
    public MlPoint[]? MlPoints { get; set; }
    /// <summary>机器学习待分类查询点。</summary>
    public PointF? QueryPoint { get; set; }
    /// <summary>初始簇分配（-1 表示未分配）。</summary>
    public int[]? ClusterOf { get; set; }
    /// <summary>初始质心。</summary>
    public PointF[]? Centroids { get; set; }

    /// <summary>线性代数：主矩阵。</summary>
    public double[,]? MatrixA { get; set; }
    /// <summary>线性代数：第二矩阵（乘法右操作数）。</summary>
    public double[,]? MatrixB { get; set; }
    /// <summary>线性代数：右端向量 / 初始向量。</summary>
    public double[]? Vector { get; set; }

    /// <summary>算法参数。</summary>
    public Dictionary<string, double> Parameters { get; } = new();
}
