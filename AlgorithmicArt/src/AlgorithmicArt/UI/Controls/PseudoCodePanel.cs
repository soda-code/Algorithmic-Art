using System.Drawing.Drawing2D;
using AlgorithmicArt.UI;

namespace AlgorithmicArt.UI.Controls;

/// <summary>伪代码面板：显示算法伪代码并高亮当前执行行。</summary>
public class PseudoCodePanel : Control
{
    private IReadOnlyList<string> _lines = Array.Empty<string>();
    private int _highlight = -1;

    public PseudoCodePanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Theme.CardBack;
        Font = new Font("Consolas", 9.5f);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
    }

    public void SetCode(IReadOnlyList<string> lines)
    {
        _lines = lines;
        _highlight = -1;
        Invalidate();
    }

    public void Highlight(int line)
    {
        if (_highlight == line) return;
        _highlight = line;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);

        var rect = ClientRectangle;
        using var titleFont = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold);
        TextRenderer.DrawText(g, "伪代码", titleFont,
            new Rectangle(rect.X + 10, rect.Y + 8, rect.Width - 20, 24), Theme.TextPrimary,
            TextFormatFlags.VerticalCenter);

        int y = rect.Y + 36;
        const int lineHeight = 22;
        for (int i = 0; i < _lines.Count; i++)
        {
            var lineRect = new Rectangle(rect.X + 6, y, rect.Width - 12, lineHeight);
            if (i == _highlight)
            {
                using var path = Theme.RoundedRect(lineRect, 6);
                using var brush = new SolidBrush(Theme.NavItemHover);
                g.FillPath(brush, path);
            }
            TextRenderer.DrawText(g, $"{i + 1}  {_lines[i]}", Font, lineRect, Theme.TextPrimary,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            y += lineHeight;
        }

        if (_lines.Count == 0)
        {
            TextRenderer.DrawText(g, "（暂无算法伪代码）", Font, rect, Theme.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
