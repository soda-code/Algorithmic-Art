using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>矩阵乘法：按行乘列累加，观察结果矩阵逐格填充。</summary>
public class MatrixMultiplicationAlgorithm : IAlgorithm
{
    public string Category => "矩阵计算";
    public string Name => "矩阵相乘";
    public string Description => "C[i][j] 逐个累加 A 的第 i 行与 B 的第 j 列";
    public RendererKind Renderer => RendererKind.Matrix;
    public SceneKind Scene => SceneKind.MatrixMultiply;

    public long EstimateSteps(int n) => 100;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "for i = 0 to rows(A)-1",
        "  for j = 0 to cols(B)-1",
        "    C[i][j] = Σ A[i][k]·B[k][j]",
        "输出 C"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var a = ctx.MatrixA;
        var b = ctx.MatrixB;
        if (a == null || b == null) return;

        int rows = a.GetLength(0);
        int inner = a.GetLength(1);
        int cols = b.GetLength(1);
        var c = new double[rows, cols];

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Assign,
            CodeLine = 0,
            Description = $"C({rows}×{cols}) = A({rows}×{inner}) × B({inner}×{cols})",
            Extra = Frame(a, b, c, -1, -1, Array.Empty<int>(), Array.Empty<int>())
        });

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                double sum = 0;

                for (int k = 0; k < inner; k++)
                {
                    sum += a[i, k] * b[k, j];

                    int ci = i, cj = j, ck = k;
                    double part = sum;
                    double av = a[i, k], bv = b[k, j];

                    recorder.Record(() => new AnimStep
                    {
                        Op = AnimOp.Compare,
                        CodeLine = 2,
                        Description = $"C[{ci}][{cj}] += A[{ci}][{ck}]×B[{ck}][{cj}] = {av}×{bv}，累计 {part:0.##}",
                        Extra = Frame(a, b, c, ci, cj, new[] { ci }, new[] { cj })
                    });
                }

                c[i, j] = sum;

                int ri = i, rj = j;
                double val = sum;
                recorder.Record(new AnimStep
                {
                    Op = AnimOp.Assign,
                    CodeLine = 2,
                    Description = $"C[{ri}][{rj}] = {val:0.##}",
                    Extra = Frame(a, b, c, ri, rj, Array.Empty<int>(), Array.Empty<int>())
                });
            }
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 3,
            Description = "矩阵乘法完成",
            Extra = Frame(a, b, c, -1, -1, Array.Empty<int>(), Array.Empty<int>())
        });
    }

    private static MatrixFrame Frame(double[,] a, double[,] b, double[,] c,
        int accentRow, int accentCol, int[] aRows, int[] bCols) =>
        MatrixFrame.Of(
            new MatrixBlock("A", a) { RowHighlights = aRows },
            new MatrixBlock("B", b) { ColHighlights = bCols },
            new MatrixBlock("C = A × B", c) { AccentRow = accentRow, AccentCol = accentCol });
}
