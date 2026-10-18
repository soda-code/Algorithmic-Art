namespace AlgorithmicArt.Core.Algorithm;

/// <summary>阶跃响应单步采样。</summary>
public class PidStepSample
{
    /// <summary>设定值 SP。</summary>
    public float Sp { get; set; }
    /// <summary>过程值 PV（响应）。</summary>
    public float Pv { get; set; }
    /// <summary>误差 e = SP - PV。</summary>
    public float Error { get; set; }
    /// <summary>比例分量。</summary>
    public float P { get; set; }
    /// <summary>积分分量。</summary>
    public float I { get; set; }
    /// <summary>微分分量。</summary>
    public float D { get; set; }
    /// <summary>控制输出 u。</summary>
    public float U { get; set; }
}

/// <summary>阶跃响应帧：至当前步的采样序列。</summary>
public class PidStepFrame
{
    public PidStepSample[] Samples { get; set; } = Array.Empty<PidStepSample>();
}
