namespace AlgorithmicArt.Core.Algorithm;

/// <summary>矩阵块：渲染器按块并排绘制（如 A、B、C 或 A、L、U）。</summary>
public class MatrixBlock
{
    public string Title { get; set; } = "";
    public double[,] Values { get; set; } = new double[0, 0];

    /// <summary>高亮的行索引。</summary>
    public int[] RowHighlights { get; set; } = Array.Empty<int>();
    /// <summary>高亮的列索引。</summary>
    public int[] ColHighlights { get; set; } = Array.Empty<int>();
    /// <summary>强调单元格（当前计算位置）。</summary>
    public int AccentRow { get; set; } = -1;
    public int AccentCol { get; set; } = -1;
    /// <summary>竖直分隔线的列位置（用于增广矩阵 [A|b]）。</summary>
    public int SplitAfterColumn { get; set; } = -1;

    public MatrixBlock() { }

    public MatrixBlock(string title, double[,] values)
    {
        Title = title;
        Values = (double[,])values.Clone();
    }

    public int Rows => Values.GetLength(0);
    public int Cols => Values.GetLength(1);
}

/// <summary>线性代数动画帧：一组并排的矩阵块。</summary>
public class MatrixFrame
{
    public MatrixBlock[] Blocks { get; set; } = Array.Empty<MatrixBlock>();

    public static MatrixFrame Of(params MatrixBlock[] blocks) => new() { Blocks = blocks };
}

/// <summary>二维向量帧（幂迭代等）。</summary>
public class VectorFrame
{
    public PointF[] Vectors { get; set; } = Array.Empty<PointF>();
    public string[] Labels { get; set; } = Array.Empty<string>();
    /// <summary>目标方向（特征向量），用虚线标出。</summary>
    public PointF? Target { get; set; }
}
