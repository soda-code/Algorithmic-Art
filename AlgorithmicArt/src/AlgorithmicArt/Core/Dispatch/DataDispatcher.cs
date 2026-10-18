using AlgorithmicArt.Communication;
using AlgorithmicArt.Models;

namespace AlgorithmicArt.Core.Dispatch;

/// <summary>数据分发器：把协议帧分发为采样/状态事件。</summary>
public class DataDispatcher
{
    public event Action<Sample>? SampleArrived;
    public event Action<DeviceStatus>? StatusArrived;

    public void Dispatch(Frame frame)
    {
        switch ((FrameType)frame.Type)
        {
            case FrameType.Sample:
                SampleArrived?.Invoke(ParseSample(frame));
                break;
            case FrameType.Status:
                StatusArrived?.Invoke(new DeviceStatus(frame.Timestamp, frame.Type, frame.Payload));
                break;
            case FrameType.Config:
            case FrameType.Control:
                // 上行一般不包含此类，忽略
                break;
        }
    }

    /// <summary>采样帧载荷解析：每 2 字节小端 int16 为一个通道值。</summary>
    private static Sample ParseSample(Frame frame)
    {
        var payload = frame.Payload;
        var values = new List<double>();
        for (int i = 0; i + 1 < payload.Length; i += 2)
        {
            short raw = (short)(payload[i] | (payload[i + 1] << 8));
            values.Add(raw);
        }
        if (values.Count == 0 && payload.Length > 0)
        {
            values.Add(payload[0]);
        }
        return new Sample(frame.Timestamp, values.ToArray());
    }
}
