using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>线性回归：最小二乘法逐步累加统计量，拟合 y = a + b·x。</summary>
public class LinearRegressionAlgorithm : IAlgorithm
{
    public string Category => "机器学习算法";
    public string Group => "回归";
    public string Name => "线性回归";
    public string Description => "最小二乘法逐样本累加，实时更新拟合直线与均方误差";
    public RendererKind Renderer => RendererKind.Scatter;
    public SceneKind Scene => SceneKind.MlRegression;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "sumX = sumY = sumXY = sumX2 = 0",
        "for each (x, y) in 样本",
        "  sumX += x; sumY += y",
        "  sumXY += x*y; sumX2 += x*x",
        "b = (n·sumXY - sumX·sumY) / (n·sumX2 - sumX²)",
        "a = (sumY - b·sumX) / n",
        "拟合直线 y = a + b·x"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var pts = ctx.MlPoints;
        if (pts == null || pts.Length < 2) return;

        // 点集在整个算法中不变，只构建一次
        var points = new PointF[pts.Length];
        for (int i = 0; i < pts.Length; i++)
            points[i] = new PointF(pts[i].X, pts[i].Y);

        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
        int n = 0;
        var loss = new List<float>();

        for (int i = 0; i < pts.Length; i++)
        {
            double x = pts[i].X;
            double y = pts[i].Y;

            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
            n++;

            double denom = n * sumX2 - sumX * sumX;
            double b = Math.Abs(denom) < 1e-9 ? 0 : (n * sumXY - sumX * sumY) / denom;
            double a = (sumY - b * sumX) / n;

            // 当前模型在已见样本上的均方误差
            double mse = 0;
            for (int j = 0; j <= i; j++)
            {
                double pred = a + b * pts[j].X;
                double err = pts[j].Y - pred;
                mse += err * err;
            }
            mse /= (i + 1);
            loss.Add((float)mse);

            var line = new[]
            {
                new PointF(0, (float)a),
                new PointF(100, (float)(a + b * 100))
            };

            recorder.Record(new AnimStep
            {
                Op = AnimOp.PlotPoint,
                Indices = new[] { i },
                CodeLine = 2,
                Description = $"加入样本 {i + 1}/{pts.Length}：y = {a:0.00} + {b:0.00}x，MSE = {mse:0.0}",
                Extra = MlFrame.Snapshot(points, Array.Empty<int>(), line: line,
                    highlight: new[] { i }, loss: loss.ToArray())
            });
        }
    }
}
