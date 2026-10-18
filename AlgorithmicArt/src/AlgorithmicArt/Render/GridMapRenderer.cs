using AlgorithmicArt.Core.Algorithm;
using AlgorithmicArt.Models;

namespace AlgorithmicArt.Render;

/// <summary>栅格地图渲染器（扫地/寻路类）：左侧标注行号（纵坐标）。</summary>
public class GridMapRenderer : IAnimRenderer
{
    private static readonly Color AxisTextColor = Color.FromArgb(138, 148, 166);

    public void Render(Graphics g, Rectangle bounds, AlgorithmContext ctx, AnimStep? current)
    {
        var map = ctx.Map;
        if (map == null) return;

        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

        const int axisLeft = 34; // 左侧行号区宽度
        const int margin = 10;

        int availW = Math.Max(1, bounds.Width - axisLeft - margin * 2);
        int availH = Math.Max(1, bounds.Height - margin * 2);

        int cell = Math.Max(2, Math.Min(
            availW / Math.Max(map.Width, 1),
            availH / Math.Max(map.Height, 1)));
        int offsetX = bounds.X + axisLeft + margin + (availW - cell * map.Width) / 2;
        int offsetY = bounds.Y + margin + (availH - cell * map.Height) / 2;

        var highlight = new HashSet<int>(current?.Indices ?? Array.Empty<int>());
        var scanned = current?.Extra as int[];
        var scannedSet = scanned != null ? new HashSet<int>(scanned) : null;

        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                int idx = map.Index(x, y);
                var state = map.Get(x, y);
                var rect = new Rectangle(offsetX + x * cell, offsetY + y * cell, cell - 1, cell - 1);

                Color color;
                if (state == CellState.Obstacle) color = Color.FromArgb(70, 74, 82);
                else if (state == CellState.Start) color = Color.FromArgb(46, 160, 67);
                else if (highlight.Contains(idx)) color = Color.FromArgb(240, 160, 60);
                else if (scannedSet?.Contains(idx) == true) color = Color.FromArgb(150, 200, 240);
                else color = Color.FromArgb(246, 248, 250);

                using var brush = new SolidBrush(color);
                g.FillRectangle(brush, rect);
                if (highlight.Contains(idx))
                {
                    using var pen = new Pen(Color.FromArgb(220, 110, 20), 1.5f);
                    g.DrawRectangle(pen, rect);
                }
            }
        }

        // 纵坐标（行号）刻度
        using var axisFont = new Font("Microsoft YaHei UI", 7.5f);
        int step = Math.Max(1, (int)Math.Ceiling(map.Height / 10.0));
        int textX = offsetX - 8;
        for (int y = 0; y < map.Height; y += step)
        {
            int cy = offsetY + y * cell + cell / 2;
            TextRenderer.DrawText(g, y.ToString(), axisFont,
                new Rectangle(bounds.X, cy - 8, Math.Max(20, textX - bounds.X), 16), AxisTextColor,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        // 左侧纵轴
        using var axisPen = new Pen(Color.FromArgb(214, 220, 228), 1f);
        g.DrawLine(axisPen, offsetX - 3, offsetY, offsetX - 3, offsetY + cell * map.Height);
    }
}
