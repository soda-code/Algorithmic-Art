using System.Drawing.Drawing2D;
using AlgorithmicArt.UI;

namespace AlgorithmicArt.UI.Controls;

/// <summary>圆角按钮：自绘背景，支持悬停/按下状态。</summary>
public class RoundedButton : Button
{
    public int CornerRadius { get; set; } = 8;
    public Color FillColor { get; set; } = Theme.Accent;
    public Color HoverColor { get; set; } = Theme.AccentHover;
    public Color PressedColor { get; set; } = Theme.AccentPressed;
    public Color TextColor { get; set; } = Color.White;
    /// <summary>悬停时的文字颜色（为空则沿用 TextColor）。</summary>
    public Color? HoverTextColor { get; set; }
    public Color BorderColor { get; set; } = Color.Transparent;
    public int BorderWidth { get; set; } = 0;

    private bool _hover;
    private bool _pressed;

    public RoundedButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Cursor = Cursors.Hand;
        Height = 32;
    }

    /// <summary>主按钮（蓝底白字）。</summary>
    public static RoundedButton Primary(string text, EventHandler onClick, int width = 90) =>
        new RoundedButton
        {
            Text = text,
            Width = width,
            FillColor = Theme.Accent,
            HoverColor = Theme.AccentHover,
            PressedColor = Theme.AccentPressed,
            TextColor = Color.White
        }.WithClick(onClick);

    /// <summary>次要按钮（浅底深字带边框）。</summary>
    public static RoundedButton Secondary(string text, EventHandler onClick, int width = 80) =>
        new RoundedButton
        {
            Text = text,
            Width = width,
            FillColor = Theme.SecondaryBack,
            HoverColor = Theme.SecondaryHover,
            PressedColor = Theme.SecondaryPressed,
            TextColor = Theme.TextPrimary,
            BorderColor = Theme.SecondaryBorder,
            BorderWidth = 1
        }.WithClick(onClick);

    private RoundedButton WithClick(EventHandler onClick)
    {
        Click += onClick;
        return this;
    }

    /// <summary>应用主按钮配色（蓝底白字）。</summary>
    public RoundedButton ApplyPrimary()
    {
        FillColor = Theme.Accent;
        HoverColor = Theme.AccentHover;
        PressedColor = Theme.AccentPressed;
        TextColor = Color.White;
        BorderColor = Theme.AccentPressed;
        BorderWidth = 1;
        return this;
    }

    /// <summary>应用次要按钮配色（浅底深字带边框）。</summary>
    public RoundedButton ApplySecondary()
    {
        FillColor = Theme.SecondaryBack;
        HoverColor = Theme.SecondaryHover;
        PressedColor = Theme.SecondaryPressed;
        TextColor = Theme.TextPrimary;
        BorderColor = Theme.SecondaryBorder;
        BorderWidth = 1;
        return this;
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        // 先用父背景色铺底，消除圆角外残留黑边
        var surface = Parent?.BackColor ?? BackColor;
        using (var bg = new SolidBrush(surface))
            e.Graphics.FillRectangle(bg, ClientRectangle);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Color fill = Enabled
            ? (_pressed ? PressedColor : _hover ? HoverColor : FillColor)
            : Color.FromArgb(200, 204, 210);

        using var path = Theme.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius);
        using var brush = new SolidBrush(fill);
        e.Graphics.FillPath(brush, path);
        if (BorderWidth > 0 && BorderColor.A > 0)
        {
            Color border = _pressed
                ? ControlPaint.Dark(BorderColor, 0.15f)
                : _hover
                    ? ControlPaint.Dark(BorderColor, 0.06f)
                    : BorderColor;
            using var pen = new Pen(border, BorderWidth);
            e.Graphics.DrawPath(pen, path);
        }
        Color textColor = !Enabled
            ? Color.White
            : (_hover || _pressed) && HoverTextColor.HasValue
                ? HoverTextColor.Value
                : TextColor;

        TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, textColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // 由 OnPaint 统一绘制
    }
}
