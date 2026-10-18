namespace AlgorithmicArt.Models;

/// <summary>设备状态/应答。</summary>
public class DeviceStatus
{
    public DateTime Timestamp { get; set; }
    public byte Type { get; set; }
    public byte[] Payload { get; set; }

    public DeviceStatus(DateTime timestamp, byte type, byte[] payload)
    {
        Timestamp = timestamp;
        Type = type;
        Payload = payload;
    }
}
