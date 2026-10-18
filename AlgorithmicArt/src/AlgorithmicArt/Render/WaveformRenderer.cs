using System.Drawing.Drawing2D;
using AlgorithmicArt.Core.Algorithm;

namespace AlgorithmicArt.Render;

/// <summary>波形渲染器（现代风格）：Y 轴数值、X 轴时间，渐变区域 + 图例。</summary>
public class WaveformRenderer : IAnimRenderer
{
    private static readonly Color GridColor = Color.FromArgb(238, 241, 246);
    private static readonly Color AxisTextColor = Color.FromArgb(138, 148, 166);
    private static readonly Color RawColor = Color.FromArgb(178, 188, 202);
    private static readonly Color AccentColor = Color.FromArgb(37, 99, 235);
    private static readonly Color HighlightColor = Color.FromArgb(255, 122, 69);

    public void Render(Graphics g, Rectangle bounds, AlgorithmContext ctx, AnimStep? current)
    {
        var signal = ctx.Signal;
        if (signal == null || signal.Length == 0) return;

        var plot = new Rectangle(bounds.X + 58, bounds.Y + 30,
            bounds.Width - 58 - 18, bounds.Height - 30 - 38);
        if (plot.Width <= 0 || plot.Height <= 0) return;

        double min = signal.Min();
        double max = signal.Max();
        if (max - min < 1e-9) max = min + 1;
        double pad = (max - min) * 0.12;
        min -= pad;
        max += pad;

        double fs = ctx.Parameters.GetValueOrDefault("Fs", 1000);
        double totalTime = (signal.Length - 1) / Math.Max(fs, 1e-9);

        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var gridPen = new Pen(GridColor, 1f) { DashStyle = DashStyle.Dot };
        using var axisPen = new Pen(Color.FromArgb(214, 220, 228), 1f);
        using var labelFont = new Font("Microsoft YaHei UI", 7.5f);
        using var legendFont = new Font("Microsoft YaHei UI", 8f);

        // 水平网格 + Y 轴数值
        for (int k = 0; k <= 4; k++)
        {
            double v = min + (max - min) * k / 4.0;
            float y = plot.Bottom - (float)((v - min) / (max - min)) * plot.Height;

            if (k is > 0 and < 4)
                g.DrawLine(gridPen, plot.Left, y, plot.Right, y);

            TextRenderer.DrawText(g, FormatValue(v), labelFont,
                new Rectangle(4, (int)y - 8, 50, 16), AxisTextColor,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        // 垂直网格 + X 轴时间
        for (int k = 0; k <= 4; k++)
        {
            double t = totalTime * k / 4.0;
            float x = plot.Left + (float)k / 4 * plot.Width;

            if (k is > 0 and < 4)
                g.DrawLine(gridPen, x, plot.Top, x, plot.Bottom);

            TextRenderer.DrawText(g, FormatTime(t), labelFont,
                new Rectangle((int)x - 30, plot.Bottom + 5, 60, 16), AxisTextColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
        }

        // 基线
        g.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);

        // 原始信号（浅灰）
        using (var rawPen = new Pen(RawColor, 1.2f))
            DrawSeries(g, rawPen, plot, signal, signal.Length, min, max);

        // 滤波输出（渐变区域 + 主色曲线）
        var filtered = current?.Extra as double[];
        if (filtered is { Length: > 1 })
        {
            var pts = BuildPoints(plot, filtered, signal.Length, min, max);

            using (var area = new GraphicsPath())
            {
                area.AddLines(pts);
                area.AddLine(pts[^1].X, plot.Bottom, pts[0].X, plot.Bottom);
                area.CloseFigure();
                using var brush = new LinearGradientBrush(
                    new Rectangle(plot.Left, plot.Top, Math.Max(1, plot.Width), Math.Max(1, plot.Height)),
                    Color.FromArgb(56, AccentColor), Color.FromArgb(0, AccentColor),
                    LinearGradientMode.Vertical);
                g.FillPath(brush, area);
            }

            using var linePen = new Pen(AccentColor, 2f) { LineJoin = LineJoin.Round };
            g.DrawLines(linePen, pts);
        }

        // 当前采样点：垂直参考线 + 光晕圆点
        if (current?.Points is { Length: > 0 })
        {
            foreach (var pt in current.Points)
            {
                float x = plot.Left + pt.X / Math.Max(signal.Length - 1, 1) * plot.Width;
                float y = plot.Bottom - (float)((pt.Y - min) / (max - min)) * plot.Height;

                using (var vPen = new Pen(Color.FromArgb(90, HighlightColor), 1f) { DashStyle = DashStyle.Dash })
                    g.DrawLine(vPen, x, plot.Top, x, plot.Bottom);

                using (var halo = new SolidBrush(Color.FromArgb(48, HighlightColor)))
                    g.FillEllipse(halo, x - 9, y - 9, 18, 18);

                using var dot = new SolidBrush(HighlightColor);
                g.FillEllipse(dot, x - 4, y - 4, 8, 8);
                using var white = new Pen(Color.White, 1.6f);
                g.DrawEllipse(white, x - 4, y - 4, 8, 8);
            }
        }

        DrawLegend(g, legendFont, plot, filtered is { Length: > 1 });
    }

    private static PointF[] BuildPoints(Rectangle plot, double[] data, int total, double min, double max)
    {
        var pts = new PointF[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            pts[i] = new PointF(
                plot.Left + (float)i / Math.Max(total - 1, 1) * plot.Width,
                plot.Bottom - (float)((data[i] - min) / (max - min)) * plot.Height);
        }
        return pts;
    }

    private static void DrawSeries(Graphics g, Pen pen, Rectangle plot, double[] data, int total, double min, double max)
    {
        if (data.Length < 2) return;
        g.DrawLines(pen, BuildPoints(plot, data, total, min, max));
    }

    private static void DrawLegend(Graphics g, Font font, Rectangle plot, bool hasFiltered)
    {
        int y = Math.Max(2, plot.Top - 18);
        int x = plot.Right - (hasFiltered ? 180 : 92);

        using (var rawBrush = new SolidBrush(RawColor))
            g.FillEllipse(rawBrush, x, y + 4, 8, 8);
        TextRenderer.DrawText(g, "原始信号", font, new Point(x + 12, y), AxisTextColor);

        if (hasFiltered)
        {
            x += 92;
            using var accBrush = new SolidBrush(AccentColor);
            g.FillEllipse(accBrush, x, y + 4, 8, 8);
            TextRenderer.DrawText(g, "滤波输出", font, new Point(x + 12, y), AxisTextColor);
        }
    }

    private static string FormatValue(double v)
    {
        double a = Math.Abs(v);
        if (a >= 100) return v.ToString("0");
        if (a >= 1) return v.ToString("0.0");
        return v.ToString("0.00");
    }

    private static string FormatTime(double t) => t >= 1
        ? t.ToString("0.0") + "s"
        : (t * 1000).ToString("0") + "ms";
}
