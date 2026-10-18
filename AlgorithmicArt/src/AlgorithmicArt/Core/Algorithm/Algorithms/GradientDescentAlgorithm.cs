using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>梯度下降：沿负梯度迭代更新斜率与截距，损失逐步下降。</summary>
public class GradientDescentAlgorithm : IAlgorithm
{
    public string Category => "机器学习算法";
    public string Group => "优化";
    public string Name => "梯度下降拟合";
    public string Description => "以学习率 0.15 迭代更新 w、b，观察直线逼近与 MSE 下降";
    public RendererKind Renderer => RendererKind.Scatter;
    public SceneKind Scene => SceneKind.MlRegression;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "初始化 w = 0, b = 0, lr = 0.15",
        "repeat epochs",
        "  ŷ = w·x + b",
        "  gw = 2/n·Σ(ŷ-y)·x,  gb = 2/n·Σ(ŷ-y)",
        "  w -= lr·gw;  b -= lr·gb",
        "  记录 MSE"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var pts = ctx.MlPoints;
        if (pts == null || pts.Length < 2) return;

        int n = pts.Length;
        const double lr = 0.15;
        const int epochs = 100;

        // 点集在整个算法中不变，只构建一次
        var points = new PointF[n];
        for (int i = 0; i < n; i++)
            points[i] = new PointF(pts[i].X, pts[i].Y);

        // 归一化到 0~1 区间训练，输出时再还原
        var xs = pts.Select(p => p.X / 100.0).ToArray();
        var ys = pts.Select(p => p.Y / 100.0).ToArray();

        double w = 0, b = 0;
        var loss = new List<float>();

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            double gw = 0, gb = 0, mse = 0;

            for (int i = 0; i < n; i++)
            {
                double pred = w * xs[i] + b;
                double err = pred - ys[i];
                gw += err * xs[i];
                gb += err;
                mse += err * err;
            }

            gw = 2 * gw / n;
            gb = 2 * gb / n;
            mse /= n;

            w -= lr * gw;
            b -= lr * gb;

            double mseRaw = mse * 10000.0; // 还原到原始坐标量级，便于阅读
            loss.Add((float)mseRaw);

            var line = new[]
            {
                new PointF(0, (float)(b * 100)),
                new PointF(100, (float)((w + b) * 100))
            };

            recorder.Record(new AnimStep
            {
                Op = AnimOp.Move,
                CodeLine = 4,
                Description = $"第 {epoch + 1} 轮：w = {w:0.000}，b = {b:0.000}，MSE = {mseRaw:0.0}",
                Extra = MlFrame.Snapshot(points, Array.Empty<int>(), line: line, loss: loss.ToArray())
            });
        }
    }
}
