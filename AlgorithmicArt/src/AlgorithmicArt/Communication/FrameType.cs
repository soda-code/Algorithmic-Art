namespace AlgorithmicArt.Communication;

/// <summary>协议帧类型。</summary>
public enum FrameType : byte
{
    /// <summary>下位机→上位机：采样数据帧。</summary>
    Sample = 0x01,
    /// <summary>下位机→上位机：状态/应答帧。</summary>
    Status = 0x02,
    /// <summary>上位机→下位机：参数配置帧。</summary>
    Config = 0x03,
    /// <summary>上位机→下位机：控制指令帧。</summary>
    Control = 0x04
}
