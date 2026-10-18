using System.Drawing.Drawing2D;
using AlgorithmicArt.Core.Algorithm;

namespace AlgorithmicArt.Render;

/// <summary>柱状图渲染器（排序/查找类）：插值动画 + 已排序着色 + 高亮 + 纵坐标刻度。</summary>
public class ChartRenderer : IAnimRenderer
{
    private static readonly Color BarColor = Color.FromArgb(70, 140, 210);
    private static readonly Color SortedColor = Color.FromArgb(52, 168, 118);
    private static readonly Color HighlightColor = Color.FromArgb(245, 130, 40);
    private static readonly Color LabelColor = Color.FromArgb(96, 104, 116);
    private static readonly Color AxisTextColor = Color.FromArgb(138, 148, 166);
    private static readonly Color GridColor = Color.FromArgb(238, 241, 246);
    private static readonly Color AxisColor = Color.FromArgb(214, 220, 228);

    private int[]? _from;
    private int[]? _to;
    private int _animStart;

    public void Render(Graphics g, Rectangle bounds, AlgorithmContext ctx, AnimStep? current)
    {
        var target = current?.Extra as int[] ?? ctx.Array;
        if (target == null || target.Length == 0) return;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        // 数组变化时，从上一状态平滑插值到新状态
        if (_to == null || _to.Length != target.Length || !_to.SequenceEqual(target))
        {
            _from = _to ?? (int[])target.Clone();
            _to = (int[])target.Clone();
            _animStart = Environment.TickCount;
        }

        float t = Math.Clamp((Environment.TickCount - _animStart) / 140f, 0f, 1f);
        float ease = 1 - (float)Math.Pow(1 - t, 3);

        // 左侧预留纵坐标刻度区，底部留出透视空间
        var plot = new Rectangle(
            bounds.X + 56,
            bounds.Y + 16,
            Math.Max(1, bounds.Width - 56 - 22),
            Math.Max(1, bounds.Height - 16 - 34));
        if (plot.Width <= 0 || plot.Height <= 0) return;

        int n = target.Length;
        int rawMax = Math.Max(1, Math.Max(target.Max(), _from?.Max() ?? 0));
        int max = NiceCeil(rawMax);

        float barWidth = (float)plot.Width / n;
        var highlight = new HashSet<int>(current?.Indices ?? Array.Empty<int>());
        int sortedLow = current?.SortedLow ?? -1;
        int sortedHigh = current?.SortedHigh ?? -1;

        using var labelFont = new Font("Microsoft YaHei UI", 7f);
        using var axisFont = new Font("Microsoft YaHei UI", 7.5f);
        using var gridPen = new Pen(GridColor, 1f) { DashStyle = DashStyle.Dot };
        using var axisPen = new Pen(AxisColor, 1f);

        // 纵坐标刻度 + 水平网格
        const int ticks = 4;
        for (int k = 0; k <= ticks; k++)
        {
            int v = (int)Math.Round(max * k / (double)ticks);
            float y = plot.Bottom - (float)v / max * plot.Height;

            if (k is > 0 and < ticks)
                g.DrawLine(gridPen, plot.Left, y, plot.Right, y);

            TextRenderer.DrawText(g, v.ToString(), axisFont,
                new Rectangle(bounds.X, (int)Math.Round(y) - 8, 50, 16), AxisTextColor,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        using var barBrush = new SolidBrush(BarColor);
        using var sortedBrush = new SolidBrush(SortedColor);
        using var highlightBrush = new SolidBrush(HighlightColor);
        using var highlightPen = new Pen(Color.FromArgb(200, 80, 20), 1.5f);

        if (barWidth >= 1.2f)
        {
            // 常规数据量：逐柱绘制
            for (int i = 0; i < n; i++)
            {
                float fromVal = _from != null && i < _from.Length ? _from[i] : target[i];
                float val = fromVal + (target[i] - fromVal) * ease;

                float h = Math.Max(1f, val / max * (plot.Height - 10));
                var bar = new RectangleF(
                    plot.X + i * barWidth,
                    plot.Bottom - h,
                    Math.Max(1f, barWidth - 1f),
                    h);

                bool isHighlight = highlight.Contains(i);
                bool isSorted = sortedLow >= 0 && i >= sortedLow && i <= sortedHigh;

                g.FillRectangle(isHighlight ? highlightBrush : isSorted ? sortedBrush : barBrush, bar);

                if (isHighlight)
                    g.DrawRectangle(highlightPen, bar.X, bar.Y, bar.Width, bar.Height);

                if (barWidth >= 22 && n <= 40)
                {
                    TextRenderer.DrawText(g, ((int)Math.Round(val)).ToString(), labelFont,
                        new Rectangle((int)bar.X, (int)(bar.Y - 15), (int)bar.Width, 13), LabelColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.Bottom);
                }
            }
        }
        else
        {
            // 数据量超过像素宽度：按像素列聚合，绘制开销固定为画布宽度量级
            for (int px = 0; px < plot.Width; px++)
            {
                int from = (int)((long)px * n / plot.Width);
                int to = (int)((long)(px + 1) * n / plot.Width);
                if (to <= from) to = from + 1;

                float peak = 0f;
                bool anyHighlight = false;

                for (int i = from; i < to && i < n; i++)
                {
                    float fromVal = _from != null && i < _from.Length ? _from[i] : target[i];
                    float val = fromVal + (target[i] - fromVal) * ease;
                    if (val > peak) peak = val;
                    if (highlight.Contains(i)) anyHighlight = true;
                }

                float h = Math.Max(1f, peak / max * (plot.Height - 10));
                g.FillRectangle(anyHighlight ? highlightBrush : barBrush,
                    new RectangleF(plot.X + px, plot.Bottom - h, 1f, h));
            }
        }

        // 坐标轴
        g.DrawLine(axisPen, plot.Left, plot.Top, plot.Left, plot.Bottom);
        g.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);
    }

    /// <summary>把最大值向上取整到「整齐」的刻度上限（如 997 → 1000、42 → 50）。</summary>
    private static int NiceCeil(int v)
    {
        if (v <= 10) return 10;
        int mag = (int)Math.Pow(10, Math.Floor(Math.Log10(v)));
        int step = mag / 2 > 0 ? mag / 2 : 1;
        return (int)(Math.Ceiling(v / (double)step) * step);
    }
}
