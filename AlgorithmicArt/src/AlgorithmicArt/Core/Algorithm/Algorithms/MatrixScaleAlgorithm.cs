using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>矩阵数乘：标量乘以矩阵的每个元素。</summary>
public class MatrixScaleAlgorithm : IAlgorithm
{
    public string Category => "矩阵计算";
    public string Name => "矩阵数乘";
    public string Description => "标量 k 乘以矩阵的每个元素：C[i][j] = k·A[i][j]";
    public RendererKind Renderer => RendererKind.Matrix;
    public SceneKind Scene => SceneKind.MatrixArithmetic;

    public long EstimateSteps(int n) => 20;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "读取标量 k",
        "for i, j",
        "  C[i][j] = k · A[i][j]"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var a = ctx.MatrixA;
        if (a == null) return;

        double k = ctx.Parameters.GetValueOrDefault("Scalar", 3);
        int rows = a.GetLength(0);
        int cols = a.GetLength(1);
        var c = new double[rows, cols];

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Assign,
            CodeLine = 0,
            Description = $"标量 k = {k:0.##}",
            Extra = Frame(a, c, k, -1, -1)
        });

        for (int r = 0; r < rows; r++)
        {
            for (int col = 0; col < cols; col++)
            {
                c[r, col] = k * a[r, col];

                int rr = r, cc = col;
                double av = a[r, col], cv = c[r, col];

                recorder.Record(new AnimStep
                {
                    Op = AnimOp.Assign,
                    CodeLine = 2,
                    Description = $"C[{rr}][{cc}] = {k:0.##} × {av} = {cv:0.##}",
                    Extra = Frame(a, c, k, rr, cc)
                });
            }
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 2,
            Description = "数乘完成",
            Extra = Frame(a, c, k, -1, -1)
        });
    }

    private static MatrixFrame Frame(double[,] a, double[,] c, double k, int row, int col) =>
        MatrixFrame.Of(
            new MatrixBlock("A", a),
            new MatrixBlock($"C = {k:0.##} × A", c) { AccentRow = row, AccentCol = col });
}
