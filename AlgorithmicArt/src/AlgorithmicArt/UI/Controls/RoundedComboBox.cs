using AlgorithmicArt.UI;

namespace AlgorithmicArt.UI.Controls;

/// <summary>圆角下拉框：通过 Region 裁剪实现圆角。</summary>
public class RoundedComboBox : ComboBox
{
    private int _cornerRadius = 6;

    public int CornerRadius
    {
        get => _cornerRadius;
        set
        {
            _cornerRadius = Math.Max(0, value);
            UpdateRegion();
        }
    }

    public RoundedComboBox()
    {
        FlatStyle = FlatStyle.Flat;
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateRegion();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        UpdateRegion();
    }

    private void UpdateRegion()
    {
        if (Width <= 0 || Height <= 0) return;
        using var path = Theme.RoundedRect(new Rectangle(0, 0, Width, Height), CornerRadius);
        var region = new Region(path);
        var old = Region;
        Region = region;
        old?.Dispose();
    }
}
