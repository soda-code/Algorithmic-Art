namespace AlgorithmicArt.Core.Replay;

/// <summary>动画会话状态机。</summary>
public class SessionStateMachine
{
    public SessionState State { get; private set; } = SessionState.Idle;

    public event Action<SessionState>? StateChanged;

    public void Play() => Transition(SessionState.Running);

    public void Pause()
    {
        if (State == SessionState.Running)
            Transition(SessionState.Paused);
    }

    public void Complete()
    {
        if (State == SessionState.Running)
            Transition(SessionState.Completed);
    }

    public void Reset() => Transition(SessionState.Idle);

    private void Transition(SessionState next)
    {
        if (State == next) return;
        State = next;
        StateChanged?.Invoke(State);
    }
}
