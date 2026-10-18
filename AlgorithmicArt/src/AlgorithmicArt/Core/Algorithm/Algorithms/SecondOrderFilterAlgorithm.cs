using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>二阶低通滤波（Biquad 双二阶）。</summary>
public class SecondOrderFilterAlgorithm : IAlgorithm
{
    public string Category => "信号算法";
    public string Group => "滤波";
    public string Name => "二阶滤波";
    public string Description => "二阶低通（Biquad）滤波器平滑噪声";
    public RendererKind Renderer => RendererKind.Waveform;
    public SceneKind Scene => SceneKind.Signal;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "for n = 0 to N-1",
        "  y[n] = b0*x[n] + b1*x[n-1] + b2*x[n-2]",
        "        - a1*y[n-1] - a2*y[n-2]",
        "  record(y[n])"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var x = ctx.Signal;
        if (x == null || x.Length == 0) return;

        double fs = ctx.Parameters.GetValueOrDefault("Fs", 1000);
        double fc = ctx.Parameters.GetValueOrDefault("Fc", 50);
        double q = ctx.Parameters.GetValueOrDefault("Quality", 0.7071067811865476);

        double omega = 2 * Math.PI * fc / fs;
        double alpha = Math.Sin(omega) / (2 * q);
        double cosw = Math.Cos(omega);

        double a0 = 1 + alpha;
        double b0 = (1 - cosw) / 2 / a0;
        double b1 = (1 - cosw) / a0;
        double b2 = (1 - cosw) / 2 / a0;
        double a1 = -2 * cosw / a0;
        double a2 = (1 - alpha) / a0;

        double x1 = 0, x2 = 0, y1 = 0, y2 = 0;
        var output = new double[x.Length];

        for (int n = 0; n < x.Length; n++)
        {
            double y = b0 * x[n] + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
            x2 = x1; x1 = x[n];
            y2 = y1; y1 = y;
            output[n] = y;

            recorder.Record(new AnimStep
            {
                Op = AnimOp.PlotPoint,
                Points = new[] { new PointF(n, (float)y) },
                Extra = output[..(n + 1)],
                CodeLine = 3,
                Description = $"n={n}，输入 {x[n]:0.00}，二阶滤波输出 {y:0.00}"
            });
        }
    }
}
