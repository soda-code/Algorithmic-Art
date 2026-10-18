using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>K 近邻分类：按距离找最近 K 个样本，投票决定待测点类别。</summary>
public class KNearestNeighborsAlgorithm : IAlgorithm
{
    public string Category => "机器学习算法";
    public string Group => "分类";
    public string Name => "K 近邻分类";
    public string Description => "计算待测点到所有样本的距离，取最近 K 个投票";
    public RendererKind Renderer => RendererKind.Scatter;
    public SceneKind Scene => SceneKind.MlClassification;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "读取待测点 q",
        "for 每个样本 p：dist = |p - q|",
        "按距离升序排序",
        "取前 K 个最近邻",
        "多数投票 → 输出预测类别"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var pts = ctx.MlPoints;
        if (pts == null || pts.Length == 0) return;

        var query = ctx.QueryPoint ?? new PointF(50, 50);
        int n = pts.Length;
        int k = Math.Clamp((int)ctx.Parameters.GetValueOrDefault("K", 5), 1, Math.Max(1, n));

        // 点集与标签在整个算法中不变，只构建一次并共享给所有步骤
        var points = new PointF[n];
        for (int i = 0; i < n; i++)
            points[i] = new PointF(pts[i].X, pts[i].Y);

        var labels = pts.Select(p => p.Label).ToArray();
        var dist = new float[n];

        // 逐步计算距离
        for (int i = 0; i < n; i++)
        {
            float dx = pts[i].X - query.X;
            float dy = pts[i].Y - query.Y;
            dist[i] = (float)Math.Sqrt(dx * dx + dy * dy);

            recorder.Record(new AnimStep
            {
                Op = AnimOp.Compare,
                Indices = new[] { i },
                CodeLine = 1,
                Description = $"样本 {i}（类别 {labels[i]}）到待测点距离 {dist[i]:0.0}",
                Extra = MlFrame.Snapshot(points, labels, highlight: new[] { i }, query: query)
            });
        }

        var order = Enumerable.Range(0, n).OrderBy(i => dist[i]).ToArray();
        var neighbors = order.Take(k).ToArray();

        // 逐个点亮近邻
        for (int j = 0; j < neighbors.Length; j++)
        {
            var picked = neighbors.Take(j + 1).ToArray();
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Highlight,
                Indices = picked,
                CodeLine = 3,
                Description = $"第 {j + 1} 近邻：样本 {neighbors[j]}，距离 {dist[neighbors[j]]:0.0}",
                Extra = MlFrame.Snapshot(points, labels, highlight: picked, query: query)
            });
        }

        // 投票
        int votes0 = neighbors.Count(i => labels[i] == 0);
        int votes1 = neighbors.Count(i => labels[i] == 1);
        int predict = votes1 > votes0 ? 1 : 0;

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            Indices = neighbors,
            CodeLine = 4,
            Description = $"投票：类别 0 = {votes0} 票，类别 1 = {votes1} 票 → 预测类别 {predict}",
            Extra = MlFrame.Snapshot(points, labels, highlight: neighbors, query: query)
        });
    }
}
