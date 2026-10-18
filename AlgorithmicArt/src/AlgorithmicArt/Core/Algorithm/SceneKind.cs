namespace AlgorithmicArt.Core.Algorithm;

/// <summary>场景类型（算法声明需要哪种初始数据）。</summary>
public enum SceneKind
{
    /// <summary>随机数组（排序）。</summary>
    RandomArray,
    /// <summary>有序数组（查找）。</summary>
    SortedArray,
    /// <summary>信号波形（信号/滤波）。</summary>
    Signal,
    /// <summary>栅格地图（扫地）。</summary>
    GridMap,
    /// <summary>参考路径（汽车）。</summary>
    Trajectory,
    /// <summary>阶跃响应（PID 演示）。</summary>
    StepResponse,
    /// <summary>机器学习：多簇散点（聚类）。</summary>
    MlClusters,
    /// <summary>机器学习：沿直线分布的散点（回归）。</summary>
    MlRegression,
    /// <summary>机器学习：两类散点 + 待测点（分类）。</summary>
    MlClassification,
    /// <summary>线性代数：两个方阵相乘。</summary>
    MatrixMultiply,
    /// <summary>线性代数：两个同型矩阵（加/减/数乘）。</summary>
    MatrixArithmetic,
    /// <summary>线性代数：非方阵转置。</summary>
    MatrixTranspose,
    /// <summary>线性代数：可逆方阵求逆。</summary>
    MatrixInverse,
    /// <summary>线性代数：增广矩阵（方程组）。</summary>
    LinearSystem,
    /// <summary>线性代数：LU 分解。</summary>
    LuDecomposition,
    /// <summary>线性代数：2×2 矩阵 + 初始向量（幂迭代）。</summary>
    PowerIteration,
    /// <summary>线性代数：二维散点（PCA）。</summary>
    PcaScatter
}
