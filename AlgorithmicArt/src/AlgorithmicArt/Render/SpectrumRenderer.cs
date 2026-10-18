using System.Drawing.Drawing2D;
using AlgorithmicArt.Core.Algorithm;

namespace AlgorithmicArt.Render;

/// <summary>频谱渲染器：横轴频率、纵轴幅度，高亮当前 bin 与主频峰值。</summary>
public class SpectrumRenderer : IAnimRenderer
{
    private static readonly Color BarColor = Color.FromArgb(56, 132, 220);
    private static readonly Color PeakColor = Color.FromArgb(245, 130, 40);
    private static readonly Color AxisTextColor = Color.FromArgb(138, 148, 166);
    private static readonly Color GridColor = Color.FromArgb(238, 241, 246);
    private static readonly Color AxisColor = Color.FromArgb(214, 220, 228);

    public void Render(Graphics g, Rectangle bounds, AlgorithmContext ctx, AnimStep? current)
    {
        var mags = current?.Extra as float[];
        if (mags == null || mags.Length == 0) return;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var plot = new Rectangle(
            bounds.X + 58,
            bounds.Y + 22,
            Math.Max(40, bounds.Width - 58 - 24),
            Math.Max(40, bounds.Height - 22 - 42));

        double fs = ctx.Parameters.GetValueOrDefault("Fs", 1000);
        int fftSize = (int)ctx.Parameters.GetValueOrDefault("FftSize", mags.Length * 2);
        double maxFreq = fs / 2.0;

        float maxMag = 0.0001f;
        for (int k = 0; k < mags.Length; k++)
            if (mags[k] > maxMag) maxMag = mags[k];

        using var font = new Font("Microsoft YaHei UI", 7.5f);
        using var gridPen = new Pen(GridColor, 1f) { DashStyle = DashStyle.Dot };
        using var axisPen = new Pen(AxisColor, 1f);

        // 纵坐标：幅度
        for (int k = 0; k <= 4; k++)
        {
            float v = maxMag * k / 4f;
            float y = plot.Bottom - v / maxMag * plot.Height;

            if (k is > 0 and < 4)
                g.DrawLine(gridPen, plot.Left, y, plot.Right, y);

            TextRenderer.DrawText(g, FormatAmp(v), font,
                new Rectangle(plot.Left - 54, (int)Math.Round(y) - 8, 48, 16), AxisTextColor,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        // 横坐标：频率
        for (int k = 0; k <= 4; k++)
        {
            double f = maxFreq * k / 4.0;
            float x = plot.Left + (float)k / 4 * plot.Width;

            if (k is > 0 and < 4)
                g.DrawLine(gridPen, x, plot.Top, x, plot.Bottom);

            TextRenderer.DrawText(g, $"{f:0}Hz", font,
                new Rectangle((int)Math.Round(x) - 26, plot.Bottom + 6, 52, 16), AxisTextColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
        }

        // 峰值 bin
        int peak = 0;
        for (int k = 1; k < mags.Length; k++)
            if (mags[k] > mags[peak]) peak = k;

        // 频谱柱
        float barW = Math.Max(1f, (float)plot.Width / mags.Length);
        var highlight = new HashSet<int>(current?.Indices ?? Array.Empty<int>());

        using var barBrush = new SolidBrush(BarColor);
        using var peakBrush = new SolidBrush(PeakColor);

        for (int k = 0; k < mags.Length; k++)
        {
            float h = Math.Max(0.5f, mags[k] / maxMag * plot.Height);
            var rect = new RectangleF(
                plot.Left + k * barW,
                plot.Bottom - h,
                Math.Max(1f, barW - 0.6f),
                h);

            bool isHighlight = highlight.Contains(k);
            g.FillRectangle(isHighlight || k == peak ? peakBrush : barBrush, rect);
        }

        // 坐标轴
        g.DrawLine(axisPen, plot.Left, plot.Top, plot.Left, plot.Bottom);
        g.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);

        // 主频标注
        float peakX = plot.Left + (peak + 0.5f) * barW;
        double peakFreq = peak * fs / Math.Max(fftSize, 1);

        using (var peakPen = new Pen(PeakColor, 1.2f) { DashStyle = DashStyle.Dash })
            g.DrawLine(peakPen, peakX, plot.Top, peakX, plot.Bottom);

        TextRenderer.DrawText(g, $"主频 {peakFreq:0.0} Hz", font,
            new Rectangle((int)peakX + 5, plot.Top + 4, 100, 16), PeakColor,
            TextFormatFlags.Left | TextFormatFlags.Top);

        // 轴标题
        TextRenderer.DrawText(g, "幅度", font,
            new Rectangle(plot.Left - 54, plot.Top - 20, 48, 16), AxisTextColor,
            TextFormatFlags.Right | TextFormatFlags.Top);
        TextRenderer.DrawText(g, "频率", font,
            new Rectangle(plot.Right - 40, plot.Bottom + 24, 40, 16), AxisTextColor,
            TextFormatFlags.Right | TextFormatFlags.Top);
    }

    private static string FormatAmp(float v) => v >= 1 ? v.ToString("0.0") : v.ToString("0.000");
}
