namespace AlgorithmicArt.Core.Algorithm;

/// <summary>P/PI/PID 对比帧：三条曲线至当前步的采样。</summary>
public class PidCompareFrame
{
    public PidStepSample[] P { get; set; } = Array.Empty<PidStepSample>();
    public PidStepSample[] PI { get; set; } = Array.Empty<PidStepSample>();
    public PidStepSample[] PID { get; set; } = Array.Empty<PidStepSample>();
}
