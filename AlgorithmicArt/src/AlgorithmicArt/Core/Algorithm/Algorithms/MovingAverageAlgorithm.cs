using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>滑动平均滤波。</summary>
public class MovingAverageAlgorithm : IAlgorithm
{
    public string Category => "信号算法";
    public string Group => "滤波";
    public string Name => "滑动平均";
    public string Description => "用滑动窗口均值平滑信号";
    public RendererKind Renderer => RendererKind.Waveform;
    public SceneKind Scene => SceneKind.Signal;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "for n = 0 to N-1",
        "  sum = Σ y[n-k], k=0..W-1",
        "  out[n] = sum / W",
        "  record(out[n])"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var signal = ctx.Signal;
        if (signal == null || signal.Length == 0) return;

        int w = Math.Max(1, (int)ctx.Parameters.GetValueOrDefault("Window", 11));

        var output = new double[signal.Length];

        for (int i = 0; i < signal.Length; i++)
        {
            int from = Math.Max(0, i - w + 1);
            int count = i - from + 1;
            double sum = 0;
            for (int k = from; k <= i; k++) sum += signal[k];
            double avg = sum / count;
            output[i] = avg;

            recorder.Record(new AnimStep
            {
                Op = AnimOp.PlotPoint,
                Points = new[] { new PointF(i, (float)avg) },
                Extra = output[..(i + 1)],
                CodeLine = 2,
                Description = $"n={i}，窗口[{from},{i}]，均值 {avg:0.00}"
            });
        }
    }
}
