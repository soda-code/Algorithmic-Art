using System.Drawing.Drawing2D;
using AlgorithmicArt.Core.Algorithm;

namespace AlgorithmicArt.Render;

/// <summary>轨迹渲染器（汽车类）：左侧标注纵坐标（0~100）。</summary>
public class TrajectoryRenderer : IAnimRenderer
{
    private static readonly Color AxisTextColor = Color.FromArgb(138, 148, 166);
    private static readonly Color AxisColor = Color.FromArgb(214, 220, 228);
    private static readonly Color GridColor = Color.FromArgb(238, 241, 246);

    public void Render(Graphics g, Rectangle bounds, AlgorithmContext ctx, AnimStep? current)
    {
        var plot = new Rectangle(
            bounds.X + 54,
            bounds.Y + 16,
            Math.Max(1, bounds.Width - 54 - 22),
            Math.Max(1, bounds.Height - 16 - 34));
        if (plot.Width <= 0 || plot.Height <= 0) return;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var axisFont = new Font("Microsoft YaHei UI", 7.5f);
        using var gridPen = new Pen(GridColor, 1f) { DashStyle = DashStyle.Dot };
        using var axisPen = new Pen(AxisColor, 1f);

        // 纵坐标刻度 + 水平网格（坐标范围 0~100）
        const int ticks = 4;
        for (int k = 0; k <= ticks; k++)
        {
            int v = 100 * k / ticks;
            float y = plot.Bottom - v / 100f * plot.Height;

            if (k is > 0 and < ticks)
                g.DrawLine(gridPen, plot.Left, y, plot.Right, y);

            TextRenderer.DrawText(g, v.ToString(), axisFont,
                new Rectangle(bounds.X, (int)Math.Round(y) - 8, 48, 16), AxisTextColor,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        g.DrawLine(axisPen, plot.Left, plot.Top, plot.Left, plot.Bottom);
        g.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);

        // 参考路径（虚线）
        if (ctx.Path is { Count: > 1 })
        {
            using var pen = new Pen(Color.FromArgb(170, 176, 186), 1.5f)
            {
                DashStyle = DashStyle.Dash
            };
            DrawPolyline(g, pen, plot, ctx.Path);
        }

        // 实际轨迹（实线），优先取当前步骤快照
        var trace = current?.Extra as PointF[] ?? ctx.Trace?.ToArray();
        if (trace is { Length: > 1 })
        {
            using var pen = new Pen(Color.FromArgb(24, 128, 220), 2f);
            DrawPolyline(g, pen, plot, trace);
        }

        // 车辆当前位置
        if (trace is { Length: > 0 })
        {
            var pos = trace[^1];
            float x = plot.Left + pos.X / 100f * plot.Width;
            float y = plot.Bottom - pos.Y / 100f * plot.Height;

            // 方向箭头（依据最后两点）
            if (trace.Length >= 2)
            {
                var prev = trace[^2];
                float dx = pos.X - prev.X;
                float dy = pos.Y - prev.Y;
                float len = (float)Math.Sqrt(dx * dx + dy * dy);
                if (len > 1e-3f)
                {
                    float ux = dx / len, uy = dy / len;
                    using var arrowPen = new Pen(Color.FromArgb(240, 120, 40), 2.5f)
                    {
                        EndCap = LineCap.ArrowAnchor
                    };
                    g.DrawLine(arrowPen, x, y, x + ux * 16, y - uy * 16);
                }
            }

            using var carBrush = new SolidBrush(Color.FromArgb(240, 120, 40));
            using var halo = new Pen(Color.FromArgb(140, 240, 160, 60), 2f);
            g.FillEllipse(carBrush, x - 5, y - 5, 10, 10);
            g.DrawEllipse(halo, x - 10, y - 10, 20, 20);
        }
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
