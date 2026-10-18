using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>FFT 频谱分析：基 2 蝶形运算，逐级呈现频谱成形过程。</summary>
public class FftAlgorithm : IAlgorithm
{
    public string Category => "信号算法";
    public string Group => "频域";
    public string Name => "FFT 频谱分析";
    public string Description => "补零到 2 的幂后做基 2 快速傅里叶变换，观察频谱逐级成形";
    public RendererKind Renderer => RendererKind.Spectrum;
    public SceneKind Scene => SceneKind.Signal;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "补零到 N = 2^m",
        "位反转重排输入序列",
        "for len = 2, 4, 8, ... N",
        "  for 每个蝶形 (a, b)",
        "    A[a], A[b] = A[a]+w·A[b], A[a]-w·A[b]",
        "幅度谱 |X[k]| = √(Re²+Im²) / N",
        "取前 N/2 个 bin，频率 f = k·Fs/N"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var signal = ctx.Signal;
        if (signal == null || signal.Length < 4) return;

        // 补零到 2 的幂
        int n = 1;
        while (n < signal.Length) n <<= 1;
        n = Math.Clamp(n, 8, 1024);

        var re = new double[n];
        var im = new double[n];
        for (int i = 0; i < signal.Length && i < n; i++) re[i] = signal[i];

        ctx.Parameters["FftSize"] = n;
        double fs = ctx.Parameters.GetValueOrDefault("Fs", 1000);

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Assign,
            CodeLine = 0,
            Description = $"信号 {signal.Length} 点，补零到 {n} 点（2 的幂）",
            Extra = Magnitudes(re, im)
        });

        // 位反转重排
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1) j ^= bit;
            j ^= bit;
            if (i < j) (re[i], re[j]) = (re[j], re[i]);
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Assign,
            CodeLine = 1,
            Description = "位反转重排完成，准备逐级蝶形运算",
            Extra = Magnitudes(re, im)
        });

        // 蝶形运算逐级推进
        int stage = 0;
        int totalStages = (int)Math.Log2(n);

        for (int len = 2; len <= n; len <<= 1)
        {
            double ang = -2 * Math.PI / len;
            double wRe = Math.Cos(ang);
            double wIm = Math.Sin(ang);

            for (int i = 0; i < n; i += len)
            {
                double curRe = 1, curIm = 0;

                for (int j = 0; j < len / 2; j++)
                {
                    int a = i + j;
                    int b = a + len / 2;

                    double vRe = re[b] * curRe - im[b] * curIm;
                    double vIm = re[b] * curIm + im[b] * curRe;
                    double uRe = re[a];
                    double uIm = im[a];

                    re[a] = uRe + vRe;
                    im[a] = uIm + vIm;
                    re[b] = uRe - vRe;
                    im[b] = uIm - vIm;

                    double nextRe = curRe * wRe - curIm * wIm;
                    curIm = curRe * wIm + curIm * wRe;
                    curRe = nextRe;
                }
            }

            stage++;
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Compare,
                CodeLine = 2,
                Description = $"第 {stage}/{totalStages} 级蝶形运算完成（跨度 {len}）",
                Extra = Magnitudes(re, im)
            });
        }

        // 逐 bin 展示幅度谱
        var mags = Magnitudes(re, im);
        int peak = 0;
        for (int k = 1; k < mags.Length; k++)
            if (mags[k] > mags[peak]) peak = k;

        // 逐 bin 扫描最多 128 步，避免大点数时步数膨胀
        int stride = Math.Max(1, mags.Length / 128);
        for (int k = 0; k < mags.Length; k += stride)
        {
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Highlight,
                Indices = new[] { k },
                CodeLine = 5,
                Description = $"bin {k}：频率 {k * fs / n:0.0} Hz，幅度 {mags[k]:0.000}",
                Extra = mags
            });
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            Indices = new[] { peak },
            CodeLine = 6,
            Description = $"主频 {peak * fs / n:0.0} Hz（bin {peak}），幅度 {mags[peak]:0.000}",
            Extra = mags
        });
    }

    /// <summary>计算归一化幅度谱（取前 N/2 个 bin）。</summary>
    private static float[] Magnitudes(double[] re, double[] im)
    {
        int half = Math.Max(1, re.Length / 2);
        var mags = new float[half];
        double scale = 1.0 / Math.Max(re.Length, 1);

        for (int k = 0; k < half; k++)
            mags[k] = (float)(Math.Sqrt(re[k] * re[k] + im[k] * im[k]) * scale);

        return mags;
    }
}
