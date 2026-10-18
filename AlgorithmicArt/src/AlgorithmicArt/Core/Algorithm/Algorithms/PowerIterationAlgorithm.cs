using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>幂迭代法：反复用矩阵乘向量并归一化，方向收敛到主特征向量。</summary>
public class PowerIterationAlgorithm : IAlgorithm
{
    public string Category => "其他";
    public string Name => "幂迭代法";
    public string Description => "v ← A·v / ‖A·v‖，向量方向逐步收敛到主特征向量";
    public RendererKind Renderer => RendererKind.Vector;
    public SceneKind Scene => SceneKind.PowerIteration;

    public long EstimateSteps(int n) => 20;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "任取初始单位向量 v",
        "repeat",
        "  w = A · v",
        "  v = w / ‖w‖",
        "until 方向收敛",
        "λ ≈ vᵀ·A·v"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var a = ctx.MatrixA;
        var v0 = ctx.Vector;
        if (a == null || v0 == null) return;

        double vx = v0[0], vy = v0[1];
        double len = Math.Sqrt(vx * vx + vy * vy);
        if (len > 1e-12)
        {
            vx /= len;
            vy /= len;
        }

        var target = DominantEigenVector(a);

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Assign,
            CodeLine = 0,
            Description = $"初始向量 v = ({vx:0.00}, {vy:0.00})",
            Extra = new VectorFrame
            {
                Vectors = new[] { new PointF((float)vx, (float)vy) },
                Labels = new[] { "v₀" },
                Target = target
            }
        });

        for (int it = 1; it <= 12; it++)
        {
            double nx = a[0, 0] * vx + a[0, 1] * vy;
            double ny = a[1, 0] * vx + a[1, 1] * vy;
            double nl = Math.Sqrt(nx * nx + ny * ny);
            if (nl < 1e-12) break;

            vx = nx / nl;
            vy = ny / nl;

            double lambda = vx * (a[0, 0] * vx + a[0, 1] * vy)
                          + vy * (a[1, 0] * vx + a[1, 1] * vy);

            recorder.Record(new AnimStep
            {
                Op = AnimOp.Move,
                CodeLine = 3,
                Description = $"第 {it} 次迭代：v = ({vx:0.000}, {vy:0.000})，λ ≈ {lambda:0.000}",
                Extra = new VectorFrame
                {
                    Vectors = new[] { new PointF((float)vx, (float)vy) },
                    Labels = new[] { $"v{it}" },
                    Target = target
                }
            });
        }
    }

    /// <summary>2×2 对称矩阵的主特征向量方向。</summary>
    private static PointF DominantEigenVector(double[,] a)
    {
        double tr = a[0, 0] + a[1, 1];
        double det = a[0, 0] * a[1, 1] - a[0, 1] * a[1, 0];
        double disc = Math.Sqrt(Math.Max(0, tr * tr / 4 - det));
        double lambda = tr / 2 + disc;

        double ex = a[0, 1];
        double ey = lambda - a[0, 0];

        if (Math.Abs(ex) < 1e-9 && Math.Abs(ey) < 1e-9)
        {
            ex = lambda - a[1, 1];
            ey = a[1, 0];
        }

        double len = Math.Sqrt(ex * ex + ey * ey);
        if (len < 1e-9) return new PointF(1, 0);

        return new PointF((float)(ex / len), (float)(ey / len));
    }
}
