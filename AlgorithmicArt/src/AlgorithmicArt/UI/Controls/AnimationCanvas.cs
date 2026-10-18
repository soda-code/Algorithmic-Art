using AlgorithmicArt.Core.Algorithm;
using AlgorithmicArt.Render;

namespace AlgorithmicArt.UI.Controls;

/// <summary>动画画布：双缓冲，按渲染器 + 场景 + 当前步骤重绘，支持步间插值动画。</summary>
public class AnimationCanvas : Control
{
    private readonly System.Windows.Forms.Timer _animTimer = new() { Interval = 16 };
    private int _animFrames;

    private IAnimRenderer? _renderer;
    private AlgorithmContext? _context;
    private AnimStep? _current;

    public AnimationCanvas()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

        _animTimer.Tick += (_, _) =>
        {
            Invalidate();
            if (--_animFrames <= 0) _animTimer.Stop();
        };
    }

    public void SetRenderer(IAnimRenderer? renderer)
    {
        _renderer = renderer;
        Invalidate();
    }

    public void SetContext(AlgorithmContext? context)
    {
        _context = context;
        Invalidate();
    }

    public void SetStep(AnimStep? step)
    {
        _current = step;
        if (step == null)
        {
            // 回到初始态时不需要插值，直接停掉后台重绘
            StopAnimation();
        }
        else
        {
            _animFrames = 20; // 约 320ms 的插值动画
            _animTimer.Start();
        }
        Invalidate();
    }

    /// <summary>停止步间插值定时器（切换页面/分类时调用，避免后台空转）。</summary>
    public void StopAnimation()
    {
        _animFrames = 0;
        _animTimer.Stop();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.Clear(BackColor);

        var bounds = new Rectangle(0, 0, Width, Height);
        if (_renderer != null && _context != null)
        {
            _renderer.Render(g, bounds, _context, _current);
        }
        else
        {
            TextRenderer.DrawText(g, "动画画布\r\n选择算法后自动开始演示", Font, bounds, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _animTimer.Stop();
            _animTimer.Dispose();
        }
        base.Dispose(disposing);
    }
}
