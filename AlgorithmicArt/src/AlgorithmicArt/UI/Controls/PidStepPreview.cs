using System.Drawing.Drawing2D;
using AlgorithmicArt.Core.Algorithm;

namespace AlgorithmicArt.UI.Controls;

/// <summary>PID 阶跃响应预览：经典曲线（目标值 SP + 实时值 PV）。</summary>
public class PidStepPreview : Control
{
    private static readonly Color AxisColor = Color.FromArgb(70, 76, 86);
    private static readonly Color AxisTextColor = Color.FromArgb(120, 128, 140);
    private static readonly Color SpColor = Color.FromArgb(150, 158, 170);
    private static readonly Color CurveColor = Color.FromArgb(67, 160, 71);
    private static readonly Color StepColor = Color.FromArgb(245, 160, 50);

    private IReadOnlyList<PidStepSample> _samples = Array.Empty<PidStepSample>();

    public PidStepPreview()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
    }

    public void SetSamples(IReadOnlyList<PidStepSample> samples)
    {
        _samples = samples;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(BackColor);
        if (_samples.Count < 2) return;

        g.SmoothingMode = SmoothingMode.AntiAlias;

        var plot = new Rectangle(62, 42, Math.Max(40, Width - 86), Math.Max(40, Height - 88));
        RenderCurve(g, plot);
    }

    private void RenderCurve(Graphics g, Rectangle plot)
    {
        using var axisPen = new Pen(AxisColor, 1.2f);
        using var labelFont = new Font("Microsoft YaHei UI", 8.5f);

        float sp = _samples[0].Sp;
        float pvMax = _samples.Max(s => s.Pv);
        float yMax = Math.Max(sp, pvMax) * 1.25f;
        const float yMin = 0f;

        g.DrawLine(axisPen, plot.Left, plot.Top, plot.Left, plot.Bottom);
        g.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);

        TextRenderer.DrawText(g, "PV", labelFont,
            new Rectangle(plot.Left - 46, plot.Top - 2, 42, 16), AxisTextColor,
            TextFormatFlags.Right | TextFormatFlags.Top);
        TextRenderer.DrawText(g, "时间 t", labelFont,
            new Rectangle(plot.Left, plot.Bottom + 8, plot.Width, 16), AxisTextColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);

        float stepX = plot.Left + 10;
        using (var stepPen = new Pen(StepColor, 1.4f) { DashStyle = DashStyle.Dash })
            g.DrawLine(stepPen, stepX, plot.Top, stepX, plot.Bottom);
        TextRenderer.DrawText(g, "阶跃扰动", labelFont,
            new Rectangle((int)stepX + 6, plot.Top - 2, 84, 16), StepColor,
            TextFormatFlags.Left | TextFormatFlags.Top);

        float spY = plot.Bottom - (sp - yMin) / (yMax - yMin) * plot.Height;
        using (var spPen = new Pen(SpColor, 1.4f) { DashStyle = DashStyle.Dash })
            g.DrawLine(spPen, plot.Left, spY, plot.Right, spY);
        TextRenderer.DrawText(g, "SP", labelFont,
            new Rectangle(plot.Right + 4, (int)spY - 8, 30, 16), AxisTextColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

        var pts = new PointF[_samples.Count];
        for (int i = 0; i < _samples.Count; i++)
        {
            float x = plot.Left + (float)i / Math.Max(_samples.Count - 1, 1) * plot.Width;
            float y = plot.Bottom - (_samples[i].Pv - yMin) / (yMax - yMin) * plot.Height;
            pts[i] = new PointF(x, y);
        }

        using (var pen = new Pen(CurveColor, 2.2f) { LineJoin = LineJoin.Round })
            g.DrawLines(pen, pts);

        int lx = Math.Max(plot.Left + 24, plot.Right - 96);
        int ly = plot.Top + 18;
        using (var b = new SolidBrush(CurveColor)) g.FillRectangle(b, lx, ly + 7, 16, 3);
        TextRenderer.DrawText(g, "实时值", labelFont, new Point(lx + 22, ly), CurveColor);
    }
}
