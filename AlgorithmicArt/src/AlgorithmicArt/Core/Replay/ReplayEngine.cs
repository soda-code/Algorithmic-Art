using AlgorithmicArt.Core.Algorithm;

namespace AlgorithmicArt.Core.Replay;

/// <summary>动画回放引擎：消费 AnimStep 列表，UI 定时器驱动，支持前进/后退/定位。</summary>
public class ReplayEngine : IDisposable
{
    private readonly System.Windows.Forms.Timer _timer;
    private readonly SessionStateMachine _state = new();
    private bool _disposed;

    public IReadOnlyList<AnimStep> Steps { get; private set; } = Array.Empty<AnimStep>();
    /// <summary>回放速度 1~100（映射为每步延时）。</summary>
    public int Speed { get; set; } = 85;
    public int CurrentIndex { get; private set; } = -1;
    public int TotalSteps => Steps.Count;
    public SessionState State => _state.State;

    /// <summary>当前步骤变化（null 表示回到初始态）。</summary>
    public event Action<AnimStep?>? StepChanged;
    public event Action? Completed;
    public event Action<SessionState>? StateChanged;

    public ReplayEngine()
    {
        _timer = new System.Windows.Forms.Timer { Interval = 100 };
        _timer.Tick += (_, _) => Advance();
        _state.StateChanged += s => StateChanged?.Invoke(s);
    }

    public void Load(IReadOnlyList<AnimStep> steps)
    {
        Steps = steps;
        CurrentIndex = -1;
        _state.Reset();
    }

    public void Play()
    {
        if (Steps.Count == 0) return;
        if (CurrentIndex >= Steps.Count - 1) CurrentIndex = -1; // 完成态重播
        _timer.Interval = DelayFromSpeed(Speed);
        if (CurrentIndex < 0)
        {
            CurrentIndex = 0;
            EmitStep(); // 立即显示第一步，避免启动延迟
        }
        _timer.Start();
        _state.Play();
    }

    public void Pause()
    {
        _timer.Stop();
        _state.Pause();
    }

    public void StepForward()
    {
        if (Steps.Count == 0) return;
        _timer.Stop();
        _state.Play();
        if (CurrentIndex >= Steps.Count - 1)
        {
            _state.Complete();
            Completed?.Invoke();
            return;
        }
        CurrentIndex++;
        EmitStep();
        _state.Pause();
    }

    public void StepBackward()
    {
        if (Steps.Count == 0) return;
        _timer.Stop();
        if (CurrentIndex > -1) CurrentIndex--;
        EmitStep();
        _state.Pause();
    }

    public void Reset()
    {
        _timer.Stop();
        CurrentIndex = -1;
        _state.Reset();
        EmitStep();
    }

    /// <summary>停止回放并清空步骤：切换页面/分类时彻底关闭旧动画的后台驱动。</summary>
    public void StopAndClear()
    {
        if (_disposed) return;
        _timer.Stop();
        Steps = Array.Empty<AnimStep>();
        CurrentIndex = -1;
        _state.Reset();
        EmitStep();
    }

    public void SeekTo(int index)
    {
        index = Math.Clamp(index, -1, Steps.Count - 1);
        if (index == CurrentIndex) return;
        CurrentIndex = index;
        EmitStep();
    }

    private void Advance()
    {
        if (Steps.Count == 0) return;
        if (CurrentIndex >= Steps.Count - 1)
        {
            _timer.Stop();
            _state.Complete();
            Completed?.Invoke();
            return;
        }
        CurrentIndex++;
        EmitStep();
    }

    private void EmitStep() => StepChanged?.Invoke(CurrentIndex >= 0 ? Steps[CurrentIndex] : null);

    private static int DelayFromSpeed(int speed)
    {
        speed = Math.Clamp(speed, 1, 100);
        return (int)Math.Max(2, 150 - speed * 1.5); // 1→148ms, 50→75ms, 85→22ms, 100→2ms
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
        _timer.Dispose();
    }
}
