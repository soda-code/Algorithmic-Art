using System.Drawing.Drawing2D;
using AlgorithmicArt.UI;

namespace AlgorithmicArt.UI.Controls;

/// <summary>圆角面板：自绘圆角背景 + 可选边框，子控件通过 Padding 避开圆角。</summary>
public class RoundedPanel : Panel
{
    private int _cornerRadius = 12;

    public int CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = Math.Max(0, value); UpdateRegion(); Invalidate(); }
    }

    public Color FillColor { get; set; } = Theme.CardBack;
    public Color BorderColor { get; set; } = Color.Transparent;
    public int BorderWidth { get; set; } = 0;

    public RoundedPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent;
        Padding = new Padding(8);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateRegion();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = Theme.RoundedRect(new Rectangle(0, 0, Width, Height), CornerRadius);
        using var brush = new SolidBrush(FillColor);
        e.Graphics.FillPath(brush, path);
        if (BorderWidth > 0 && BorderColor.A > 0)
        {
            using var pen = new Pen(BorderColor, BorderWidth);
            e.Graphics.DrawPath(pen, path);
        }
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // 由 OnPaint 统一绘制，避免闪烁
    }

    private void UpdateRegion()
    {
        if (Width <= 0 || Height <= 0) return;
        using var path = Theme.RoundedRect(new Rectangle(0, 0, Width, Height), CornerRadius);
        Region = new Region(path);
    }
}
