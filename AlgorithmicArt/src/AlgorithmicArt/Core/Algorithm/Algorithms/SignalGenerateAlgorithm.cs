using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>信号生成。</summary>
public class SignalGenerateAlgorithm : IAlgorithm
{
    public string Category => "信号算法";
    public string Group => "信号";
    public string Name => "信号生成";
    public string Description => "逐个采样生成正弦叠加信号";
    public RendererKind Renderer => RendererKind.Waveform;
    public SceneKind Scene => SceneKind.Signal;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "for n = 0 to N-1",
        "  y[n] = sin(2πfn/fs)",
        "  record(y[n])"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var signal = ctx.Signal;
        if (signal == null || signal.Length == 0) return;

        var output = new double[signal.Length];

        for (int i = 0; i < signal.Length; i++)
        {
            output[i] = signal[i];
            recorder.Record(new AnimStep
            {
                Op = AnimOp.PlotPoint,
                Points = new[] { new PointF(i, (float)signal[i]) },
                Extra = output[..(i + 1)],
                CodeLine = 1,
                Description = $"采样 n={i}，幅值 {signal[i]:0.00}"
            });
        }
    }
}
