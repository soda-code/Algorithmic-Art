using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>矩阵加法：同型矩阵对应元素相加。</summary>
public class MatrixAdditionAlgorithm : IAlgorithm
{
    public string Category => "矩阵计算";
    public string Name => "矩阵加法";
    public string Description => "同型矩阵对应位置相加：C[i][j] = A[i][j] + B[i][j]";
    public RendererKind Renderer => RendererKind.Matrix;
    public SceneKind Scene => SceneKind.MatrixArithmetic;

    public long EstimateSteps(int n) => 20;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "要求 A、B 同型",
        "for i, j",
        "  C[i][j] = A[i][j] + B[i][j]"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var a = ctx.MatrixA;
        var b = ctx.MatrixB;
        if (a == null || b == null) return;

        int rows = a.GetLength(0);
        int cols = a.GetLength(1);
        var c = new double[rows, cols];

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Assign,
            CodeLine = 0,
            Description = $"A、B 均为 {rows}×{cols}，可相加",
            Extra = Frame(a, b, c, -1, -1)
        });

        for (int r = 0; r < rows; r++)
        {
            for (int col = 0; col < cols; col++)
            {
                c[r, col] = a[r, col] + b[r, col];

                int rr = r, cc = col;
                double av = a[r, col], bv = b[r, col], cv = c[r, col];

                recorder.Record(new AnimStep
                {
                    Op = AnimOp.Assign,
                    CodeLine = 2,
                    Description = $"C[{rr}][{cc}] = {av} + {bv} = {cv}",
                    Extra = Frame(a, b, c, rr, cc)
                });
            }
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 2,
            Description = "矩阵加法完成",
            Extra = Frame(a, b, c, -1, -1)
        });
    }

    private static MatrixFrame Frame(double[,] a, double[,] b, double[,] c, int row, int col) =>
        MatrixFrame.Of(
            new MatrixBlock("A", a),
            new MatrixBlock("B", b),
            new MatrixBlock("C = A + B", c) { AccentRow = row, AccentCol = col });
}
