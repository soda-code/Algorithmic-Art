using System.Drawing.Drawing2D;

namespace AlgorithmicArt.UI;

/// <summary>全局主题：配色、字体、圆角辅助。</summary>
public static class Theme
{
    // 背景与卡片
    public static readonly Color WindowBack = Color.FromArgb(238, 241, 246);
    public static readonly Color CardBack = Color.White;
    public static readonly Color Border = Color.FromArgb(222, 226, 232);

    // 强调色（导航选中 / 主按钮）
    public static readonly Color Accent = Color.FromArgb(24, 128, 220);
    public static readonly Color AccentHover = Color.FromArgb(18, 105, 185);
    public static readonly Color AccentPressed = Color.FromArgb(14, 88, 158);

    // 次要按钮
    public static readonly Color SecondaryBack = Color.FromArgb(245, 247, 250);
    public static readonly Color SecondaryHover = Color.FromArgb(232, 240, 248);
    public static readonly Color SecondaryPressed = Color.FromArgb(214, 222, 230);
    public static readonly Color SecondaryBorder = Color.FromArgb(198, 205, 214);

    // 导航项
    public static readonly Color NavItemSelected = Color.FromArgb(24, 128, 220);
    public static readonly Color NavItemHover = Color.FromArgb(232, 240, 248);

    // 文字
    public static readonly Color TextPrimary = Color.FromArgb(38, 42, 50);
    public static readonly Color TextSecondary = Color.FromArgb(120, 128, 140);
    public static readonly Color StatusBarBack = Color.FromArgb(250, 251, 252);

    public static readonly Font BaseFont = new("Microsoft YaHei UI", 9f);
    public static readonly Font NavFont = new("Microsoft YaHei UI", 10f);

    /// <summary>创建圆角矩形路径（复用）。</summary>
    public static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        var path = new GraphicsPath();
        if (radius <= 0 || r.Width <= 0 || r.Height <= 0)
        {
            path.AddRectangle(r);
            return path;
        }
        int d = radius * 2;
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
