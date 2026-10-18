namespace AlgorithmicArt.Core.Algorithm;

/// <summary>步骤录制器：支持采样间隔、步数上限与按需快照，避免大数据量时内存暴涨。</summary>
public class StepRecorder
{
    /// <summary>单次录制的最大步数上限。</summary>
    public const int MaxSteps = 20000;

    private readonly List<AnimStep> _trace = new();
    private int _sequence;
    private int _visit;
    private int _recordEvery = 1;

    /// <summary>上一步的数组快照，用于相邻重复内容复用，降低内存。</summary>
    private int[]? _lastIntSnapshot;

    /// <summary>
    /// 取消令牌：切换分类/算法时置为已取消，
    /// 录制点会立即抛出 OperationCanceledException，终止仍在后台运行的旧任务。
    /// </summary>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// 采样间隔：1 表示每步都录制，N 表示每隔 N 次访问录一次。
    /// 由调用方依据数据量与算法复杂度估算后设置。
    /// </summary>
    public int RecordEvery
    {
        get => _recordEvery;
        set => _recordEvery = Math.Max(1, value);
    }

    /// <summary>是否已录满上限。</summary>
    public bool IsFull => _trace.Count >= MaxSteps;

    /// <summary>是否因上限被截断。</summary>
    public bool Truncated { get; private set; }

    /// <summary>
    /// 预测下一次调用 Record 是否会被采纳。
    /// 算法可先调用它，只有返回 true 时才构造快照，避免无谓的数组克隆。
    /// </summary>
    public bool ShouldRecord()
    {
        ThrowIfCancelled();
        if (_trace.Count >= MaxSteps) return false;
        if (_recordEvery <= 1) return true;
        return (_visit + 1) % _recordEvery == 0;
    }

    /// <summary>录制一步（自动应用采样过滤）。</summary>
    public void Record(AnimStep step)
    {
        ThrowIfCancelled();

        if (_trace.Count >= MaxSteps)
        {
            Truncated = true;
            _visit++;
            return;
        }

        _visit++;
        if (_recordEvery > 1 && _visit % _recordEvery != 0) return;

        // 相邻步骤数组内容未变时复用快照
        if (step.Extra is int[] snapshot)
        {
            if (_lastIntSnapshot != null &&
                _lastIntSnapshot.Length == snapshot.Length &&
                _lastIntSnapshot.AsSpan().SequenceEqual(snapshot))
            {
                step.Extra = _lastIntSnapshot;
            }
            else
            {
                _lastIntSnapshot = snapshot;
            }
        }

        step.Sequence = _sequence++;
        _trace.Add(step);
    }

    /// <summary>
    /// 按需构造录制：只有该步会被采纳时才执行工厂方法。
    /// 用于避免为被采样丢弃的步骤克隆大数组。
    /// </summary>
    public void Record(Func<AnimStep> factory)
    {
        if (ShouldRecord())
            Record(factory());
        else
            _visit++;
    }

    private void ThrowIfCancelled()
    {
        if (CancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(CancellationToken);
    }

    public IReadOnlyList<AnimStep> Trace => _trace;

    public void Clear()
    {
        _trace.Clear();
        _sequence = 0;
        _visit = 0;
        _lastIntSnapshot = null;
        Truncated = false;
    }
}
