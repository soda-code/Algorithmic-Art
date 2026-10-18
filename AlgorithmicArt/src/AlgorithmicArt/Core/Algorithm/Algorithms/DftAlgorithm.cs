using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>DFT 离散傅里叶变换：按定义逐 bin 计算，与 FFT 结果对照。</summary>
public class DftAlgorithm : IAlgorithm
{
    public string Category => "信号算法";
    public string Group => "频域";
    public string Name => "DFT 离散傅里叶";
    public string Description => "按定义对每个频率点累加，O(N²)，用于对照 FFT 结果";
    public RendererKind Renderer => RendererKind.Spectrum;
    public SceneKind Scene => SceneKind.Signal;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "补零到 N",
        "for k = 0 to N/2-1",
        "  Re = Im = 0",
        "  for t = 0 to N-1",
        "    Re += x[t]·cos(2πkt/N)",
        "    Im -= x[t]·sin(2πkt/N)",
        "  |X[k]| = √(Re²+Im²) / N"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var signal = ctx.Signal;
        if (signal == null || signal.Length < 4) return;

        int n = 1;
        while (n < signal.Length) n <<= 1;
        n = Math.Clamp(n, 8, 512); // DFT 是 O(N²)，上限更保守

        var x = new double[n];
        for (int i = 0; i < signal.Length && i < n; i++) x[i] = signal[i];

        ctx.Parameters["FftSize"] = n;
        double fs = ctx.Parameters.GetValueOrDefault("Fs", 1000);

        int half = n / 2;
        var mags = new float[half];

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Assign,
            CodeLine = 0,
            Description = $"信号 {signal.Length} 点，补零到 {n} 点，逐个频率点计算",
            Extra = (float[])mags.Clone()
        });

        for (int k = 0; k < half; k++)
        {
            double sumRe = 0, sumIm = 0;
            double step = -2 * Math.PI * k / n;

            for (int t = 0; t < n; t++)
            {
                double ang = step * t;
                sumRe += x[t] * Math.Cos(ang);
                sumIm += x[t] * Math.Sin(ang);
            }

            mags[k] = (float)(Math.Sqrt(sumRe * sumRe + sumIm * sumIm) / n);

            recorder.Record(new AnimStep
            {
                Op = AnimOp.Compare,
                Indices = new[] { k },
                CodeLine = 6,
                Description = $"bin {k}：频率 {k * fs / n:0.0} Hz，幅度 {mags[k]:0.000}",
                Extra = (float[])mags.Clone()
            });
        }

        int peak = 0;
        for (int k = 1; k < half; k++)
            if (mags[k] > mags[peak]) peak = k;

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            Indices = new[] { peak },
            CodeLine = 6,
            Description = $"主频 {peak * fs / n:0.0} Hz（bin {peak}），幅度 {mags[peak]:0.000}",
            Extra = mags
        });
    }
}
