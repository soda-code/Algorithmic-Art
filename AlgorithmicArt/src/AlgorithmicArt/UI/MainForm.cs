using System.Runtime.InteropServices;
using AlgorithmicArt.Core.Algorithm;
using AlgorithmicArt.Core.Replay;
using AlgorithmicArt.Data;
using AlgorithmicArt.UI.Controls;
using AlgorithmicArt.UI.Pages;

namespace AlgorithmicArt.UI;

/// <summary>主窗体：主菜单 + 左侧算法分类 + 内容区 + 状态栏。</summary>
public class MainForm : Form
{
    private readonly SimpleLogger _logger = new();
    private readonly ConfigStore _configStore = new();
    private readonly AlgorithmManager _algorithmManager = new();
    private readonly ReplayEngine _replay = new();
    private readonly AppConfig _config;

    private readonly AlgorithmPage _algorithmPage;

    private readonly MenuStrip _menu = new();
    private readonly RoundedPanel _navCard = new();
    private readonly RoundedPanel _contentCard = new();
    private readonly RoundedDropDown _navGroupCombo = new();
    private readonly ListBox _subNav = new();

    /// <summary>导航结构：大类 → 子分类。</summary>
    private static readonly (string Name, string[] Categories)[] NavGroups =
    {
        ("经典算法", new[] { "查找算法", "排序算法", "信号算法", "控制算法", "机器学习算法" }),
        ("线性代数", new[] { "矩阵计算", "方程组计算", "其他" })
    };
    private readonly Panel _content = new();
    private readonly TableLayoutPanel _body = new();
    private readonly StatusStrip _status = new();
    private readonly ToolStripStatusLabel _statusPage = new();
    private readonly ToolStripStatusLabel _statusData = new();
    private readonly Dictionary<int, ToolStripMenuItem> _dataSizeItems = new();
    private readonly ToolStripMenuItem _miFilter = new();
    private readonly ToolStripMenuItem _miPid = new();
    private readonly ToolStripMenuItem _miMatrixRows = new();
    private readonly ToolStripMenuItem _miMatrixCols = new();
    private readonly Dictionary<int, ToolStripMenuItem> _matrixRowItems = new();
    private readonly Dictionary<int, ToolStripMenuItem> _matrixColItems = new();

    // 自绘标题栏
    private readonly Panel _titleBar = new();
    private readonly Label _titleText = new();
    private readonly WindowButton _btnMin = new();
    private readonly WindowButton _btnMax = new();
    private readonly WindowButton _btnClose = new();

    private int _navHoverIndex = -1;
    private bool _useRegionCorners;
    private bool _isFullScreen;

    // ---------- 无边框窗口（Win32） ----------

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateRoundRectRgn(int left, int top, int right, int bottom, int width, int height);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hwnd, int msg, int wParam, int lParam);

    private const int DwmWindowCornerPreference = 33;
    private const int DwmCornerRound = 2;

    private const int WmNcHitTest = 0x0084;
    private const int WmNcLButtonDown = 0x00A1;
    private const int HtCaption = 0x0002;
    private const int HtLeft = 10, HtRight = 11, HtTop = 12, HtTopLeft = 13;
    private const int HtTopRight = 14, HtBottom = 15, HtBottomLeft = 16, HtBottomRight = 17;
    private const int ResizeBorder = 7;
    private const int CsDropShadow = 0x00020000;

    public MainForm()
    {
        Text = "算法动画上位机";

        var appIcon = LoadAppIcon();
        if (appIcon != null) Icon = appIcon;

        FormBorderStyle = FormBorderStyle.None; // 无边框：由自绘标题栏 + 圆角 Region 组成
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1100, 700);
        MinimumSize = new Size(900, 600);
        BackColor = Theme.WindowBack;
        Font = Theme.BaseFont;
        DoubleBuffered = true;
        Padding = new Padding(1); // 让圆角裁切落在窗口底色上，弱化边缘硬切感

        _config = _configStore.Load();
        // 旧配置可能保存了超大数值，这里收敛到安全范围
        _config.DataSize = Math.Clamp(_config.DataSize, 10, 5000);
        _config.MatrixRows = Math.Clamp(_config.MatrixRows, 2, 6);
        _config.MatrixCols = Math.Clamp(_config.MatrixCols, 2, 6);
        AlgorithmRegistry.RegisterAll(_algorithmManager);
        ApplyBackground();
        _replay.Speed = Math.Clamp(_config.Speed, 1, 100);
        _algorithmPage = new AlgorithmPage(_replay, _algorithmManager, _logger, _config);

        BuildMenu();
        BuildLayout();
        WireEvents();

        RebuildSubNav(); // 依据默认大类构建子导航并选中首项
        _logger.Info("应用启动");
    }

    /// <summary>从嵌入资源读取应用图标（与 exe 图标一致）。</summary>
    private static Icon? LoadAppIcon()
    {
        try
        {
            using var stream = typeof(MainForm).Assembly.GetManifestResourceStream("app.ico");
            return stream != null ? new Icon(stream) : null;
        }
        catch
        {
            return null;
        }
    }

    // ---------- 菜单 ----------

    private void BuildMenu()
    {
        var miFile = new ToolStripMenuItem("文件(&F)");
        var miExport = new ToolStripMenuItem("导出数据(CSV)(&E)");
        var miExit = new ToolStripMenuItem("退出(&X)");
        miExport.Click += (_, _) => ExportCsv();
        miExit.Click += (_, _) => Close();
        miFile.DropDownItems.AddRange(new ToolStripItem[] { miExport, miExit });

        var miView = new ToolStripMenuItem("视图(&V)");
        var miBg = new ToolStripMenuItem("设置背景图片(&B)");
        var miBgClear = new ToolStripMenuItem("清除背景(&C)");
        var miFull = new ToolStripMenuItem("全屏(&F)");
        miBg.Click += (_, _) => SetBackgroundImage();
        miBgClear.Click += (_, _) => ClearBackgroundImage();
        miFull.Click += (_, _) => ToggleFullScreen();
        miView.DropDownItems.AddRange(new ToolStripItem[] { miBg, miBgClear, miFull });

        var miParam = new ToolStripMenuItem("参数(&P)");
        _miFilter.Text = "滤波系数设定(&F)";
        _miFilter.Click += (_, _) => ShowFilterSettings();
        _miFilter.Visible = false; // 仅信号算法时显示
        miParam.DropDownItems.Add(_miFilter);

        _miPid.Text = "PID 参数(&P)";
        _miPid.Click += (_, _) => ShowPidSettings();
        _miPid.Visible = false; // 仅控制算法时显示
        miParam.DropDownItems.Add(_miPid);

        var miDataSize = new ToolStripMenuItem("数据量(&D)");
        foreach (var size in new[] { 10, 50, 100, 200, 500, 1000, 2000, 5000 })
        {
            var item = new ToolStripMenuItem($"{size}", null, (_, _) => SetDataSize(size))
            {
                Checked = _config.DataSize == size
            };
            _dataSizeItems[size] = item;
            miDataSize.DropDownItems.Add(item);
        }
        miParam.DropDownItems.Add(miDataSize);

        // 线性代数：矩阵行列数
        _miMatrixRows.Text = "矩阵行数(&R)";
        _miMatrixRows.Visible = false;
        foreach (var v in new[] { 2, 3, 4, 5, 6 })
        {
            var item = new ToolStripMenuItem($"{v}", null, (_, _) => SetMatrixSize(v, null))
            {
                Checked = _config.MatrixRows == v
            };
            _matrixRowItems[v] = item;
            _miMatrixRows.DropDownItems.Add(item);
        }
        miParam.DropDownItems.Add(_miMatrixRows);

        _miMatrixCols.Text = "矩阵列数(&C)";
        _miMatrixCols.Visible = false;
        foreach (var v in new[] { 2, 3, 4, 5, 6 })
        {
            var item = new ToolStripMenuItem($"{v}", null, (_, _) => SetMatrixSize(null, v))
            {
                Checked = _config.MatrixCols == v
            };
            _matrixColItems[v] = item;
            _miMatrixCols.DropDownItems.Add(item);
        }
        miParam.DropDownItems.Add(_miMatrixCols);

        var miHelp = new ToolStripMenuItem("帮助(&H)");
        var miCsv = new ToolStripMenuItem("CSV 填写格式(&C)");
        var miAbout = new ToolStripMenuItem("关于(&A)");
        miCsv.Click += (_, _) => ShowCsvFormat();
        miAbout.Click += (_, _) => MessageBox.Show(
            "算法动画上位机\n基于 .NET 8 + WinForms + GDI+ 双缓冲",
            "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
        miHelp.DropDownItems.AddRange(new ToolStripItem[] { miCsv, miAbout });

        _menu.Items.AddRange(new ToolStripItem[] { miFile, miView, miParam, miHelp });
    }

    // ---------- 布局 ----------

    private void BuildLayout()
    {
        BuildTitleBar();

        MainMenuStrip = _menu;
        _menu.Dock = DockStyle.Top;
        _menu.BackColor = Theme.CardBack;

        _navCard.Dock = DockStyle.Fill;
        _navCard.FillColor = Theme.CardBack;
        _navCard.BorderColor = Theme.Border;
        _navCard.BorderWidth = 1;
        _navCard.CornerRadius = 12;
        _navCard.Padding = new Padding(10);
        _navCard.Margin = new Padding(0, 0, 12, 0);

        // 大类下拉：经典算法 / 线性代数
        _navGroupCombo.Dock = DockStyle.Top;
        _navGroupCombo.Height = 34;
        _navGroupCombo.Font = Theme.BaseFont;
        _navGroupCombo.AddRange(NavGroups.Select(g => (object)g.Name));

        _subNav.Dock = DockStyle.Fill;
        _subNav.BorderStyle = BorderStyle.None;
        _subNav.IntegralHeight = false;
        _subNav.BackColor = Theme.CardBack;
        _subNav.Font = Theme.NavFont;
        _subNav.DrawMode = DrawMode.OwnerDrawFixed;
        _subNav.ItemHeight = 38;
        _subNav.DrawItem += OnDrawNavItem;

        _navCard.Controls.Add(_subNav);
        _navCard.Controls.Add(_navGroupCombo);

        _contentCard.Dock = DockStyle.Fill;
        _contentCard.FillColor = Theme.CardBack;
        _contentCard.BorderColor = Theme.Border;
        _contentCard.BorderWidth = 1;
        _contentCard.CornerRadius = 12;
        _contentCard.Padding = new Padding(8);

        _content.Dock = DockStyle.Fill;
        _content.BackColor = Theme.CardBack;
        _algorithmPage.Dock = DockStyle.Fill;
        _content.Controls.Add(_algorithmPage);
        _contentCard.Controls.Add(_content);

        _body.Dock = DockStyle.Fill;
        _body.ColumnCount = 2;
        _body.RowCount = 1;
        _body.BackColor = Theme.WindowBack;
        _body.Padding = new Padding(12);
        _body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
        _body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _body.Controls.Add(_navCard, 0, 0);
        _body.Controls.Add(_contentCard, 1, 0);

        _status.Dock = DockStyle.Bottom;
        _status.BackColor = Theme.StatusBarBack;
        _statusPage.Text = "页面：算法动画";
        _statusData.Text = $"数据量：{_config.DataSize}";
        _statusPage.ForeColor = Theme.TextSecondary;
        _statusData.ForeColor = Theme.TextSecondary;
        _status.Items.AddRange(new ToolStripItem[] { _statusPage, _statusData });

        Controls.Add(_body);
        Controls.Add(_status);
        Controls.Add(_menu);
        Controls.Add(_titleBar);
    }

    // ---------- 自绘标题栏 ----------

    private void BuildTitleBar()
    {
        _titleBar.Dock = DockStyle.Top;
        _titleBar.Height = 44;
        _titleBar.BackColor = Theme.CardBack;
        _titleBar.Paint += OnTitleBarPaint;
        _titleBar.MouseDown += OnTitleBarMouseDown;
        _titleBar.Resize += (_, _) => LayoutTitleButtons();

        _titleText.Dock = DockStyle.Fill;
        _titleText.Text = "算法动画上位机";
        _titleText.Font = new Font("Microsoft YaHei UI", 10f, FontStyle.Bold);
        _titleText.ForeColor = Theme.TextPrimary;
        _titleText.TextAlign = ContentAlignment.MiddleLeft;
        _titleText.Padding = new Padding(44, 0, 0, 0);
        _titleText.BackColor = Color.Transparent;
        _titleText.MouseDown += OnTitleBarMouseDown;

        _titleBar.Controls.Add(_titleText);

        _btnMin.Kind = WindowButtonKind.Minimize;
        _btnMax.Kind = WindowButtonKind.Maximize;
        _btnClose.Kind = WindowButtonKind.Close;

        foreach (var btn in new[] { _btnMin, _btnMax, _btnClose })
        {
            btn.Size = new Size(42, 30);
            btn.CornerRadius = 7;
            btn.FillColor = Color.Transparent;
            btn.HoverColor = Theme.SecondaryHover;
            btn.PressedColor = Theme.SecondaryPressed;
            btn.IconColor = Theme.TextSecondary;
            btn.HoverIconColor = Theme.TextPrimary;
            _titleBar.Controls.Add(btn);
        }

        _btnClose.HoverColor = Color.FromArgb(232, 17, 35);
        _btnClose.PressedColor = Color.FromArgb(200, 10, 30);
        _btnClose.HoverIconColor = Color.White;

        _btnMin.Click += (_, _) => WindowState = FormWindowState.Minimized;
        _btnMax.Click += (_, _) => ToggleMaximize();
        _btnClose.Click += (_, _) => Close();

        // 标题文字是 Dock=Fill，会盖住手动定位的按钮，必须把按钮提到最上层
        _btnMin.BringToFront();
        _btnMax.BringToFront();
        _btnClose.BringToFront();

        var tip = new ToolTip();
        tip.SetToolTip(_btnMin, "最小化");
        tip.SetToolTip(_btnMax, "最大化 / 还原");
        tip.SetToolTip(_btnClose, "关闭");

        LayoutTitleButtons();
    }

    private void LayoutTitleButtons()
    {
        const int width = 42, height = 30, gap = 2, rightPad = 8;
        int y = Math.Max(0, (_titleBar.Height - height) / 2);
        int right = _titleBar.Width - rightPad;

        _btnClose.Location = new Point(right - width, y);
        _btnMax.Location = new Point(right - width * 2 - gap, y);
        _btnMin.Location = new Point(right - width * 3 - gap * 2, y);
    }

    private void OnTitleBarPaint(object? sender, PaintEventArgs e)
    {
        using (var pen = new Pen(Theme.Border, 1f))
            e.Graphics.DrawLine(pen, 0, _titleBar.Height - 1, _titleBar.Width, _titleBar.Height - 1);

        if (Icon != null)
        {
            const int size = 18;
            e.Graphics.DrawIcon(Icon, new Rectangle(14, (_titleBar.Height - size) / 2, size, size));
        }
    }

    private void OnTitleBarMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;

        if (e.Clicks == 2)
        {
            ToggleMaximize();
            return;
        }

        // 交给系统处理拖动（支持贴边分屏）
        ReleaseCapture();
        SendMessage(Handle, WmNcLButtonDown, HtCaption, 0);
    }

    private void ToggleMaximize()
    {
        if (_isFullScreen) return;

        WindowState = WindowState == FormWindowState.Maximized
            ? FormWindowState.Normal
            : FormWindowState.Maximized;

        UpdateMaxButton();
        RefreshMaxButtonLater();
    }

    private void UpdateMaxButton()
    {
        // 最大化时切换为「还原（缩小）」图标：两个重叠方框
        _btnMax.Kind = WindowState == FormWindowState.Maximized
            ? WindowButtonKind.Restore
            : WindowButtonKind.Maximize;
        _btnMax.Invalidate();
    }

    /// <summary>窗口状态变化后（异步）再刷新一次按钮图标，避免时序问题。</summary>
    private void RefreshMaxButtonLater()
    {
        if (IsHandleCreated && !IsDisposed)
            BeginInvoke(new Action(UpdateMaxButton));
    }

    private void WireEvents()
    {
        _navGroupCombo.SelectedIndexChanged += (_, _) => RebuildSubNav();
        _subNav.SelectedIndexChanged += (_, _) => OnSubNavSelected();
        _subNav.MouseMove += (_, e) =>
        {
            int idx = _subNav.IndexFromPoint(e.Location);
            if (idx != _navHoverIndex)
            {
                _navHoverIndex = idx;
                _subNav.Invalidate();
            }
        };
        _subNav.MouseLeave += (_, _) => { _navHoverIndex = -1; _subNav.Invalidate(); };

        FormClosing += (_, _) =>
        {
            SaveConfig();
            // 关闭窗口：停止所有动画后台任务并释放定时器
            _replay.StopAndClear();
            _replay.Dispose();
        };
    }

    private void OnDrawNavItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;
        var text = _subNav.Items[e.Index]?.ToString() ?? string.Empty;
        bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // 先擦除整项背景，否则切换时旧的蓝色高亮会残留成「两项同时选中」
        using (var eraser = new SolidBrush(_subNav.BackColor))
            g.FillRectangle(eraser, e.Bounds);

        var rect = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 3, e.Bounds.Width - 4, e.Bounds.Height - 6);

        if (selected)
        {
            using var path = Theme.RoundedRect(rect, 8);
            using var brush = new SolidBrush(Theme.NavItemSelected);
            g.FillPath(brush, path);
            TextRenderer.DrawText(g, text, Theme.NavFont, rect, Color.White,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }
        else if (e.Index == _navHoverIndex)
        {
            using var path = Theme.RoundedRect(rect, 8);
            using var brush = new SolidBrush(Theme.NavItemHover);
            g.FillPath(brush, path);
            TextRenderer.DrawText(g, text, Theme.NavFont, rect, Theme.TextPrimary,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }
        else
        {
            TextRenderer.DrawText(g, text, Theme.NavFont, rect, Theme.TextPrimary,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }
    }

    /// <summary>按当前大类重建左侧子分类列表，并默认选中第一项。</summary>
    private void RebuildSubNav()
    {
        int index = Math.Clamp(_navGroupCombo.SelectedIndex, 0, NavGroups.Length - 1);
        var group = NavGroups[index];

        _subNav.BeginUpdate();
        _subNav.Items.Clear();
        foreach (var category in group.Categories)
            _subNav.Items.Add(category);
        _subNav.EndUpdate();

        _navHoverIndex = -1;

        if (_subNav.Items.Count > 0)
            _subNav.SelectedIndex = 0;
    }

    private void OnSubNavSelected()
    {
        if (_subNav.SelectedItem is not string item) return;

        _navHoverIndex = -1;
        _subNav.Invalidate(); // 强制整列重绘，清掉上一项的选中底色

        _algorithmPage.ShowCategory(item);
        _statusPage.Text = $"页面：算法动画/{item}";

        // 按分类显示对应参数菜单
        _miFilter.Visible = item == "信号算法";
        _miPid.Visible = item == "控制算法";

        bool linearAlgebra = _navGroupCombo.Text == "线性代数";
        _miMatrixRows.Visible = linearAlgebra;
        _miMatrixCols.Visible = linearAlgebra;
    }

    // ---------- 参数 ----------

    private void SetDataSize(int size)
    {
        _config.DataSize = size;
        foreach (var (key, item) in _dataSizeItems)
            item.Checked = key == size;

        _statusData.Text = $"数据量：{size}";
        _configStore.Save(_config);
        _algorithmPage.ReloadCurrentScene();
        _logger.Info($"数据量已设为 {size}");
    }

    private void SetMatrixSize(int? rows, int? cols)
    {
        if (rows.HasValue)
        {
            _config.MatrixRows = Math.Clamp(rows.Value, 2, 6);
            foreach (var (key, item) in _matrixRowItems)
                item.Checked = key == _config.MatrixRows;
        }

        if (cols.HasValue)
        {
            _config.MatrixCols = Math.Clamp(cols.Value, 2, 6);
            foreach (var (key, item) in _matrixColItems)
                item.Checked = key == _config.MatrixCols;
        }

        _configStore.Save(_config);
        _algorithmPage.ReloadCurrentScene();
        _logger.Info($"矩阵尺寸已设为 {_config.MatrixRows}×{_config.MatrixCols}");
    }

    private void ShowPidSettings()
    {
        using var form = new PidSettingsForm(_config.Pid);
        if (form.ShowDialog(this) != DialogResult.OK) return;

        _configStore.Save(_config);
        _algorithmPage.ReloadCurrentScene();
        _logger.Info("PID 参数已更新");
    }

    private void ShowFilterSettings()
    {
        using var form = new FilterSettingsForm(_config.Filter);
        if (form.ShowDialog(this) != DialogResult.OK) return;

        _configStore.Save(_config);
        _algorithmPage.ReloadCurrentScene();
        _logger.Info("滤波系数已更新");
    }

    // ---------- 帮助 ----------

    private void ShowCsvFormat()
    {
        using var form = new Form
        {
            Text = "CSV 填写格式说明",
            Size = new Size(680, 580),
            MinimumSize = new Size(520, 400),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.White,
            Font = Theme.BaseFont
        };

        var rtb = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = Color.White,
            Font = new Font("Microsoft YaHei UI", 9.5f),
            DetectUrls = false
        };
        rtb.Text = CsvHelpText;
        form.Controls.Add(rtb);
        form.ShowDialog(this);
    }

    private const string CsvHelpText = """
CSV 数据填写格式说明

一、通用规则
1. 支持 .csv 文件（也可选 .txt）。
2. 分隔符支持：逗号（,）、分号（;）、制表符（Tab）。
3. 自动跳过空行和无法解析的行。
4. 数值使用英文小数点，例如 3.14。
5. 不需要表头。

二、各分类格式

1. 信号算法（滤波）
   格式：单列数值，每行一个采样值。
   示例：
     0.12
     0.35
    -0.08
     0.42

2. 查找算法 / 排序算法
   格式：单列整数，每行一个。
   示例：
     15
      7
     23
      9
   说明：查找导入后自动升序，目标取中间值；排序保持原序用于演示。

3. 控制算法 - 汽车（PID 循迹）
   格式：两列坐标，第 1 列 x、第 2 列 y。
   示例：
   0,0
   10,5
   20,12
   30,18
   说明：作为参考路径，导入后自动归一化到画布范围。

4. 控制算法 - 扫地（蛇形清扫）
   暂不支持导入 CSV。

三、操作方式
   算法页 → 选择分类和算法 → 点击「导入CSV」→ 选择文件。
""";

    // ---------- 导出 ----------

    private void ExportCsv()
    {
        using var dlg = new SaveFileDialog
        {
            Title = "导出数据为 CSV",
            Filter = "CSV 文件 (*.csv)|*.csv",
            FileName = $"数据_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        _algorithmPage.ExportCsv(dlg.FileName);
    }

    // ---------- 圆角窗口 ----------

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyRoundedWindow();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        // Region 方案需要随窗口尺寸重建；DWM 方案由系统自动处理
        if (_useRegionCorners) ApplyRegionCorners();
        LayoutTitleButtons();
        UpdateMaxButton();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ClassStyle |= CsDropShadow; // 无边框窗口的系统投影
            return cp;
        }
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);

        if (m.Msg != WmNcHitTest) return;
        if (_isFullScreen || WindowState != FormWindowState.Normal) return;

        long lp = m.LParam.ToInt64();
        var screen = new Point((short)(lp & 0xFFFF), (short)((lp >> 16) & 0xFFFF));
        int hit = HitTestBorder(PointToClient(screen));
        if (hit != 0) m.Result = (IntPtr)hit;
    }

    /// <summary>命中窗口边缘时返回缩放热区，实现无边框窗口的自由缩放。</summary>
    private int HitTestBorder(Point p)
    {
        bool left = p.X <= ResizeBorder;
        bool right = p.X >= ClientSize.Width - ResizeBorder;
        bool top = p.Y <= ResizeBorder;
        bool bottom = p.Y >= ClientSize.Height - ResizeBorder;

        if (left && top) return HtTopLeft;
        if (right && top) return HtTopRight;
        if (left && bottom) return HtBottomLeft;
        if (right && bottom) return HtBottomRight;
        if (left) return HtLeft;
        if (right) return HtRight;
        if (top) return HtTop;
        if (bottom) return HtBottom;
        return 0;
    }

    /// <summary>优先使用 Windows 11 DWM 圆角（带阴影与抗锯齿），不支持时用 Region 裁剪保底。</summary>
    private void ApplyRoundedWindow()
    {
        try
        {
            int preference = DwmCornerRound;
            if (DwmSetWindowAttribute(Handle, DwmWindowCornerPreference, ref preference, sizeof(int)) == 0)
                return;
        }
        catch
        {
            // 旧系统无 dwmapi 对应属性，走 Region
        }

        _useRegionCorners = true;
        ApplyRegionCorners();
    }

    private void ApplyRegionCorners()
    {
        if (!_useRegionCorners) return;

        // 最大化时不需要圆角，避免屏幕四角露白
        if (WindowState == FormWindowState.Maximized)
        {
            Region = null;
            return;
        }

        // CreateRoundRectRgn 的宽高是椭圆直径，半径取一半
        const int radius = 12;
        IntPtr region = CreateRoundRectRgn(0, 0, Width + 1, Height + 1, radius * 2, radius * 2);
        var previous = Region;
        Region = System.Drawing.Region.FromHrgn(region);
        previous?.Dispose();
        DeleteObject(region);
    }

    // ---------- 背景 ----------

    private void SetBackgroundImage()
    {
        using var dlg = new OpenFileDialog
        {
            Title = "选择背景图片",
            Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif|所有文件|*.*"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        if (!TryLoadBackground(dlg.FileName))
        {
            MessageBox.Show("加载背景图片失败，请检查文件是否有效。", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _config.BackgroundImagePath = dlg.FileName;
    }

    private void ClearBackgroundImage()
    {
        BackgroundImage?.Dispose();
        BackgroundImage = null;
        _config.BackgroundImagePath = "";
    }

    private void ApplyBackground()
    {
        if (string.IsNullOrEmpty(_config.BackgroundImagePath)) return;
        if (!File.Exists(_config.BackgroundImagePath)) return;
        TryLoadBackground(_config.BackgroundImagePath);
    }

    private bool TryLoadBackground(string path)
    {
        try
        {
            using var fs = File.OpenRead(path);
            using var img = Image.FromStream(fs);

            // 超大背景图先缩放到 1920 以内，避免原图整张常驻内存
            const int maxSide = 1920;
            int w = img.Width, h = img.Height;
            if (Math.Max(w, h) > maxSide)
            {
                float scale = maxSide / (float)Math.Max(w, h);
                w = Math.Max(1, (int)(w * scale));
                h = Math.Max(1, (int)(h * scale));
            }

            var scaled = new Bitmap(w, h);
            using (var g = Graphics.FromImage(scaled))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, w, h);
            }

            BackgroundImage?.Dispose();
            BackgroundImage = scaled;
            BackgroundImageLayout = ImageLayout.Stretch;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void ToggleFullScreen()
    {
        _isFullScreen = !_isFullScreen;

        _titleBar.Visible = !_isFullScreen;
        _menu.Visible = !_isFullScreen;
        _status.Visible = !_isFullScreen;

        WindowState = _isFullScreen ? FormWindowState.Maximized : FormWindowState.Normal;

        if (_isFullScreen)
        {
            Region = null;
        }
        else if (_useRegionCorners)
        {
            ApplyRegionCorners();
        }

        UpdateMaxButton();
    }

    private void SaveConfig()
    {
        _config.Speed = _replay.Speed;
        _configStore.Save(_config);
    }
}
