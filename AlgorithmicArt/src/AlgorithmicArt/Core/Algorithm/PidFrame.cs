namespace AlgorithmicArt.Core.Algorithm;

/// <summary>单步 PID 采样：误差与三项分量。</summary>
public class PidSample
{
    public float Error { get; set; }
    public float P { get; set; }
    public float I { get; set; }
    public float D { get; set; }
    public float Steer { get; set; }
}

/// <summary>PID 帧：轨迹快照 + 至当前步的采样序列。</summary>
public class PidFrame
{
    public PointF[] Trace { get; set; } = Array.Empty<PointF>();
    public PidSample[] Samples { get; set; } = Array.Empty<PidSample>();
}
