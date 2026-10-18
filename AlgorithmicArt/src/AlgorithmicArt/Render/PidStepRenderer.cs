using System.Drawing.Drawing2D;
using AlgorithmicArt.Core.Algorithm;

namespace AlgorithmicArt.Render;

/// <summary>PID 阶跃响应渲染器：左侧纵轴刻度 + 曲线 + 右侧 PID 参数栏（避免文字重叠）。</summary>
public class PidStepRenderer : IAnimRenderer
{
    private static readonly Color AxisColor = Color.FromArgb(70, 76, 86);
    private static readonly Color AxisTextColor = Color.FromArgb(120, 128, 140);
    private static readonly Color SpColor = Color.FromArgb(150, 158, 170);
    private static readonly Color CurveColor = Color.FromArgb(67, 160, 71);
    private static readonly Color StepColor = Color.FromArgb(245, 160, 50);
    private static readonly Color InfoBack = Color.FromArgb(248, 250, 253);
    private static readonly Color InfoBorder = Color.FromArgb(228, 234, 242);
    private static readonly Color TitleColor = Color.FromArgb(56, 62, 72);

    public void Render(Graphics g, Rectangle bounds, AlgorithmContext ctx, AnimStep? current)
    {
        var frame = current?.Extra as PidStepFrame;
        var samples = frame?.Samples ?? Array.Empty<PidStepSample>();
        if (samples.Length < 2) return;

        g.SmoothingMode = SmoothingMode.AntiAlias;

        const int axisLeft = 58;
        const int infoWidth = 140;
        const int gap = 14;
        const int top = 34;
        const int bottom = 42;

        var plot = new Rectangle(
            bounds.X + axisLeft,
            bounds.Y + top,
            Math.Max(60, bounds.Width - axisLeft - infoWidth - gap - 8),
            Math.Max(60, bounds.Height - top - bottom));

        var info = new Rectangle(plot.Right + gap, plot.Top, infoWidth, plot.Height);

        RenderCurve(g, plot, samples);

        DrawInfoPanel(g, info,
            ctx.Parameters.GetValueOrDefault("Kp", 0),
            ctx.Parameters.GetValueOrDefault("Ki", 0),
            ctx.Parameters.GetValueOrDefault("Kd", 0),
            samples);
    }

    private static void RenderCurve(Graphics g, Rectangle plot, IReadOnlyList<PidStepSample> samples)
    {
        using var axisPen = new Pen(AxisColor, 1.2f);
        using var labelFont = new Font("Microsoft YaHei UI", 8.5f);

        float sp = samples[0].Sp;
        float pvMax = samples.Max(s => s.Pv);
        float yMax = Math.Max(sp, pvMax) * 1.25f;
        const float yMin = 0f;

        // L 形坐标轴
        g.DrawLine(axisPen, plot.Left, plot.Top, plot.Left, plot.Bottom);
        g.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);

        // 纵坐标刻度 + 水平网格
        using var gridPen = new Pen(Color.FromArgb(238, 241, 246), 1f) { DashStyle = DashStyle.Dot };
        const int ticks = 5;
        for (int k = 0; k <= ticks; k++)
        {
            float v = yMin + (yMax - yMin) * k / ticks;
            float ty = plot.Bottom - (v - yMin) / (yMax - yMin) * plot.Height;

            if (k is > 0 and < ticks)
                g.DrawLine(gridPen, plot.Left, ty, plot.Right, ty);

            TextRenderer.DrawText(g, v.ToString("0.0"), labelFont,
                new Rectangle(plot.Left - 54, (int)Math.Round(ty) - 8, 46, 16), AxisTextColor,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        // 轴标题：PV 放在刻度上方，避开顶部刻度文字
        TextRenderer.DrawText(g, "PV", labelFont,
            new Rectangle(plot.Left - 54, plot.Top - 26, 46, 16), AxisTextColor,
            TextFormatFlags.Right | TextFormatFlags.Top);
        TextRenderer.DrawText(g, "时间 t", labelFont,
            new Rectangle(plot.Left, plot.Bottom + 10, plot.Width, 16), AxisTextColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);

        // SP 目标值（灰色虚线）
        float spY = plot.Bottom - (sp - yMin) / (yMax - yMin) * plot.Height;
        using (var spPen = new Pen(SpColor, 1.4f) { DashStyle = DashStyle.Dash })
            g.DrawLine(spPen, plot.Left, spY, plot.Right, spY);

        // 实时值曲线
        var pts = new PointF[samples.Count];
        for (int i = 0; i < samples.Count; i++)
        {
            float x = plot.Left + (float)i / Math.Max(samples.Count - 1, 1) * plot.Width;
            float y = plot.Bottom - (samples[i].Pv - yMin) / (yMax - yMin) * plot.Height;
            pts[i] = new PointF(x, y);
        }

        using (var pen = new Pen(CurveColor, 2.2f) { LineJoin = LineJoin.Round })
            g.DrawLines(pen, pts);

        // 当前位置圆点
        var last = pts[^1];
        using (var halo = new SolidBrush(Color.FromArgb(46, CurveColor)))
            g.FillEllipse(halo, last.X - 7, last.Y - 7, 14, 14);
        using (var dot = new SolidBrush(CurveColor))
            g.FillEllipse(dot, last.X - 4, last.Y - 4, 8, 8);

        // 阶跃扰动标记：竖线 + 图内左上角文字（带底衬，不压线）
        float stepX = plot.Left + 10;
        using (var stepPen = new Pen(StepColor, 1.4f) { DashStyle = DashStyle.Dash })
            g.DrawLine(stepPen, stepX, plot.Top, stepX, plot.Bottom);

        var tagRect = new Rectangle((int)stepX + 6, plot.Top + 6, 60, 18);
        using (var tagBack = new SolidBrush(Color.FromArgb(235, 255, 255, 255)))
            g.FillRectangle(tagBack, tagRect);
        TextRenderer.DrawText(g, "阶跃扰动", labelFont, tagRect, StepColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }

    private static void DrawInfoPanel(Graphics g, Rectangle info,
        double kp, double ki, double kd, IReadOnlyList<PidStepSample> samples)
    {
        using var titleFont = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold);
        using var labelFont = new Font("Microsoft YaHei UI", 8.5f);
        using var valueFont = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold);

        using (var path = RoundRect(info, 8))
        using (var back = new SolidBrush(InfoBack))
        using (var border = new Pen(InfoBorder, 1f))
        {
            g.FillPath(back, path);
            g.DrawPath(border, path);
        }

        int x = info.Left + 12;
        int right = info.Right - 12;
        int y = info.Top + 10;

        // ---- PID 系数 ----
        TextRenderer.DrawText(g, "PID 系数", titleFont, new Point(x, y), TitleColor);
        y += 26;

        DrawRow(g, labelFont, valueFont, x, right, ref y, "Kp", kp.ToString("0.00"), CurveColor);
        DrawRow(g, labelFont, valueFont, x, right, ref y, "Ki", ki.ToString("0.00"), CurveColor);
        DrawRow(g, labelFont, valueFont, x, right, ref y, "Kd", kd.ToString("0.00"), CurveColor);

        y += 8;
        using (var sep = new Pen(InfoBorder, 1f))
            g.DrawLine(sep, x, y, right, y);
        y += 12;

        // ---- 运行状态 ----
        TextRenderer.DrawText(g, "运行状态", titleFont, new Point(x, y), TitleColor);
        y += 26;

        var cur = samples[^1];
        DrawRow(g, labelFont, valueFont, x, right, ref y, "设定值", cur.Sp.ToString("0.000"), SpColor);
        DrawRow(g, labelFont, valueFont, x, right, ref y, "实时值", cur.Pv.ToString("0.000"), CurveColor);
        DrawRow(g, labelFont, valueFont, x, right, ref y, "误差", cur.Error.ToString("0.000"), TitleColor);
        DrawRow(g, labelFont, valueFont, x, right, ref y, "输出 u", cur.U.ToString("0.000"), TitleColor);

        // ---- 图例 ----
        y += 10;
        using (var line = new Pen(CurveColor, 2f))
            g.DrawLine(line, x, y + 8, x + 16, y + 8);
        TextRenderer.DrawText(g, "实时值", labelFont, new Point(x + 22, y), TitleColor);
        y += 20;
        using (var line = new Pen(SpColor, 1.4f) { DashStyle = DashStyle.Dash })
            g.DrawLine(line, x, y + 8, x + 16, y + 8);
        TextRenderer.DrawText(g, "设定值 SP", labelFont, new Point(x + 22, y), TitleColor);
    }

    private static void DrawRow(Graphics g, Font labelFont, Font valueFont,
        int x, int right, ref int y, string label, string value, Color valueColor)
    {
        TextRenderer.DrawText(g, label, labelFont, new Point(x, y), AxisTextColor);
        TextRenderer.DrawText(g, value, valueFont,
            new Rectangle(x, y - 1, right - x, 18), valueColor,
            TextFormatFlags.Right | TextFormatFlags.Top);
        y += 22;
    }

    private static GraphicsPath RoundRect(Rectangle r, int radius)
    {
        int d = Math.Max(1, radius * 2);
        var path = new GraphicsPath();
        path.AddArc(r.Left, r.Top, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
