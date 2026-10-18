using System.Drawing.Drawing2D;
using AlgorithmicArt.Core.Algorithm;

namespace AlgorithmicArt.Render;

/// <summary>P / PI / PID 三种响应曲线对比渲染器。</summary>
public class PidCompareRenderer : IAnimRenderer
{
    private static readonly Color PColor = Color.FromArgb(229, 57, 53);
    private static readonly Color PIColor = Color.FromArgb(30, 136, 229);
    private static readonly Color PIDColor = Color.FromArgb(67, 160, 71);
    private static readonly Color SpColor = Color.FromArgb(150, 158, 170);
    private static readonly Color StepColor = Color.FromArgb(245, 160, 50);
    private static readonly Color GridColor = Color.FromArgb(238, 241, 246);
    private static readonly Color AxisTextColor = Color.FromArgb(138, 148, 166);

    public void Render(Graphics g, Rectangle bounds, AlgorithmContext ctx, AnimStep? current)
    {
        var frame = current?.Extra as PidCompareFrame;
        if (frame == null || frame.PID.Length < 2) return;

        g.SmoothingMode = SmoothingMode.AntiAlias;

        var plot = new Rectangle(bounds.X + 62, bounds.Y + 44,
            Math.Max(40, bounds.Width - 86), Math.Max(40, bounds.Height - 92));

        using var gridPen = new Pen(GridColor, 1f) { DashStyle = DashStyle.Dot };
        using var axisPen = new Pen(Color.FromArgb(214, 220, 228), 1f);
        using var labelFont = new Font("Microsoft YaHei UI", 8f);
        using var legendFont = new Font("Microsoft YaHei UI", 8.5f);

        float sp = frame.PID[0].Sp;
        float pvMax = Math.Max(frame.P.Max(s => s.Pv),
            Math.Max(frame.PI.Max(s => s.Pv), frame.PID.Max(s => s.Pv)));
        float yMax = Math.Max(sp, pvMax) * 1.2f;
        const float yMin = 0f;

        // 网格 + Y 刻度
        for (int k = 0; k <= 4; k++)
        {
            float v = yMin + (yMax - yMin) * k / 4f;
            float y = plot.Bottom - (v - yMin) / (yMax - yMin) * plot.Height;
            if (k is > 0 and < 4)
                g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
            TextRenderer.DrawText(g, v.ToString("0.0"), labelFont,
                new Rectangle(4, (int)y - 8, 54, 16), AxisTextColor,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        // 坐标轴
        g.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);
        g.DrawLine(axisPen, plot.Left, plot.Top, plot.Left, plot.Bottom);

        // 阶跃扰动标记
        using (var stepPen = new Pen(StepColor, 1.5f) { DashStyle = DashStyle.Dash })
            g.DrawLine(stepPen, plot.Left + 2, plot.Top, plot.Left + 2, plot.Bottom);
        TextRenderer.DrawText(g, "阶跃扰动", labelFont,
            new Rectangle(plot.Left + 7, plot.Top + 2, 80, 16), StepColor,
            TextFormatFlags.Left | TextFormatFlags.Top);

        // SP 水平虚线
        float spY = plot.Bottom - (sp - yMin) / (yMax - yMin) * plot.Height;
        using (var spPen = new Pen(SpColor, 1.4f) { DashStyle = DashStyle.Dash })
            g.DrawLine(spPen, plot.Left, spY, plot.Right, spY);
        TextRenderer.DrawText(g, "SP", labelFont,
            new Rectangle(plot.Right + 4, (int)spY - 8, 30, 16), SpColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        // 三条曲线
        DrawCurve(g, plot, frame.P, PColor, yMin, yMax);
        DrawCurve(g, plot, frame.PI, PIColor, yMin, yMax);
        DrawCurve(g, plot, frame.PID, PIDColor, yMin, yMax);

        // 图例（右侧）
        int lx = Math.Max(plot.Left + 16, plot.Right - 168);
        int ly = plot.Top + 20;
        DrawLegend(g, legendFont, lx, ly, "PID（快速无超调）", PIDColor);
        DrawLegend(g, legendFont, lx, ly + 26, "PI（消除静差）", PIColor);
        DrawLegend(g, legendFont, lx, ly + 52, "纯P（有静差）", PColor);

        // 轴标题
        TextRenderer.DrawText(g, "PV", labelFont,
            new Rectangle(4, plot.Top - 4, 54, 16), AxisTextColor,
            TextFormatFlags.Right | TextFormatFlags.Bottom);
        TextRenderer.DrawText(g, "时间 t", labelFont,
            new Rectangle(plot.Left, plot.Bottom + 8, plot.Width, 16), AxisTextColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
    }

    private static void DrawCurve(Graphics g, Rectangle plot, PidStepSample[] data, Color color, float yMin, float yMax)
    {
        if (data.Length < 2) return;
        var pts = new PointF[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            float x = plot.Left + (float)i / Math.Max(data.Length - 1, 1) * plot.Width;
            float y = plot.Bottom - (data[i].Pv - yMin) / (yMax - yMin) * plot.Height;
            pts[i] = new PointF(x, y);
        }
        using var pen = new Pen(color, 2f) { LineJoin = LineJoin.Round };
        g.DrawLines(pen, pts);
    }

    private static void DrawLegend(Graphics g, Font font, int x, int y, string text, Color color)
    {
        using var brush = new SolidBrush(color);
        g.FillRectangle(brush, x, y + 7, 16, 3);
        TextRenderer.DrawText(g, text, font, new Point(x + 22, y), color);
    }
}
