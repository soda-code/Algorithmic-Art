using System.Drawing.Drawing2D;
using AlgorithmicArt.Core.Algorithm;

namespace AlgorithmicArt.Render;

/// <summary>PID 循迹渲染器：上方轨迹，下方误差与 P/I/D 分量曲线。</summary>
public class PidRenderer : IAnimRenderer
{
    private static readonly Color PathColor = Color.FromArgb(170, 176, 186);
    private static readonly Color TraceColor = Color.FromArgb(37, 99, 235);
    private static readonly Color CarColor = Color.FromArgb(240, 120, 40);
    private static readonly Color ErrorColor = Color.FromArgb(60, 68, 78);
    private static readonly Color PColor = Color.FromArgb(37, 99, 235);
    private static readonly Color IColor = Color.FromArgb(52, 168, 118);
    private static readonly Color DColor = Color.FromArgb(245, 158, 40);
    private static readonly Color GridColor = Color.FromArgb(238, 241, 246);
    private static readonly Color AxisTextColor = Color.FromArgb(138, 148, 166);

    public void Render(Graphics g, Rectangle bounds, AlgorithmContext ctx, AnimStep? current)
    {
        var frame = current?.Extra as PidFrame;
        var trace = frame?.Trace ?? ctx.Trace?.ToArray() ?? Array.Empty<PointF>();
        var samples = frame?.Samples ?? Array.Empty<PidSample>();

        int topHeight = (int)(bounds.Height * 0.56);
        var topRect = new Rectangle(bounds.X + 8, bounds.Y + 8,
            Math.Max(40, bounds.Width - 16), Math.Max(40, topHeight - 16));
        var bottomRect = new Rectangle(bounds.X + 52, topRect.Bottom + 24,
            Math.Max(40, bounds.Width - 68), Math.Max(20, bounds.Height - topHeight - 48));

        RenderTrajectory(g, topRect, ctx, trace);
        if (bottomRect.Height > 20) RenderCurves(g, bottomRect, samples);
    }

    // ---------- 上方：轨迹 ----------

    private static void RenderTrajectory(Graphics g, Rectangle plot, AlgorithmContext ctx, PointF[] trace)
    {
        if (plot.Width <= 0 || plot.Height <= 0) return;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        if (ctx.Path is { Count: > 1 })
        {
            using var pen = new Pen(PathColor, 1.5f) { DashStyle = DashStyle.Dash };
            DrawPolyline(g, pen, plot, ctx.Path);
        }

        if (trace.Length > 1)
        {
            using var pen = new Pen(TraceColor, 2f) { LineJoin = LineJoin.Round };
            DrawPolyline(g, pen, plot, trace);
        }

        if (trace.Length > 0)
        {
            var pos = trace[^1];
            float x = plot.Left + pos.X / 100f * plot.Width;
            float y = plot.Bottom - pos.Y / 100f * plot.Height;

            if (trace.Length >= 2)
            {
                var prev = trace[^2];
                float dx = pos.X - prev.X, dy = pos.Y - prev.Y;
                float len = (float)Math.Sqrt(dx * dx + dy * dy);
                if (len > 1e-3f)
                {
                    using var arrow = new Pen(CarColor, 2.5f) { EndCap = LineCap.ArrowAnchor };
                    g.DrawLine(arrow, x, y, x + dx / len * 16, y - dy / len * 16);
                }
            }

            using var halo = new Pen(Color.FromArgb(60, CarColor), 6f);
            g.DrawEllipse(halo, x - 8, y - 8, 16, 16);
            using var brush = new SolidBrush(CarColor);
            g.FillEllipse(brush, x - 5, y - 5, 10, 10);
        }
    }

    // ---------- 下方：误差 + P/I/D 曲线 ----------

    private static void RenderCurves(Graphics g, Rectangle plot, PidSample[] samples)
    {
        if (samples.Length < 2) return;

        using var gridPen = new Pen(GridColor, 1f) { DashStyle = DashStyle.Dot };
        using var axisPen = new Pen(Color.FromArgb(214, 220, 228), 1f);
        using var labelFont = new Font("Microsoft YaHei UI", 7.5f);
        using var legendFont = new Font("Microsoft YaHei UI", 8f);

        double min = double.MaxValue, max = double.MinValue;
        foreach (var s in samples)
        {
            min = Math.Min(min, Math.Min(Math.Min(s.Error, s.P), Math.Min(s.I, s.D)));
            max = Math.Max(max, Math.Max(Math.Max(s.Error, s.P), Math.Max(s.I, s.D)));
        }
        if (max - min < 1e-6) { max += 1; min -= 1; }
        double pad = (max - min) * 0.12;
        min -= pad;
        max += pad;

        // 零轴
        float zeroY = plot.Bottom - (float)((0 - min) / (max - min)) * plot.Height;
        if (zeroY > plot.Top && zeroY < plot.Bottom)
        {
            using var zeroPen = new Pen(Color.FromArgb(224, 228, 233), 1f);
            g.DrawLine(zeroPen, plot.Left, zeroY, plot.Right, zeroY);
        }

        for (int k = 0; k <= 4; k++)
        {
            double v = min + (max - min) * k / 4.0;
            float y = plot.Bottom - (float)((v - min) / (max - min)) * plot.Height;
            if (k is > 0 and < 4)
                g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
            TextRenderer.DrawText(g, v.ToString("0.0"), labelFont,
                new Rectangle(4, (int)y - 8, 44, 16), AxisTextColor,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        g.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);

        DrawCurve(g, plot, samples, s => s.Error, ErrorColor, 1.8f, min, max);
        DrawCurve(g, plot, samples, s => s.P, PColor, 1.1f, min, max);
        DrawCurve(g, plot, samples, s => s.I, IColor, 1.1f, min, max);
        DrawCurve(g, plot, samples, s => s.D, DColor, 1.1f, min, max);

        int lx = plot.Left, ly = plot.Top - 18;
        DrawLegend(g, legendFont, ref lx, ly, "误差 e", ErrorColor);
        DrawLegend(g, legendFont, ref lx, ly, "P", PColor);
        DrawLegend(g, legendFont, ref lx, ly, "I", IColor);
        DrawLegend(g, legendFont, ref lx, ly, "D", DColor);

        // 响应指标
        float maxAbs = 0f, sumSq = 0f;
        foreach (var s in samples)
        {
            float a = Math.Abs(s.Error);
            if (a > maxAbs) maxAbs = a;
            sumSq += s.Error * s.Error;
        }
        float rms = (float)Math.Sqrt(sumSq / samples.Length);
        float cur = Math.Abs(samples[^1].Error);

        string metric = $"峰值误差 {maxAbs:0.00}｜RMS {rms:0.00}｜当前 {cur:0.00}";
        TextRenderer.DrawText(g, metric, legendFont,
            new Rectangle(plot.Left, plot.Top - 40, plot.Width, 18), AxisTextColor,
            TextFormatFlags.Right | TextFormatFlags.Top);
    }

    private static void DrawCurve(Graphics g, Rectangle plot, PidSample[] samples,
        Func<PidSample, float> selector, Color color, float width, double min, double max)
    {
        using var pen = new Pen(color, width) { LineJoin = LineJoin.Round };
        PointF? prev = null;
        for (int i = 0; i < samples.Length; i++)
        {
            float v = selector(samples[i]);
            float x = plot.Left + (float)i / Math.Max(samples.Length - 1, 1) * plot.Width;
            float y = plot.Bottom - (float)((v - min) / (max - min)) * plot.Height;
            var p = new PointF(x, y);
            if (prev.HasValue) g.DrawLine(pen, prev.Value, p);
            prev = p;
        }
    }

    private static void DrawLegend(Graphics g, Font font, ref int x, int y, string text, Color color)
    {
        using var brush = new SolidBrush(color);
        g.FillRectangle(brush, x, y + 5, 12, 3);
        TextRenderer.DrawText(g, text, font, new Point(x + 16, y), AxisTextColor);
        x += 30 + TextRenderer.MeasureText(text, font).Width;
    }

    private static void DrawPolyline(Graphics g, Pen pen, Rectangle plot, IReadOnlyList<PointF> points)
    {
        if (points.Count < 2) return;
        var pts = new PointF[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            pts[i] = new PointF(
                plot.Left + points[i].X / 100f * plot.Width,
                plot.Bottom - points[i].Y / 100f * plot.Height);
        }
        g.DrawLines(pen, pts);
    }
}
