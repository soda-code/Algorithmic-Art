namespace AlgorithmicArt.Data;

/// <summary>滤波系数设置。</summary>
public class FilterSettings
{
    /// <summary>滑动平均窗口宽度。</summary>
    public int MovingWindow { get; set; } = 11;
    /// <summary>采样率 Fs（Hz）。</summary>
    public double SampleRate { get; set; } = 1000;
    /// <summary>二阶滤波截止频率 Fc（Hz）。</summary>
    public double CutoffFreq { get; set; } = 50;
    /// <summary>二阶滤波品质因数 Q。</summary>
    public double Quality { get; set; } = 0.707;
    /// <summary>卡尔曼过程噪声 Q。</summary>
    public double KalmanQ { get; set; } = 0.001;
    /// <summary>卡尔曼测量噪声 R。</summary>
    public double KalmanR { get; set; } = 0.02;
}
