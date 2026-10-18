using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>矩阵求逆：对增广矩阵 [A | E] 做初等行变换化为 [E | A⁻¹]。</summary>
public class MatrixInverseAlgorithm : IAlgorithm
{
    public string Category => "矩阵计算";
    public string Name => "矩阵求逆";
    public string Description => "初等行变换把 [A | E] 化为 [E | A⁻¹]，相当于矩阵的“除法”";
    public RendererKind Renderer => RendererKind.Matrix;
    public SceneKind Scene => SceneKind.MatrixInverse;

    public long EstimateSteps(int n) => 60;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "构造增广矩阵 [A | E]",
        "for col = 0 to n-1",
        "  第 col 行除以主元",
        "  for 其它行 r：A[r] -= A[r][col]·A[col]",
        "右侧 n 列即为 A⁻¹"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var a = ctx.MatrixA;
        if (a == null) return;

        int n = a.GetLength(0);
        var aug = new double[n, 2 * n];

        for (int r = 0; r < n; r++)
        {
            for (int c = 0; c < n; c++) aug[r, c] = a[r, c];
            for (int c = 0; c < n; c++) aug[r, n + c] = r == c ? 1 : 0;
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Assign,
            CodeLine = 0,
            Description = "构造增广矩阵 [A | E]",
            Extra = Frame(aug, -1, -1, Array.Empty<int>())
        });

        for (int col = 0; col < n; col++)
        {
            double pivot = aug[col, col];
            if (Math.Abs(pivot) < 1e-12) continue;

            for (int c = 0; c < 2 * n; c++) aug[col, c] /= pivot;

            int pr = col;
            double pv = pivot;
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Assign,
                CodeLine = 2,
                Description = $"第 {pr + 1} 行除以主元 {pv:0.###}",
                Extra = Frame(aug, pr, pr, new[] { pr })
            });

            for (int r = 0; r < n; r++)
            {
                if (r == col) continue;

                double factor = aug[r, col];
                if (Math.Abs(factor) < 1e-12) continue;

                for (int c = 0; c < 2 * n; c++) aug[r, c] -= factor * aug[col, c];

                int rr = r;
                recorder.Record(new AnimStep
                {
                    Op = AnimOp.Assign,
                    CodeLine = 3,
                    Description = $"第 {rr + 1} 行 -= {factor:0.###} × 第 {col + 1} 行",
                    Extra = Frame(aug, rr, col, new[] { rr, col })
                });
            }
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 4,
            Description = "求逆完成：右侧 n 列即 A⁻¹",
            Extra = Frame(aug, -1, -1, Array.Empty<int>())
        });
    }

    private static MatrixFrame Frame(double[,] aug, int row, int col, int[] rows) =>
        MatrixFrame.Of(new MatrixBlock("[A | E] ⇒ [E | A⁻¹]", aug)
        {
            AccentRow = row,
            AccentCol = col,
            RowHighlights = rows,
            SplitAfterColumn = aug.GetLength(1) / 2 - 1
        });
}
