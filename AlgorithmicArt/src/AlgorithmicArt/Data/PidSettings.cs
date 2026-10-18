namespace AlgorithmicArt.Data;

/// <summary>PID 循迹参数设置。</summary>
public class PidSettings
{
    /// <summary>比例系数 Kp。</summary>
    public double Kp { get; set; } = 1.2;
    /// <summary>积分系数 Ki。</summary>
    public double Ki { get; set; } = 0.02;
    /// <summary>微分系数 Kd。</summary>
    public double Kd { get; set; } = 0.3;
    /// <summary>行驶速度（单位/步）。</summary>
    public double Speed { get; set; } = 1.5;
    /// <summary>前视距离。</summary>
    public double LookAhead { get; set; } = 3.0;
    /// <summary>积分限幅。</summary>
    public double IntegralLimit { get; set; } = 5.0;
    /// <summary>最大转向角（弧度）。</summary>
    public double MaxSteer { get; set; } = 0.5;
    /// <summary>最大仿真步数。</summary>
    public int MaxSteps { get; set; } = 600;
    /// <summary>临界增益 Ku（Ziegler-Nichols 用）。</summary>
    public double Ku { get; set; } = 8.0;
    /// <summary>临界周期 Tu（Ziegler-Nichols 用）。</summary>
    public double Tu { get; set; } = 40.0;
    /// <summary>阶跃响应最大控制输出。</summary>
    public double MaxOutput { get; set; } = 5.0;
}
