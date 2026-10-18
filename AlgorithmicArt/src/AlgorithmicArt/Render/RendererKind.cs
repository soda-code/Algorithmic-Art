namespace AlgorithmicArt.Render;

/// <summary>渲染器类型。</summary>
public enum RendererKind
{
    Waveform,
    Spectrum,
    GridMap,
    Trajectory,
    Chart,
    Graph,
    Tree,
    Table,
    /// <summary>PID 循迹：轨迹 + 误差/PID 分量曲线。</summary>
    Pid,
    /// <summary>PID 阶跃响应曲线。</summary>
    PidStep,
    /// <summary>P/PI/PID 对比曲线。</summary>
    PidCompare,
    /// <summary>机器学习散点图（聚类/回归/分类）。</summary>
    Scatter,
    /// <summary>矩阵表格（线性代数）。</summary>
    Matrix,
    /// <summary>二维向量图（幂迭代）。</summary>
    Vector
}
