using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>矩阵转置：行列互换 B[i][j] = A[j][i]。</summary>
public class MatrixTransposeAlgorithm : IAlgorithm
{
    public string Category => "矩阵计算";
    public string Name => "矩阵转置";
    public string Description => "行列互换，m×n 矩阵变为 n×m";
    public RendererKind Renderer => RendererKind.Matrix;
    public SceneKind Scene => SceneKind.MatrixTranspose;

    public long EstimateSteps(int n) => 30;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "B 为 n×m 矩阵",
        "for i, j",
        "  B[j][i] = A[i][j]"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var a = ctx.MatrixA;
        if (a == null) return;

        int rows = a.GetLength(0);
        int cols = a.GetLength(1);
        var b = new double[cols, rows];

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Assign,
            CodeLine = 0,
            Description = $"A 为 {rows}×{cols}，转置后 B 为 {cols}×{rows}",
            Extra = Frame(a, b, -1, -1, -1, -1)
        });

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                b[c, r] = a[r, c];

                int rr = r, cc = c;
                double v = a[r, c];

                recorder.Record(new AnimStep
                {
                    Op = AnimOp.Assign,
                    CodeLine = 2,
                    Description = $"B[{cc}][{rr}] = A[{rr}][{cc}] = {v}",
                    Extra = Frame(a, b, rr, cc, cc, rr)
                });
            }
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 2,
            Description = "转置完成",
            Extra = Frame(a, b, -1, -1, -1, -1)
        });
    }

    private static MatrixFrame Frame(double[,] a, double[,] b,
        int aRow, int aCol, int bRow, int bCol) =>
        MatrixFrame.Of(
            new MatrixBlock("A", a) { AccentRow = aRow, AccentCol = aCol },
            new MatrixBlock("B = Aᵀ", b) { AccentRow = bRow, AccentCol = bCol });
}
