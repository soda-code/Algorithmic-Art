using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using AlgorithmicArt.UI;

namespace AlgorithmicArt.UI.Controls;

/// <summary>自绘圆角下拉框：圆角背景 + 边框 + 箭头 + 下拉列表。</summary>
public class RoundedDropDown : Control
{
    private readonly List<object> _items = new();
    private int _selectedIndex = -1;
    private bool _hover;
    private string _text = "";
    private ToolStripDropDown? _dropDown;

    public int CornerRadius { get; set; } = 6;
    public Color FillColor { get; set; } = Color.White;
    public Color HoverColor { get; set; } = Theme.SecondaryHover;
    public Color BorderColor { get; set; } = Theme.SecondaryBorder;
    public Color TextColor { get; set; } = Theme.TextPrimary;

    public event EventHandler? SelectedIndexChanged;

    public List<object> Items => _items;

    /// <summary>自定义显示文本（默认为 ToString）。</summary>
    public Func<object, string>? DisplaySelector { get; set; }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (value < -1 || value >= _items.Count) value = -1;
            if (_selectedIndex == value) return;
            _selectedIndex = value;
            Invalidate();
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public object? SelectedItem =>
        _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;

    [AllowNull]
    public override string Text
    {
        get => _items != null && _selectedIndex >= 0 ? GetDisplay(SelectedItem!) : (_text ?? "");
        set
        {
            _text = value ?? "";
            if (_items == null) return; // 基类构造期间字段尚未初始化
            int idx = _items.FindIndex(i => GetDisplay(i) == _text);
            if (idx >= 0 && idx != _selectedIndex)
            {
                _selectedIndex = idx;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
            Invalidate();
        }
    }

    public RoundedDropDown()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Cursor = Cursors.Hand;
        Height = 26;
        Font = Theme.BaseFont;
    }

    public void AddRange(IEnumerable<object> items)
    {
        _items.AddRange(items);
        if (_selectedIndex < 0 && _items.Count > 0) _selectedIndex = 0;
        Invalidate();
    }

    public void ClearItems()
    {
        _items.Clear();
        _selectedIndex = -1;
        _text = "";
        Invalidate();
    }

    private string GetDisplay(object item) => DisplaySelector?.Invoke(item) ?? item.ToString() ?? "";

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var surface = Parent?.BackColor ?? BackColor;
        using (var bg = new SolidBrush(surface))
            g.FillRectangle(bg, ClientRectangle);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = Theme.RoundedRect(rect, CornerRadius);
        using (var brush = new SolidBrush(_hover ? HoverColor : FillColor))
            g.FillPath(brush, path);
        using (var pen = new Pen(BorderColor, 1))
            g.DrawPath(pen, path);

        var textRect = new Rectangle(8, 0, Math.Max(0, Width - 28), Height);
        TextRenderer.DrawText(g, Text, Font, textRect, TextColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

        int cx = Width - 14, cy = Height / 2;
        using var arrowPen = new Pen(Theme.TextSecondary, 1.6f);
        g.DrawLine(arrowPen, cx - 4, cy - 2, cx, cy + 2);
        g.DrawLine(arrowPen, cx, cy + 2, cx + 4, cy - 2);
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left) ShowDropDown();
    }

    private void ShowDropDown()
    {
        if (_items.Count == 0) return;

        var list = new ListBox
        {
            BorderStyle = BorderStyle.None,
            BackColor = Color.White,
            ForeColor = Theme.TextPrimary,
            Font = Font,
            DrawMode = DrawMode.OwnerDrawFixed,
            ItemHeight = 24,
            IntegralHeight = false,
            Width = Math.Max(Width, 120),
            Height = Math.Min(_items.Count * 24 + 4, 220)
        };

        foreach (var item in _items) list.Items.Add(item);
        list.SelectedIndex = _selectedIndex;

        list.DrawItem += (_, de) =>
        {
            if (de.Index < 0) return;
            bool selected = (de.State & DrawItemState.Selected) == DrawItemState.Selected;
            using var bg = new SolidBrush(selected ? Theme.NavItemHover : Color.White);
            de.Graphics.FillRectangle(bg, de.Bounds);
            TextRenderer.DrawText(de.Graphics, GetDisplay(list.Items[de.Index]), Font, de.Bounds,
                Theme.TextPrimary, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        };

        list.MouseClick += (_, me) =>
        {
            int idx = list.IndexFromPoint(me.Location);
            if (idx >= 0)
            {
                SelectedIndex = idx;
                _dropDown?.Close();
            }
        };

        var host = new ToolStripControlHost(list)
        {
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            AutoSize = false,
            Size = list.Size
        };

        _dropDown = new ToolStripDropDown { Padding = Padding.Empty, AutoClose = true };
        _dropDown.Items.Add(host);
        _dropDown.Show(this, 0, Height);
    }
}
