using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>LU 分解：Doolittle 算法，把矩阵拆成单位下三角 L 与上三角 U。</summary>
public class LuDecompositionAlgorithm : IAlgorithm
{
    public string Category => "方程组计算";
    public string Name => "LU 分解";
    public string Description => "A = L·U，L 为单位下三角，U 为上三角";
    public RendererKind Renderer => RendererKind.Matrix;
    public SceneKind Scene => SceneKind.LuDecomposition;

    public long EstimateSteps(int n) => 80;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "L = I，U = 0",
        "for i = 0 to n-1",
        "  for k = i to n-1",
        "    U[i][k] = A[i][k] - Σ L[i][j]·U[j][k]",
        "  for k = i+1 to n-1",
        "    L[k][i] = (A[k][i] - Σ L[k][j]·U[j][i]) / U[i][i]"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var a = ctx.MatrixA;
        if (a == null) return;

        int n = a.GetLength(0);
        var l = new double[n, n];
        var u = new double[n, n];

        for (int i = 0; i < n; i++) l[i, i] = 1;

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Assign,
            CodeLine = 0,
            Description = "初始化 L 为单位下三角，U 全零",
            Extra = Frame(a, l, u, -1, -1, -1, -1)
        });

        for (int i = 0; i < n; i++)
        {
            for (int k = i; k < n; k++)
            {
                double sum = 0;
                for (int j = 0; j < i; j++) sum += l[i, j] * u[j, k];
                u[i, k] = a[i, k] - sum;

                int ui = i, uk = k;
                double val = u[i, k];
                recorder.Record(() => new AnimStep
                {
                    Op = AnimOp.Assign,
                    CodeLine = 3,
                    Description = $"U[{ui}][{uk}] = {val:0.##}",
                    Extra = Frame(a, l, u, ui, uk, -1, -1)
                });
            }

            for (int k = i + 1; k < n; k++)
            {
                double sum = 0;
                for (int j = 0; j < i; j++) sum += l[k, j] * u[j, i];
                l[k, i] = (a[k, i] - sum) / u[i, i];

                int lk = k, li = i;
                double val = l[k, i];
                recorder.Record(() => new AnimStep
                {
                    Op = AnimOp.Assign,
                    CodeLine = 5,
                    Description = $"L[{lk}][{li}] = {val:0.##}",
                    Extra = Frame(a, l, u, -1, -1, lk, li)
                });
            }
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 5,
            Description = "LU 分解完成：A = L × U",
            Extra = Frame(a, l, u, -1, -1, -1, -1)
        });
    }

    private static MatrixFrame Frame(double[,] a, double[,] l, double[,] u,
        int uRow, int uCol, int lRow, int lCol) =>
        MatrixFrame.Of(
            new MatrixBlock("A", a),
            new MatrixBlock("L", l) { AccentRow = lRow, AccentCol = lCol },
            new MatrixBlock("U", u) { AccentRow = uRow, AccentCol = uCol });
}
