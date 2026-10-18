using System.Text;
using AlgorithmicArt.Core.Algorithm;
using AlgorithmicArt.Core.Replay;
using AlgorithmicArt.Data;
using AlgorithmicArt.Render;
using AlgorithmicArt.UI;
using AlgorithmicArt.UI.Controls;

namespace AlgorithmicArt.UI.Pages;

/// <summary>算法动画页面（参考 VisuAlgo）：算法切换 + 画布 + 伪代码 + 播放控制。</summary>
public class AlgorithmPage : UserControl
{
    private readonly ReplayEngine _replay;
    private readonly AlgorithmManager _manager;
    private readonly SimpleLogger _logger;
    private readonly AppConfig _config;

    private readonly AnimationCanvas _canvas = new();
    private readonly Label _lblOperation = new();

    private readonly FlowLayoutPanel _algoBar = new();
    private readonly FlowLayoutPanel _bottomBar = new();

    private readonly RoundedButton _btnBackward = new();
    private readonly RoundedButton _btnPlay = new();
    private readonly RoundedButton _btnForward = new();
    private readonly RoundedButton _btnReset = new();
    private readonly TrackBar _progress = new();
    private readonly Label _lblProgress = new();
    private readonly TrackBar _speed = new();
    private readonly Label _lblSpeed = new();
    private readonly RoundedButton _btnNew = new();
    private readonly RoundedButton _btnImport = new();

    private readonly RoundedDropDown _algoCombo = new();
    private bool _suppressAlgoChange;

    private string _currentCategory = "信号算法";
    private IAlgorithm? _currentAlgorithm;
    private AlgorithmContext? _currentContext;
    private IReadOnlyList<AnimStep>? _currentSteps;

    /// <summary>场景代次：切换分类/算法后，过期的后台生成结果会被丢弃。</summary>
    private int _sceneVersion;

    /// <summary>当前场景生成任务的取消源，切换时用于中止旧任务。</summary>
    private CancellationTokenSource? _sceneCts;

    public AlgorithmPage(ReplayEngine replay, AlgorithmManager manager, SimpleLogger logger, AppConfig config)
    {
        _replay = replay;
        _manager = manager;
        _logger = logger;
        _config = config;

        BackColor = Theme.CardBack;

        BuildAlgoBar();
        BuildMiddle();
        BuildBottomBar();

        Controls.Add(_bottomBar);
        Controls.Add(_algoBar);

        WireEvents();
        UpdatePlayButtons();
        UpdateProgress();
    }

    public void ShowCategory(string category)
    {
        // 切换分类前，先中止后台生成任务并停止回放
        _sceneVersion++; // 让尚未完成的旧生成任务失效
        CancelPendingScene();
        _canvas.StopAnimation();
        _replay.StopAndClear();

        _currentCategory = category;
        _currentAlgorithm = null;
        _currentContext = null;
        _currentSteps = null;

        _canvas.SetRenderer(null);
        _canvas.SetContext(null);
        _canvas.SetStep(null);
        _lblOperation.Text = "请选择算法";

        RebuildAlgoCombo();
    }

    /// <summary>取消仍在后台运行的旧生成任务。</summary>
    private void CancelPendingScene()
    {
        var cts = _sceneCts;
        _sceneCts = null;
        if (cts == null) return;

        try { cts.Cancel(); }
        catch { /* 已释放则忽略 */ }
        cts.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sceneVersion++;
            CancelPendingScene();
            _replay.StepChanged -= OnReplayStepChanged;
            _replay.StateChanged -= OnReplayStateChanged;
            _replay.StopAndClear();
            _canvas.StopAnimation();
        }
        base.Dispose(disposing);
    }

    // ---------- 顶部算法切换 ----------

    private void BuildAlgoBar()
    {
        _algoBar.Dock = DockStyle.Top;
        _algoBar.AutoSize = true;
        _algoBar.FlowDirection = FlowDirection.LeftToRight;
        _algoBar.WrapContents = false;
        _algoBar.Padding = new Padding(8, 6, 8, 6);
        _algoBar.BackColor = Theme.CardBack;

        _algoBar.Controls.Add(new Label
        {
            Text = "算法",
            AutoSize = true,
            ForeColor = Theme.TextSecondary,
            Margin = new Padding(4, 8, 4, 0)
        });

        _algoCombo.Width = 240;
        _algoCombo.DisplaySelector = item => item is IAlgorithm a
            ? (string.IsNullOrEmpty(a.Group) ? a.Name : $"{a.Group}：{a.Name}")
            : item.ToString() ?? "";
        _algoCombo.SelectedIndexChanged += (_, _) =>
        {
            if (_suppressAlgoChange) return;
            if (_algoCombo.SelectedItem is IAlgorithm algo) SelectAlgorithm(algo);
        };
        _algoBar.Controls.Add(_algoCombo);

        _btnNew.Text = "新建";
        _btnNew.Width = 64;
        _btnNew.ApplySecondary();
        _algoBar.Controls.Add(_btnNew);

        _btnImport.Text = "导入CSV";
        _btnImport.Width = 80;
        _btnImport.ApplySecondary();
        _algoBar.Controls.Add(_btnImport);
    }

    private void RebuildAlgoCombo()
    {
        _suppressAlgoChange = true;
        _algoCombo.ClearItems();

        foreach (var algo in _manager.GetByCategory(_currentCategory))
            _algoCombo.Items.Add(algo);

        _suppressAlgoChange = false;

        if (_algoCombo.Items.Count > 0)
        {
            _algoCombo.SelectedIndex = 0; // 触发选中，自动生成场景并播放
        }
        else
        {
            _currentAlgorithm = null;
            _lblOperation.Text = $"「{_currentCategory}」暂无已注册算法";
        }
    }

    private void SelectAlgorithm(IAlgorithm algo)
    {
        _currentAlgorithm = algo;
        BuildNewData();
    }

    /// <summary>重新生成当前算法场景（滤波系数变更后调用）。</summary>
    public void ReloadCurrentScene()
    {
        if (_currentAlgorithm != null) BuildNewData();
    }

    // ---------- 中央：画布 + 伪代码 ----------

    private void BuildMiddle()
    {
        var canvasPanel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.CardBack };
        _lblOperation.Dock = DockStyle.Bottom;
        _lblOperation.Height = 30;
        _lblOperation.Text = "请选择算法";
        _lblOperation.ForeColor = Theme.TextSecondary;
        _lblOperation.TextAlign = ContentAlignment.MiddleLeft;
        _lblOperation.Padding = new Padding(6, 0, 0, 0);
        _canvas.Dock = DockStyle.Fill;
        canvasPanel.Controls.Add(_canvas);
        canvasPanel.Controls.Add(_lblOperation);

        Controls.Add(canvasPanel);
    }

    // ---------- 底部控制栏 ----------

    private void BuildBottomBar()
    {
        _bottomBar.Dock = DockStyle.Top;
        _bottomBar.AutoSize = true;
        _bottomBar.FlowDirection = FlowDirection.LeftToRight;
        _bottomBar.WrapContents = false;
        _bottomBar.Padding = new Padding(8, 0, 8, 6);
        _bottomBar.BackColor = Theme.CardBack;

        _btnBackward.Text = "⏮";
        _btnBackward.Width = 44;
        _btnBackward.ApplySecondary();
        _btnPlay.Text = "▶";
        _btnPlay.Width = 60;
        _btnPlay.ApplyPrimary();
        _btnForward.Text = "⏭";
        _btnForward.Width = 44;
        _btnForward.ApplySecondary();
        _btnReset.Text = "↺";
        _btnReset.Width = 44;
        _btnReset.ApplySecondary();

        _bottomBar.Controls.Add(_btnBackward);
        _bottomBar.Controls.Add(_btnPlay);
        _bottomBar.Controls.Add(_btnForward);
        _bottomBar.Controls.Add(_btnReset);

        _bottomBar.Controls.Add(new Label { Text = "进度", AutoSize = true, Margin = new Padding(16, 8, 4, 0) });
        _progress.Minimum = 0;
        _progress.Maximum = 1;
        _progress.Value = 0;
        _progress.Width = 180;
        _progress.TickStyle = TickStyle.None;
        _bottomBar.Controls.Add(_progress);
        _lblProgress.Text = "0/0";
        _lblProgress.AutoSize = true;
        _lblProgress.Width = 56;
        _lblProgress.Margin = new Padding(6, 8, 0, 0);
        _bottomBar.Controls.Add(_lblProgress);

        _bottomBar.Controls.Add(new Label { Text = "速度", AutoSize = true, Margin = new Padding(16, 8, 4, 0) });
        _speed.Minimum = 1;
        _speed.Maximum = 100;
        _speed.Value = _replay.Speed;
        _speed.Width = 140;
        _speed.TickStyle = TickStyle.None;
        _bottomBar.Controls.Add(_speed);
        _lblSpeed.Text = FormatSpeed(_speed.Value);
        _lblSpeed.AutoSize = true;
        _lblSpeed.Width = 44;
        _lblSpeed.Margin = new Padding(6, 8, 0, 0);
        _bottomBar.Controls.Add(_lblSpeed);
    }

    // ---------- 事件 ----------

    private void WireEvents()
    {
        _btnBackward.Click += (_, _) => _replay.StepBackward();
        _btnPlay.Click += (_, _) => TogglePlay();
        _btnForward.Click += (_, _) => _replay.StepForward();
        _btnReset.Click += (_, _) => _replay.Reset();
        _btnNew.Click += (_, _) => BuildNewData();
        _btnImport.Click += (_, _) => ImportCsv();

        _speed.ValueChanged += (_, _) =>
        {
            _replay.Speed = _speed.Value;
            _lblSpeed.Text = FormatSpeed(_speed.Value);
        };

        _progress.Scroll += (_, _) => _replay.SeekTo(_progress.Value - 1);

        _replay.StepChanged += OnReplayStepChanged;
        _replay.StateChanged += OnReplayStateChanged;
    }

    private void OnReplayStepChanged(AnimStep? step)
    {
        _canvas.SetStep(step);
        _lblOperation.Text = step?.Description ?? "就绪";
        UpdateProgress();
        UpdatePlayButtons();
    }

    private void OnReplayStateChanged(SessionState _) => UpdatePlayButtons();

    private void TogglePlay()
    {
        if (_replay.State == SessionState.Running)
            _replay.Pause();
        else
            _replay.Play();
    }

    private async void BuildNewData()
    {
        var algo = _currentAlgorithm;
        if (algo == null)
        {
            _logger.Warn("请先选择算法");
            return;
        }

        int version = ++_sceneVersion;

        // 中止上一次仍在后台运行的生成任务
        CancelPendingScene();
        _sceneCts = new CancellationTokenSource();
        var token = _sceneCts.Token;

        // 立即响应：先停旧回放、清空画面，避免切换分类时 UI 卡住
        _canvas.StopAnimation();
        _replay.StopAndClear();
        _currentContext = null;
        _currentSteps = null;
        _canvas.SetRenderer(RendererFactory.Create(algo.Renderer));
        _canvas.SetContext(null);
        _canvas.SetStep(null);
        _lblOperation.Text = $"正在生成「{algo.Name}」场景…";
        UpdateProgress();

        AlgorithmContext ctx;
        IReadOnlyList<AnimStep> steps;
        bool truncated;

        try
        {
            // 场景构建 + 步骤录制放到后台线程，界面保持可操作
            (ctx, steps, truncated) = await Task.Run(() =>
            {
                var scene = SceneBuilder.Build(algo.Scene, _config.DataSize, _config.Filter, _config.Pid,
                    _config.MatrixRows, _config.MatrixCols);
                var recorder = new StepRecorder
                {
                    // 一旦切换分类/算法，录制点立即抛异常中止旧任务
                    CancellationToken = token
                };

                // 依据算法复杂度与数据量自动降采样，把快照总量控制在预算内
                recorder.RecordEvery = ComputeRecordEvery(algo, scene.Array?.Length ?? _config.DataSize);

                var list = _manager.Run(algo, scene, recorder);
                return (scene, list, recorder.Truncated);
            });
        }
        catch (OperationCanceledException)
        {
            // 已被新的切换请求取代，静默退出
            return;
        }
        catch (Exception ex)
        {
            _logger.Error($"生成场景失败：{ex.Message}");
            if (version == _sceneVersion && !IsDisposed)
                _lblOperation.Text = "场景生成失败";
            return;
        }

        // 期间用户已切换分类或算法，丢弃这次结果
        if (version != _sceneVersion || IsDisposed) return;

        _currentContext = ctx;
        _currentSteps = steps;

        _canvas.SetRenderer(RendererFactory.Create(algo.Renderer));
        _canvas.SetContext(ctx);
        _replay.Load(steps);
        _replay.Reset();

        _lblOperation.Text = truncated
            ? $"已生成：{algo.Name}，共 {steps.Count} 步（数据量大，已按采样录制）"
            : $"已生成：{algo.Name}，共 {steps.Count} 步";
        _logger.Info($"「{algo.Name}」运行完成，录制 {steps.Count} 步{(truncated ? "（已截断）" : "")}");

        // 生成后自动开始播放，确保动画立即可见
        if (steps.Count > 0) _replay.Play();
    }

    // CSV 导入上限
    private const int ImportLimitSort = 5000;
    private const int ImportLimitSearch = 5000;
    private const int ImportLimitSignal = 5000;
    private const int ImportLimitMl = 200;

    private async void ImportCsv()
    {
        if (_currentAlgorithm == null)
        {
            _logger.Warn("请先选择算法");
            return;
        }

        using var dlg = new OpenFileDialog
        {
            Title = "导入 CSV 数据",
            Filter = "CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        var ctx = new AlgorithmContext();
        int dataCount = 0;

        switch (_currentAlgorithm.Scene)
        {
            case SceneKind.Signal:
            {
                var data = CsvImporter.ReadColumn(dlg.FileName);
                if (data == null || data.Length < 2)
                {
                    _logger.Warn("CSV 中未找到有效数值列");
                    return;
                }
                if (data.Length > ImportLimitSignal)
                {
                    _logger.Warn($"信号点数 {data.Length} 超过上限，仅取前 {ImportLimitSignal} 点");
                    data = data.Take(ImportLimitSignal).ToArray();
                }

                ctx.Signal = data;
                dataCount = data.Length;
                ctx.Parameters["Window"] = _config.Filter.MovingWindow;
                ctx.Parameters["Fs"] = _config.Filter.SampleRate;
                ctx.Parameters["Fc"] = _config.Filter.CutoffFreq;
                ctx.Parameters["Quality"] = _config.Filter.Quality;
                ctx.Parameters["KalmanQ"] = _config.Filter.KalmanQ;
                ctx.Parameters["KalmanR"] = _config.Filter.KalmanR;
                break;
            }
            case SceneKind.SortedArray:
            case SceneKind.RandomArray:
            {
                var data = CsvImporter.ReadIntColumn(dlg.FileName);
                if (data == null || data.Length == 0)
                {
                    _logger.Warn("CSV 中未找到有效整数列");
                    return;
                }
                int limit = _currentAlgorithm.Scene == SceneKind.RandomArray
                    ? ImportLimitSort
                    : ImportLimitSearch;

                if (data.Length > limit)
                {
                    _logger.Warn($"数据量 {data.Length} 超过上限，仅取前 {limit} 个");
                    data = data.Take(limit).ToArray();
                }

                if (_currentAlgorithm.Scene == SceneKind.SortedArray)
                    Array.Sort(data); // 查找需要有序数组；排序保持原序用于演示
                ctx.Array = data;
                dataCount = data.Length;
                ctx.Parameters["Target"] = data[data.Length / 2];
                break;
            }
            case SceneKind.Trajectory:
            {
                var pts = CsvImporter.ReadPoints(dlg.FileName);
                if (pts == null || pts.Count < 2)
                {
                    _logger.Warn("CSV 需包含 x,y 两列坐标");
                    return;
                }
                NormalizePoints(pts);
                ctx.Path = pts;
                dataCount = pts.Count;
                ctx.Trace = new List<PointF>();
                ctx.Parameters["Kp"] = _config.Pid.Kp;
                ctx.Parameters["Ki"] = _config.Pid.Ki;
                ctx.Parameters["Kd"] = _config.Pid.Kd;
                ctx.Parameters["Speed"] = _config.Pid.Speed;
                ctx.Parameters["LookAhead"] = _config.Pid.LookAhead;
                ctx.Parameters["IntegralLimit"] = _config.Pid.IntegralLimit;
                ctx.Parameters["MaxSteer"] = _config.Pid.MaxSteer;
                ctx.Parameters["MaxSteps"] = _config.Pid.MaxSteps;
                break;
            }
            case SceneKind.MlClusters:
            case SceneKind.MlRegression:
            case SceneKind.MlClassification:
            {
                var pts = CsvImporter.ReadPoints(dlg.FileName);
                if (pts == null || pts.Count < 4)
                {
                    _logger.Warn("CSV 需包含 x,y 两列坐标（至少 4 行）");
                    return;
                }

                var ml = NormalizeMlPoints(pts);

                if (ml.Length > ImportLimitMl)
                {
                    _logger.Warn($"样本点 {ml.Length} 超过上限，仅保留前 {ImportLimitMl} 个");
                    Array.Resize(ref ml, ImportLimitMl);
                }

                if (_currentAlgorithm.Scene == SceneKind.MlClassification)
                {
                    float midY = ml.Average(p => p.Y);
                    foreach (var p in ml) p.Label = p.Y >= midY ? 1 : 0;
                    ctx.QueryPoint = new PointF(ml.Average(p => p.X), midY);
                }
                else if (_currentAlgorithm.Scene == SceneKind.MlClusters)
                {
                    ctx.ClusterOf = Enumerable.Repeat(-1, ml.Length).ToArray();
                }

                ctx.MlPoints = ml;
                dataCount = ml.Length;
                ctx.Parameters["K"] = _currentAlgorithm.Scene == SceneKind.MlClusters ? 3 : 5;
                break;
            }
            default:
                _logger.Warn("当前分类暂不支持导入 CSV");
                return;
        }

        var algo = _currentAlgorithm;
        var recorder = new StepRecorder();
        recorder.RecordEvery = ComputeRecordEvery(algo, ctx.Array?.Length ?? _config.DataSize);

        int version = ++_sceneVersion;

        CancelPendingScene();
        _sceneCts = new CancellationTokenSource();
        recorder.CancellationToken = _sceneCts.Token;

        _canvas.StopAnimation();
        _replay.StopAndClear();
        _lblOperation.Text = "正在处理导入数据…";

        IReadOnlyList<AnimStep> steps;
        try
        {
            // 大样本排序可能耗时，放到后台避免界面卡住
            steps = await Task.Run(() => _manager.Run(algo, ctx, recorder));
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            _logger.Error($"导入处理失败：{ex.Message}");
            return;
        }

        if (version != _sceneVersion || IsDisposed) return;

        _currentContext = ctx;
        _currentSteps = steps;

        _canvas.SetRenderer(RendererFactory.Create(algo.Renderer));
        _canvas.SetContext(ctx);
        _replay.Load(steps);
        _replay.Reset();

        _lblOperation.Text = $"已导入 {Path.GetFileName(dlg.FileName)}，数据量 {dataCount}，共 {steps.Count} 步";
        _logger.Info($"导入 CSV：{dlg.FileName}，数据量 {dataCount}，录制 {steps.Count} 步");
        if (steps.Count > 0) _replay.Play();
    }

    /// <summary>导出当前场景数据为 CSV。</summary>
    public void ExportCsv(string path)
    {
        if (_currentContext == null)
        {
            _logger.Warn("暂无数据可导出，请先运行算法");
            return;
        }

        try
        {
            using var sw = new StreamWriter(path, false, Encoding.UTF8);
            switch (_currentAlgorithm?.Scene)
            {
                case SceneKind.Signal:
                {
                    var sig = _currentContext.Signal ?? Array.Empty<double>();
                    var filtered = _currentSteps?.LastOrDefault()?.Extra as double[];
                    sw.WriteLine("序号,输入信号,滤波输出");
                    for (int i = 0; i < sig.Length; i++)
                    {
                        string f = filtered != null && i < filtered.Length ? filtered[i].ToString("F6") : "";
                        sw.WriteLine($"{i},{sig[i]:F6},{f}");
                    }
                    break;
                }
                case SceneKind.SortedArray:
                case SceneKind.RandomArray:
                {
                    var arr = _currentContext.Array ?? Array.Empty<int>();
                    sw.WriteLine("序号,数值");
                    for (int i = 0; i < arr.Length; i++)
                        sw.WriteLine($"{i},{arr[i]}");
                    break;
                }
                case SceneKind.GridMap:
                {
                    var map = _currentContext.Map;
                    if (map != null)
                    {
                        sw.WriteLine("X,Y,状态");
                        for (int y = 0; y < map.Height; y++)
                            for (int x = 0; x < map.Width; x++)
                                sw.WriteLine($"{x},{y},{map.Get(x, y)}");
                    }
                    break;
                }
                case SceneKind.Trajectory:
                {
                    var pts = _currentContext.Path;
                    if (pts != null)
                    {
                        sw.WriteLine("X,Y");
                        foreach (var p in pts)
                            sw.WriteLine($"{p.X:F3},{p.Y:F3}");
                    }
                    break;
                }
                case SceneKind.MlClusters:
                case SceneKind.MlRegression:
                case SceneKind.MlClassification:
                {
                    var pts = _currentContext.MlPoints;
                    if (pts != null)
                    {
                        sw.WriteLine("X,Y,标签");
                        foreach (var p in pts)
                            sw.WriteLine($"{p.X:F3},{p.Y:F3},{p.Label}");
                    }
                    break;
                }
            }
            _logger.Info($"已导出 CSV：{path}");
        }
        catch (Exception ex)
        {
            _logger.Error($"导出失败：{ex.Message}");
        }
    }

    private static void NormalizePoints(List<PointF> pts)
    {
        float minX = pts.Min(p => p.X), maxX = pts.Max(p => p.X);
        float minY = pts.Min(p => p.Y), maxY = pts.Max(p => p.Y);
        float spanX = Math.Max(maxX - minX, 1e-3f);
        float spanY = Math.Max(maxY - minY, 1e-3f);

        for (int i = 0; i < pts.Count; i++)
        {
            pts[i] = new PointF(
                10 + (pts[i].X - minX) / spanX * 80,
                10 + (pts[i].Y - minY) / spanY * 80);
        }
    }

    /// <summary>
    /// 按「快照总量约 50MB」预算计算采样间隔：
    /// 每步快照约 n×4 字节，用算法估算的总步数除以预算步数即得间隔。
    /// </summary>
    private static int ComputeRecordEvery(IAlgorithm algo, int n)
    {
        if (n <= 0) return 1;

        // 只有会保存数据快照的场景才需要降采样；查找类只记录索引，内存很小
        long bytesPerStep = algo.Scene switch
        {
            SceneKind.RandomArray => 4L * n,
            SceneKind.Signal => 4L * n,
            _ => 0
        };

        if (bytesPerStep <= 0) return 1;

        long estimate = Math.Max(1, algo.EstimateSteps(n));
        long budgetSteps = Math.Max(1, 50L * 1024 * 1024 / bytesPerStep);
        if (estimate <= budgetSteps) return 1;

        return (int)Math.Clamp((estimate + budgetSteps - 1) / budgetSteps, 1, 100_000);
    }

    /// <summary>把 CSV 读入的原始坐标归一化到 4~96，适配机器学习的 0~100 坐标系。</summary>
    private static MlPoint[] NormalizeMlPoints(List<PointF> pts)
    {
        float minX = pts.Min(p => p.X), maxX = pts.Max(p => p.X);
        float minY = pts.Min(p => p.Y), maxY = pts.Max(p => p.Y);
        float spanX = Math.Max(maxX - minX, 1e-3f);
        float spanY = Math.Max(maxY - minY, 1e-3f);

        var result = new MlPoint[pts.Count];
        for (int i = 0; i < pts.Count; i++)
        {
            result[i] = new MlPoint(
                4 + (pts[i].X - minX) / spanX * 92,
                4 + (pts[i].Y - minY) / spanY * 92);
        }
        return result;
    }

    private void UpdateProgress()
    {
        int total = _replay.TotalSteps;
        _progress.Maximum = Math.Max(total, 1);
        _progress.Value = Math.Clamp(_replay.CurrentIndex + 1, 0, _progress.Maximum);
        _lblProgress.Text = $"{_replay.CurrentIndex + 1}/{total}";
    }

    private void UpdatePlayButtons()
    {
        _btnPlay.Text = _replay.State == SessionState.Running ? "⏸" : "▶";
    }

    private static string FormatSpeed(int speed) => $"{speed / 50.0:0.0} 倍";
}
