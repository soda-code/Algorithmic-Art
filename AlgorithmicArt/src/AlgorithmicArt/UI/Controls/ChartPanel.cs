using System.Drawing.Drawing2D;

namespace AlgorithmicArt.UI.Controls;

/// <summary>实时曲线面板（现代风格）：Y 轴数值、X 轴时间，多通道 + 图例。</summary>
public class ChartPanel : Control
{
    private static readonly Color GridColor = Color.FromArgb(238, 241, 246);
    private static readonly Color AxisTextColor = Color.FromArgb(138, 148, 166);
    private static readonly Color[] ChannelColors =
    {
        Color.FromArgb(46, 125, 224),
        Color.FromArgb(240, 98, 146),
        Color.FromArgb(38, 198, 218),
        Color.FromArgb(171, 71, 188)
    };

    private readonly List<double[]> _series = new();
    private const int MaxPoints = 500;

    /// <summary>采样率（Hz），用于把采样序号换算为时间。</summary>
    public double SampleRate { get; set; } = 1000;

    public ChartPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
    }

    public void AddSample(double[] values)
    {
        _series.Add(values);
        if (_series.Count > MaxPoints)
            _series.RemoveAt(0);
        Invalidate();
    }

    public void Clear()
    {
        _series.Clear();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);

        var plot = new Rectangle(58, 30, Width - 76, Height - 68);
        if (plot.Width <= 0 || plot.Height <= 0) return;

        using var gridPen = new Pen(GridColor, 1f) { DashStyle = DashStyle.Dot };
        using var axisPen = new Pen(Color.FromArgb(214, 220, 228), 1f);
        using var labelFont = new Font("Microsoft YaHei UI", 7.5f);
        using var legendFont = new Font("Microsoft YaHei UI", 8f);

        if (_series.Count == 0)
        {
            g.DrawLine(axisPen, plot.Left, plot.Top, plot.Left, plot.Bottom);
            g.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);
            TextRenderer.DrawText(g, "实时曲线（等待采样数据）", Font, ClientRectangle, AxisTextColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            return;
        }

        double min = _series.SelectMany(s => s).DefaultIfEmpty(0).Min();
        double max = _series.SelectMany(s => s).DefaultIfEmpty(1).Max();
        if (max - min < 1e-9) max = min + 1;
        double pad = (max - min) * 0.12;
        min -= pad;
        max += pad;

        double totalTime = (_series.Count - 1) / Math.Max(SampleRate, 1e-9);

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

        g.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);

        // 多通道曲线
        int channels = _series.Max(s => s.Length);
        for (int c = 0; c < channels; c++)
        {
            var color = ChannelColors[c % ChannelColors.Length];
            using var pen = new Pen(color, 1.8f) { LineJoin = LineJoin.Round };

            PointF? prev = null;
            for (int i = 0; i < _series.Count; i++)
            {
                if (c >= _series[i].Length) { prev = null; continue; }
                float x = plot.Left + (float)i / Math.Max(_series.Count - 1, 1) * plot.Width;
                float y = plot.Bottom - (float)((_series[i][c] - min) / (max - min)) * plot.Height;
                var p = new PointF(x, y);
                if (prev.HasValue) g.DrawLine(pen, prev.Value, p);
                prev = p;
            }
        }

        DrawLegend(g, legendFont, plot, channels);
    }

    private static void DrawLegend(Graphics g, Font font, Rectangle plot, int channels)
    {
        int y = Math.Max(2, plot.Top - 18);
        int x = Math.Max(4, plot.Right - channels * 72);

        for (int c = 0; c < channels; c++)
        {
            using var brush = new SolidBrush(ChannelColors[c % ChannelColors.Length]);
            g.FillEllipse(brush, x, y + 4, 8, 8);
            TextRenderer.DrawText(g, $"通道{c + 1}", font, new Point(x + 12, y), AxisTextColor);
            x += 72;
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
