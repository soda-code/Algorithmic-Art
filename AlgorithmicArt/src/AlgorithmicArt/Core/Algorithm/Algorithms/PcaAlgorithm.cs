using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>PCA 主成分分析：由协方差矩阵的特征向量找方差最大方向。</summary>
public class PcaAlgorithm : IAlgorithm
{
    public string Category => "其他";
    public string Name => "PCA 主成分";
    public string Description => "中心化后求协方差矩阵，最大特征值方向即第一主成分";
    public RendererKind Renderer => RendererKind.Scatter;
    public SceneKind Scene => SceneKind.PcaScatter;

    public long EstimateSteps(int n) => 10;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "计算各维度均值",
        "中心化：x' = x - mean",
        "协方差矩阵 C = X'ᵀX' / (n-1)",
        "求 C 的特征值 λ₁ ≥ λ₂",
        "第一主成分 = λ₁ 对应特征向量"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var pts = ctx.MlPoints;
        if (pts == null || pts.Length < 3) return;

        int n = pts.Length;
        var points = new PointF[n];
        for (int i = 0; i < n; i++)
            points[i] = new PointF(pts[i].X, pts[i].Y);

        var labels = Array.Empty<int>();

        // 均值
        double mx = 0, my = 0;
        for (int i = 0; i < n; i++)
        {
            mx += points[i].X;
            my += points[i].Y;
        }
        mx /= n;
        my /= n;

        var mean = new PointF((float)mx, (float)my);

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Assign,
            CodeLine = 0,
            Description = $"样本均值 = ({mx:0.0}, {my:0.0})",
            Extra = MlFrame.Snapshot(points, labels, centroids: new[] { mean })
        });

        // 协方差
        double sxx = 0, syy = 0, sxy = 0;
        for (int i = 0; i < n; i++)
        {
            double dx = points[i].X - mx;
            double dy = points[i].Y - my;
            sxx += dx * dx;
            syy += dy * dy;
            sxy += dx * dy;
        }

        int denom = Math.Max(1, n - 1);
        sxx /= denom;
        syy /= denom;
        sxy /= denom;

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Assign,
            CodeLine = 2,
            Description = $"协方差矩阵 [[{sxx:0.0}, {sxy:0.0}], [{sxy:0.0}, {syy:0.0}]]",
            Extra = MlFrame.Snapshot(points, labels, centroids: new[] { mean })
        });

        // 2×2 对称矩阵特征值
        double tr = sxx + syy;
        double det = sxx * syy - sxy * sxy;
        double disc = Math.Sqrt(Math.Max(0, tr * tr / 4 - det));
        double l1 = tr / 2 + disc;
        double l2 = tr / 2 - disc;

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Assign,
            CodeLine = 3,
            Description = $"特征值 λ₁ = {l1:0.0}，λ₂ = {l2:0.0}",
            Extra = MlFrame.Snapshot(points, labels, centroids: new[] { mean })
        });

        // 第一主成分方向
        double ex = sxy;
        double ey = l1 - sxx;

        if (Math.Abs(ex) < 1e-9 && Math.Abs(ey) < 1e-9)
        {
            ex = l1 - syy;
            ey = sxy;
        }

        double elen = Math.Sqrt(ex * ex + ey * ey);
        if (elen < 1e-9)
        {
            ex = 1;
            ey = 0;
            elen = 1;
        }
        ex /= elen;
        ey /= elen;

        double half = Math.Sqrt(Math.Max(l1, 0)) * 1.8;
        var axis = new[]
        {
            new PointF((float)(mx - ex * half), (float)(my - ey * half)),
            new PointF((float)(mx + ex * half), (float)(my + ey * half))
        };

        double ratio = Math.Max(l1, 0) / Math.Max(l1 + Math.Max(l2, 0), 1e-9);

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 4,
            Description = $"第一主成分方向 ({ex:0.00}, {ey:0.00})，解释方差 {ratio:P0}",
            Extra = MlFrame.Snapshot(points, labels, line: axis, centroids: new[] { mean })
        });
    }
}
