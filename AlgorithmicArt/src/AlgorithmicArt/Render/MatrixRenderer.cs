using System.Drawing.Drawing2D;
using AlgorithmicArt.Core.Algorithm;

namespace AlgorithmicArt.Render;

/// <summary>矩阵表格渲染器：并排显示多个矩阵块，支持行列高亮与增广分隔线。</summary>
public class MatrixRenderer : IAnimRenderer
{
    private static readonly Color CellBack = Color.White;
    private static readonly Color CellBorder = Color.FromArgb(220, 226, 234);
    private static readonly Color HighlightBack = Color.FromArgb(226, 238, 252);
    private static readonly Color AccentBack = Color.FromArgb(252, 206, 158);
    private static readonly Color TextColor = Color.FromArgb(38, 42, 50);
    private static readonly Color TitleColor = Color.FromArgb(90, 98, 112);
    private static readonly Color SplitColor = Color.FromArgb(110, 118, 132);

    public void Render(Graphics g, Rectangle bounds, AlgorithmContext ctx, AnimStep? current)
    {
        var frame = current?.Extra as MatrixFrame;
        if (frame == null || frame.Blocks.Length == 0) return;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        const int gap = 26;
        const int titleH = 26;
        int cellW = 54;
        int cellH = 34;

        int TotalWidth(int cw) =>
            frame.Blocks.Sum(b => b.Cols * cw) + gap * (frame.Blocks.Length - 1);

        int avail = bounds.Width - 48;
        while (TotalWidth(cellW) > avail && cellW > 26) cellW -= 2;

        int totalW = TotalWidth(cellW);
        int maxRows = frame.Blocks.Max(b => b.Rows);
        int totalH = titleH + maxRows * cellH;

        int startX = bounds.X + Math.Max(16, (bounds.Width - totalW) / 2);
        int startY = bounds.Y + Math.Max(16, (bounds.Height - totalH) / 2);

        using var titleFont = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold);
        using var cellFont = new Font("Consolas", 9.5f);
        using var borderPen = new Pen(CellBorder, 1f);
        using var cellBrush = new SolidBrush(CellBack);
        using var highlightBrush = new SolidBrush(HighlightBack);
        using var accentBrush = new SolidBrush(AccentBack);
        using var splitPen = new Pen(SplitColor, 1.6f);

        int x = startX;
        foreach (var block in frame.Blocks)
        {
            int blockW = block.Cols * cellW;

            TextRenderer.DrawText(g, block.Title, titleFont,
                new Rectangle(x, startY, Math.Max(blockW, 80), titleH - 6), TitleColor,
                TextFormatFlags.Left | TextFormatFlags.Bottom);

            for (int r = 0; r < block.Rows; r++)
            {
                for (int c = 0; c < block.Cols; c++)
                {
                    var rect = new Rectangle(x + c * cellW, startY + titleH + r * cellH, cellW, cellH);

                    bool accent = r == block.AccentRow && c == block.AccentCol;
                    bool highlighted = block.RowHighlights.Contains(r) || block.ColHighlights.Contains(c);

                    g.FillRectangle(accent ? accentBrush : highlighted ? highlightBrush : cellBrush, rect);
                    g.DrawRectangle(borderPen, rect);

                    TextRenderer.DrawText(g, Format(block.Values[r, c]), cellFont, rect, TextColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }

            // 增广矩阵分隔线 [A | b]
            if (block.SplitAfterColumn >= 0 && block.SplitAfterColumn < block.Cols - 1)
            {
                int sx = x + (block.SplitAfterColumn + 1) * cellW;
                g.DrawLine(splitPen, sx, startY + titleH, sx, startY + titleH + block.Rows * cellH);
            }

            x += blockW + gap;
        }
    }

    private static string Format(double v)
    {
        if (Math.Abs(v - Math.Round(v)) < 1e-9)
            return ((long)Math.Round(v)).ToString();
        return v.ToString("0.00");
    }
}
