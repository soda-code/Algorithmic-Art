using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>一维卡尔曼滤波。</summary>
public class KalmanFilterAlgorithm : IAlgorithm
{
    public string Category => "信号算法";
    public string Group => "滤波";
    public string Name => "卡尔曼滤波";
    public string Description => "一维卡尔曼滤波估计真实信号";
    public RendererKind Renderer => RendererKind.Waveform;
    public SceneKind Scene => SceneKind.Signal;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "x = z[0], P = 1",
        "for n = 0 to N-1",
        "  P = P + Q",
        "  K = P / (P + R)",
        "  x = x + K * (z[n] - x)",
        "  P = (1 - K) * P",
        "  record(x)"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var z = ctx.Signal;
        if (z == null || z.Length == 0) return;

        double q = ctx.Parameters.GetValueOrDefault("KalmanQ", 1e-3);
        double r = ctx.Parameters.GetValueOrDefault("KalmanR", 0.02);

        double x = z[0];
        double p = 1;
        var output = new double[z.Length];

        for (int n = 0; n < z.Length; n++)
        {
            p += q;
            double k = p / (p + r);
            x += k * (z[n] - x);
            p *= 1 - k;
            output[n] = x;

            recorder.Record(new AnimStep
            {
                Op = AnimOp.PlotPoint,
                Points = new[] { new PointF(n, (float)x) },
                Extra = output[..(n + 1)],
                CodeLine = 5,
                Description = $"n={n}，观测 {z[n]:0.00}，卡尔曼估计 {x:0.00}，K={k:0.00}"
            });
        }
    }
}
