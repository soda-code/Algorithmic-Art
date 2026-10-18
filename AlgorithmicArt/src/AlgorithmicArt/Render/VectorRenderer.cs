using System.Drawing.Drawing2D;
using AlgorithmicArt.Core.Algorithm;

namespace AlgorithmicArt.Render;

/// <summary>二维向量渲染器：显示迭代向量方向与目标特征向量。</summary>
public class VectorRenderer : IAnimRenderer
{
    private static readonly Color AxisColor = Color.FromArgb(206, 213, 222);
    private static readonly Color GridColor = Color.FromArgb(240, 243, 247);
    private static readonly Color AxisTextColor = Color.FromArgb(138, 148, 166);
    private static readonly Color VectorColor = Color.FromArgb(37, 99, 235);
    private static readonly Color TargetColor = Color.FromArgb(245, 130, 40);
    private static readonly Color LabelColor = Color.FromArgb(56, 62, 72);

    public void Render(Graphics g, Rectangle bounds, AlgorithmContext ctx, AnimStep? current)
    {
        var frame = current?.Extra as VectorFrame;
        if (frame == null) return;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var plot = new Rectangle(
            bounds.X + 50,
            bounds.Y + 24,
            Math.Max(60, bounds.Width - 50 - 30),
            Math.Max(60, bounds.Height - 24 - 42));

        const float extent = 1.2f;
        float scale = Math.Min(plot.Width, plot.Height) / (2 * extent);
        float cx = plot.Left + plot.Width / 2f;
        float cy = plot.Top + plot.Height / 2f;

        PointF Map(float x, float y) => new(cx + x * scale, cy - y * scale);

        using var gridPen = new Pen(GridColor, 1f) { DashStyle = DashStyle.Dot };
        using var axisPen = new Pen(AxisColor, 1f);
        using var font = new Font("Microsoft YaHei UI", 7.5f);

        // 网格与刻度
        for (int k = -1; k <= 1; k++)
        {
            if (k == 0) continue;

            float px = Map(k * 0.5f, 0).X;
            float py = Map(0, k * 0.5f).Y;

            g.DrawLine(gridPen, px, plot.Top, px, plot.Bottom);
            g.DrawLine(gridPen, plot.Left, py, plot.Right, py);

            TextRenderer.DrawText(g, (k * 0.5f).ToString("0.0"), font,
                new Rectangle((int)px - 18, (int)cy + 4, 36, 14), AxisTextColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
        }

        // 坐标轴
        g.DrawLine(axisPen, plot.Left, cy, plot.Right, cy);
        g.DrawLine(axisPen, cx, plot.Top, cx, plot.Bottom);

        // 目标方向（特征向量）
        if (frame.Target is PointF target)
        {
            var end = Map(target.X, target.Y);
            using var pen = new Pen(TargetColor, 1.6f) { DashStyle = DashStyle.Dash };
            g.DrawLine(pen, cx, cy, end.X, end.Y);
            TextRenderer.DrawText(g, "特征向量方向", font,
                new Point((int)end.X + 6, (int)end.Y - 18), TargetColor);
        }

        // 当前向量
        for (int i = 0; i < frame.Vectors.Length; i++)
        {
            var v = frame.Vectors[i];
            var end = Map(v.X, v.Y);

            using var pen = new Pen(VectorColor, 2.4f) { EndCap = LineCap.ArrowAnchor };
            g.DrawLine(pen, cx, cy, end.X, end.Y);

            using var dot = new SolidBrush(VectorColor);
            g.FillEllipse(dot, end.X - 3.5f, end.Y - 3.5f, 7, 7);

            if (i < frame.Labels.Length && !string.IsNullOrEmpty(frame.Labels[i]))
                TextRenderer.DrawText(g, frame.Labels[i], font,
                    new Point((int)end.X + 7, (int)end.Y + 2), LabelColor);
        }
    }
}
