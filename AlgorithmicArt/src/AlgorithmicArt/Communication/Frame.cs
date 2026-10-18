namespace AlgorithmicArt.Communication;

/// <summary>协议帧。</summary>
public class Frame
{
    public byte Type { get; set; }
    public byte[] Payload { get; set; } = Array.Empty<byte>();
    public DateTime Timestamp { get; set; }
    /// <summary>XOR 校验是否通过。</summary>
    public bool CheckOk { get; set; }
}
