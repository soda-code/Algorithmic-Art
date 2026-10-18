using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>高斯消元：用初等行变换把增广矩阵化为单位阵，直接读出解。</summary>
public class GaussianEliminationAlgorithm : IAlgorithm
{
    public string Category => "方程组计算";
    public string Name => "高斯消元";
    public string Description => "主元归一 + 逐行消元，把增广矩阵化为单位阵";
    public RendererKind Renderer => RendererKind.Matrix;
    public SceneKind Scene => SceneKind.LinearSystem;

    public long EstimateSteps(int n) => 60;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "构造增广矩阵 [A | b]",
        "for col = 0 to n-1",
        "  第 col 行除以主元 A[col][col]",
        "  for 其它每一行 r",
        "    A[r] -= A[r][col] · A[col]",
        "解 x[col] = 增广列"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var a = ctx.MatrixA;
        var b = ctx.Vector;
        if (a == null || b == null) return;

        int n = a.GetLength(0);
        var aug = new double[n, n + 1];
        for (int r = 0; r < n; r++)
        {
            for (int c = 0; c < n; c++) aug[r, c] = a[r, c];
            aug[r, n] = b[r];
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Assign,
            CodeLine = 0,
            Description = $"构造增广矩阵 [{n}×{n + 1}]",
            Extra = Frame(aug, -1, -1, Array.Empty<int>())
        });

        for (int col = 0; col < n; col++)
        {
            double pivot = aug[col, col];
            if (Math.Abs(pivot) < 1e-12) continue;

            recorder.Record(new AnimStep
            {
                Op = AnimOp.Highlight,
                CodeLine = 1,
                Description = $"第 {col + 1} 列主元 = {pivot:0.##}",
                Extra = Frame(aug, col, col, new[] { col })
            });

            for (int c = col; c <= n; c++) aug[col, c] /= pivot;

            int pr = col;
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Assign,
                CodeLine = 2,
                Description = $"第 {pr + 1} 行归一化，主元变为 1",
                Extra = Frame(aug, pr, pr, new[] { pr })
            });

            for (int r = 0; r < n; r++)
            {
                if (r == col) continue;

                double factor = aug[r, col];
                if (Math.Abs(factor) < 1e-12) continue;

                for (int c = col; c <= n; c++) aug[r, c] -= factor * aug[col, c];

                int rr = r;
                recorder.Record(new AnimStep
                {
                    Op = AnimOp.Assign,
                    CodeLine = 4,
                    Description = $"第 {rr + 1} 行 -= {factor:0.##} × 第 {col + 1} 行",
                    Extra = Frame(aug, rr, col, new[] { rr, col })
                });
            }
        }

        var solution = new double[n];
        for (int i = 0; i < n; i++) solution[i] = aug[i, n];

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 5,
            Description = "求解完成：x = [" + string.Join(", ", solution.Select(v => v.ToString("0.##"))) + "]",
            Extra = Frame(aug, -1, -1, Array.Empty<int>())
        });
    }

    private static MatrixFrame Frame(double[,] aug, int accentRow, int accentCol, int[] rows) =>
        MatrixFrame.Of(new MatrixBlock("增广矩阵 [A | b]", aug)
        {
            AccentRow = accentRow,
            AccentCol = accentCol,
            RowHighlights = rows,
            SplitAfterColumn = aug.GetLength(1) - 2
        });
}
