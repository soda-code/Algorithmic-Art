using System.Drawing.Drawing2D;
using AlgorithmicArt.Core.Algorithm;

namespace AlgorithmicArt.Render;

/// <summary>机器学习散点渲染器：聚类质心 / 回归拟合线 / 分类近邻 + 坐标轴数值。</summary>
public class ScatterRenderer : IAnimRenderer
{
    private static readonly Color[] ClusterColors =
    {
        Color.FromArgb(59, 130, 246),   // 蓝
        Color.FromArgb(245, 158, 11),   // 橙
        Color.FromArgb(16, 185, 129),   // 绿
        Color.FromArgb(139, 92, 246),   // 紫
        Color.FromArgb(236, 72, 153)    // 粉
    };

    private static readonly Color UnassignedColor = Color.FromArgb(176, 184, 196);
    private static readonly Color RegressionColor = Color.FromArgb(239, 68, 68);
    private static readonly Color QueryColor = Color.FromArgb(220, 38, 38);
    private static readonly Color CentroidColor = Color.FromArgb(30, 34, 42);
    private static readonly Color AxisColor = Color.FromArgb(206, 213, 222);
    private static readonly Color AxisTextColor = Color.FromArgb(138, 148, 166);
    private static readonly Color GridColor = Color.FromArgb(240, 243, 247);
    private static readonly Color TitleColor = Color.FromArgb(56, 62, 72);

    public void Render(Graphics g, Rectangle bounds, AlgorithmContext ctx, AnimStep? current)
    {
        var frame = current?.Extra as MlFrame;

        var points = frame?.Points ?? BuildPoints(ctx.MlPoints);
        if (points.Length == 0) return;

        g.SmoothingMode = SmoothingMode.AntiAlias;

        var plot = new Rectangle(
            bounds.X + 52,
            bounds.Y + 22,
            Math.Max(80, bounds.Width - 52 - 112),
            Math.Max(80, bounds.Height - 22 - 38));

        var side = new Rectangle(plot.Right + 10, plot.Top, 92, plot.Height);

        DrawGridAndAxes(g, plot);

        // 拟合直线（裁剪到绘图区）
        if (frame?.Line is { Length: 2 })
        {
            var state = g.Save();
            g.SetClip(plot);
            using (var pen = new Pen(RegressionColor, 2f) { LineJoin = LineJoin.Round })
            {
                g.DrawLine(pen,
                    MapX(plot, frame.Line[0].X), MapY(plot, frame.Line[0].Y),
                    MapX(plot, frame.Line[1].X), MapY(plot, frame.Line[1].Y));
            }
            g.Restore(state);
        }

        // 散点
        var clusterOf = frame?.ClusterOf;
        var labels = frame?.Labels;
        var highlight = new HashSet<int>(frame?.Highlight ?? Array.Empty<int>());

        for (int i = 0; i < points.Length; i++)
        {
            int group = -1;
            if (clusterOf != null && i < clusterOf.Length && clusterOf[i] >= 0)
                group = clusterOf[i];
            else if (labels != null && i < labels.Length && labels[i] >= 0)
                group = labels[i];

            Color color = group >= 0 ? ClusterColors[group % ClusterColors.Length] : UnassignedColor;
            float x = MapX(plot, points[i].X);
            float y = MapY(plot, points[i].Y);
            bool isHighlight = highlight.Contains(i);
            float radius = isHighlight ? 6.5f : 4.2f;

            if (isHighlight)
            {
                using var halo = new SolidBrush(Color.FromArgb(64, color));
                g.FillEllipse(halo, x - 11, y - 11, 22, 22);
            }

            using (var brush = new SolidBrush(color))
                g.FillEllipse(brush, x - radius, y - radius, radius * 2, radius * 2);
            using (var pen = new Pen(Color.White, 1.2f))
                g.DrawEllipse(pen, x - radius, y - radius, radius * 2, radius * 2);
        }

        // 质心（十字 + 圆圈）
        if (frame?.Centroids is { Length: > 0 })
        {
            using var pen = new Pen(CentroidColor, 2f);
            foreach (var c in frame.Centroids)
            {
                float x = MapX(plot, c.X);
                float y = MapY(plot, c.Y);
                g.DrawLine(pen, x - 9, y, x + 9, y);
                g.DrawLine(pen, x, y - 9, x, y + 9);
                g.DrawEllipse(pen, x - 10, y - 10, 20, 20);
            }
        }

        // 待分类查询点（菱形）
        if (frame?.Query is PointF q)
        {
            float x = MapX(plot, q.X);
            float y = MapY(plot, q.Y);

            using (var halo = new SolidBrush(Color.FromArgb(56, QueryColor)))
                g.FillEllipse(halo, x - 13, y - 13, 26, 26);

            var diamond = new[]
            {
                new PointF(x, y - 9),
                new PointF(x + 9, y),
                new PointF(x, y + 9),
                new PointF(x - 9, y)
            };
            using (var brush = new SolidBrush(QueryColor))
                g.FillPolygon(brush, diamond);
            using (var pen = new Pen(Color.White, 1.8f))
                g.DrawPolygon(pen, diamond);
        }

        DrawSidePanel(g, side, frame);
    }

    private static void DrawGridAndAxes(Graphics g, Rectangle plot)
    {
        using var gridPen = new Pen(GridColor, 1f) { DashStyle = DashStyle.Dot };
        using var axisPen = new Pen(AxisColor, 1f);
        using var font = new Font("Microsoft YaHei UI", 7.5f);

        // 0~100 坐标，每 25 一格
        for (int k = 0; k <= 4; k++)
        {
            int v = 25 * k;
            float y = plot.Bottom - v / 100f * plot.Height;
            float x = plot.Left + v / 100f * plot.Width;

            if (k is > 0 and < 4)
            {
                g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                g.DrawLine(gridPen, x, plot.Top, x, plot.Bottom);
            }

            // 纵坐标数值
            TextRenderer.DrawText(g, v.ToString(), font,
                new Rectangle(plot.Left - 46, (int)Math.Round(y) - 8, 40, 16), AxisTextColor,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

            // 横坐标数值
            TextRenderer.DrawText(g, v.ToString(), font,
                new Rectangle((int)Math.Round(x) - 20, plot.Bottom + 6, 40, 16), AxisTextColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
        }

        g.DrawLine(axisPen, plot.Left, plot.Top, plot.Left, plot.Bottom);
        g.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);

        TextRenderer.DrawText(g, "Y", font,
            new Rectangle(plot.Left - 46, plot.Top - 20, 40, 16), AxisTextColor,
            TextFormatFlags.Right | TextFormatFlags.Top);
        TextRenderer.DrawText(g, "X", font,
            new Rectangle(plot.Right - 16, plot.Bottom + 6, 20, 16), AxisTextColor,
            TextFormatFlags.Left | TextFormatFlags.Top);
    }

    private static void DrawSidePanel(Graphics g, Rectangle side, MlFrame? frame)
    {
        using var titleFont = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold);
        using var labelFont = new Font("Microsoft YaHei UI", 8.5f);

        int x = side.Left;
        int y = side.Top + 4;

        // 图例
        TextRenderer.DrawText(g, "图例", titleFont, new Point(x, y), TitleColor);
        y += 24;

        int clusterCount = frame?.Centroids is { Length: > 0 } c ? c.Length : 0;
        if (clusterCount == 0 && frame?.Labels is { Length: > 0 } labels)
            clusterCount = Math.Max(2, labels.Max() + 1);

        for (int i = 0; i < Math.Min(clusterCount, ClusterColors.Length); i++)
        {
            using (var brush = new SolidBrush(ClusterColors[i]))
                g.FillEllipse(brush, x, y + 4, 9, 9);
            TextRenderer.DrawText(g, $"簇 {i + 1}", labelFont, new Point(x + 15, y), TitleColor);
            y += 20;
        }

        if (frame?.Centroids is { Length: > 0 })
        {
            using (var pen = new Pen(CentroidColor, 1.8f))
            {
                g.DrawLine(pen, x + 1, y + 8, x + 8, y + 8);
                g.DrawLine(pen, x + 4.5f, y + 4, x + 4.5f, y + 12);
            }
            TextRenderer.DrawText(g, "质心", labelFont, new Point(x + 15, y), TitleColor);
            y += 20;
        }

        if (frame?.Line is { Length: 2 })
        {
            using (var pen = new Pen(RegressionColor, 2f))
                g.DrawLine(pen, x, y + 8, x + 12, y + 8);
            TextRenderer.DrawText(g, "拟合线", labelFont, new Point(x + 15, y), TitleColor);
            y += 20;
        }

        if (frame?.Query != null)
        {
            using (var pen = new Pen(QueryColor, 1.8f))
                g.DrawRectangle(pen, x + 1, y + 4, 8, 8);
            TextRenderer.DrawText(g, "待测点", labelFont, new Point(x + 15, y), TitleColor);
            y += 20;
        }

        // 损失曲线（MSE）
        if (frame?.Loss is { Length: > 0 } loss)
        {
            y += 8;
            using (var sep = new Pen(Color.FromArgb(232, 236, 242), 1f))
                g.DrawLine(sep, x, y, x + 78, y);
            y += 8;

            TextRenderer.DrawText(g, "MSE", titleFont, new Point(x, y), TitleColor);
            y += 22;
            TextRenderer.DrawText(g, loss[^1].ToString("0.00"), labelFont, new Point(x, y), RegressionColor);
            y += 20;

            DrawMiniLoss(g, new Rectangle(x, y, 78, Math.Max(30, side.Bottom - y - 4)), loss);
        }
    }

    private static void DrawMiniLoss(Graphics g, Rectangle box, float[] loss)
    {
        if (loss.Length < 2 || box.Width < 10 || box.Height < 10) return;

        using var border = new Pen(Color.FromArgb(226, 232, 240), 1f);
        g.DrawRectangle(border, box);

        float max = Math.Max(loss.Max(), 1e-6f);
        var pts = new PointF[loss.Length];
        for (int i = 0; i < loss.Length; i++)
        {
            pts[i] = new PointF(
                box.Left + (float)i / (loss.Length - 1) * (box.Width - 2),
                box.Bottom - 1 - loss[i] / max * (box.Height - 2));
        }

        using var pen = new Pen(RegressionColor, 1.4f) { LineJoin = LineJoin.Round };
        g.DrawLines(pen, pts);
    }

    private static PointF[] BuildPoints(MlPoint[]? source)
    {
        if (source == null || source.Length == 0) return Array.Empty<PointF>();
        var pts = new PointF[source.Length];
        for (int i = 0; i < source.Length; i++)
            pts[i] = new PointF(source[i].X, source[i].Y);
        return pts;
    }

    private static float MapX(Rectangle plot, float x) => plot.Left + x / 100f * plot.Width;
    private static float MapY(Rectangle plot, float y) => plot.Bottom - y / 100f * plot.Height;
}
