using System.Drawing.Drawing2D;
using AlgorithmicArt.UI;

namespace AlgorithmicArt.UI.Controls;

/// <summary>窗口按钮图标类型。</summary>
public enum WindowButtonKind
{
    Minimize,
    Maximize,
    /// <summary>还原（缩小）窗口：两个重叠方框。</summary>
    Restore,
    Close
}

/// <summary>标题栏窗口按钮：自绘标准图标，支持悬停与禁用态。</summary>
public class WindowButton : Control
{
    private bool _hover;
    private bool _pressed;

    public WindowButtonKind Kind { get; set; } = WindowButtonKind.Minimize;

    public int CornerRadius { get; set; } = 6;
    public Color FillColor { get; set; } = Color.Transparent;
    public Color HoverColor { get; set; } = Theme.SecondaryHover;
    public Color PressedColor { get; set; } = Theme.SecondaryPressed;
    public Color IconColor { get; set; } = Theme.TextSecondary;
    public Color HoverIconColor { get; set; } = Theme.TextPrimary;

    public WindowButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Cursor = Cursors.Hand;
        Size = new Size(42, 30);
        TabStop = false;
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        using (var bg = new SolidBrush(Parent?.BackColor ?? Color.White))
            g.FillRectangle(bg, ClientRectangle);

        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (var path = Theme.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius))
        using (var brush = new SolidBrush(_pressed ? PressedColor : _hover ? HoverColor : FillColor))
        {
            g.FillPath(brush, path);
        }

        float cx = Width / 2f;
        float cy = Height / 2f;

        using var pen = new Pen(_hover ? HoverIconColor : IconColor, 1.3f);

        switch (Kind)
        {
            case WindowButtonKind.Minimize:
                g.DrawLine(pen, cx - 5, cy, cx + 5, cy);
                break;

            case WindowButtonKind.Maximize:
                g.DrawRectangle(pen, cx - 5, cy - 5, 10, 10);
                break;

            case WindowButtonKind.Restore:
                // 后框：只画上边与右边
                g.DrawLine(pen, cx + 1, cy - 5, cx + 5, cy - 5);
                g.DrawLine(pen, cx + 5, cy - 5, cx + 5, cy - 1);
                // 前框
                g.DrawRectangle(pen, cx - 5, cy - 2, 8, 8);
                break;

            case WindowButtonKind.Close:
                g.DrawLine(pen, cx - 5, cy - 5, cx + 5, cy + 5);
                g.DrawLine(pen, cx - 5, cy + 5, cx + 5, cy - 5);
                break;
        }
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // 由 OnPaint 统一绘制
    }
}
