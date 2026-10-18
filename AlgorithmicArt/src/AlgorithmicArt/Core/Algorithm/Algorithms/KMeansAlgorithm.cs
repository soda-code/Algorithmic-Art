using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>K-Means 聚类：迭代「分配样本 → 更新质心」。</summary>
public class KMeansAlgorithm : IAlgorithm
{
    public string Category => "机器学习算法";
    public string Group => "聚类";
    public string Name => "K-Means 聚类";
    public string Description => "把样本迭代划分到最近的质心，并重算各簇质心位置";
    public RendererKind Renderer => RendererKind.Scatter;
    public SceneKind Scene => SceneKind.MlClusters;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "选择 K 个初始质心",
        "repeat",
        "  每个点分配到最近的质心",
        "  每个簇重新计算质心（均值）",
        "until 质心不再移动"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var pts = ctx.MlPoints;
        if (pts == null || pts.Length == 0) return;

        int n = pts.Length;
        int k = Math.Clamp((int)ctx.Parameters.GetValueOrDefault("K", 3), 1, 5);

        // 运行期间不变的点集与标签只构建一次，所有步骤共享，避免每步复制
        var points = new PointF[n];
        var labels = new int[n];
        for (int i = 0; i < n; i++)
        {
            points[i] = new PointF(pts[i].X, pts[i].Y);
            labels[i] = pts[i].Label;
        }

        // 初始质心：按样本均匀抽样
        var centroids = new PointF[k];
        for (int c = 0; c < k; c++)
        {
            int idx = Math.Min(n - 1, (int)((c + 0.5) * n / k));
            centroids[c] = new PointF(pts[idx].X, pts[idx].Y);
        }

        var assign = Enumerable.Repeat(-1, n).ToArray();

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 0,
            Description = $"初始化 {k} 个质心",
            Extra = MlFrame.Snapshot(points, labels, assign.ToArray(), centroids.ToArray())
        });

        for (int iter = 0; iter < 8; iter++)
        {
            bool changed = false;

            // 分配步
            for (int i = 0; i < n; i++)
            {
                int best = 0;
                float bestDist = float.MaxValue;
                for (int c = 0; c < k; c++)
                {
                    float dx = pts[i].X - centroids[c].X;
                    float dy = pts[i].Y - centroids[c].Y;
                    float d = dx * dx + dy * dy;
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = c;
                    }
                }

                if (assign[i] != best)
                {
                    assign[i] = best;
                    changed = true;
                }

                recorder.Record(new AnimStep
                {
                    Op = AnimOp.Compare,
                    Indices = new[] { i },
                    CodeLine = 2,
                    Description = $"第 {iter + 1} 轮：点 {i} → 簇 {best + 1}",
                    Extra = MlFrame.Snapshot(points, labels, assign.ToArray(), centroids.ToArray(),
                        highlight: new[] { i })
                });
            }

            // 更新步
            var sumX = new double[k];
            var sumY = new double[k];
            var count = new int[k];

            for (int i = 0; i < n; i++)
            {
                int c = assign[i];
                if (c < 0) continue;
                sumX[c] += pts[i].X;
                sumY[c] += pts[i].Y;
                count[c]++;
            }

            for (int c = 0; c < k; c++)
            {
                if (count[c] == 0) continue;
                centroids[c] = new PointF(
                    (float)(sumX[c] / count[c]),
                    (float)(sumY[c] / count[c]));
            }

            recorder.Record(new AnimStep
            {
                Op = AnimOp.Move,
                CodeLine = 3,
                Description = $"第 {iter + 1} 轮：质心移动到各簇均值",
                Extra = MlFrame.Snapshot(points, labels, assign.ToArray(), centroids.ToArray())
            });

            if (!changed) break;
        }
    }
}
