namespace AlgorithmicArt.Communication;

/// <summary>
/// 协议解析器：按「帧头 0xAA 0x55 → 类型 → 长度(2B 小端) → 载荷 → XOR 校验」状态机切分字节流，
/// 自动处理粘包/半包，校验失败丢弃并计数。
/// </summary>
public class ProtocolParser
{
    public const byte Header1 = 0xAA;
    public const byte Header2 = 0x55;

    private readonly List<byte> _buffer = new();

    /// <summary>坏帧计数（校验失败）。</summary>
    public long BadFrameCount { get; private set; }

    public event Action<Frame>? FrameParsed;

    public void Feed(byte[] data) => Feed(data, 0, data.Length);

    public void Feed(byte[] data, int offset, int count)
    {
        for (int i = offset; i < offset + count; i++)
            _buffer.Add(data[i]);
        TryParse();
    }

    public void Reset() => _buffer.Clear();

    private void TryParse()
    {
        while (true)
        {
            int idx = FindHeader();
            if (idx < 0)
            {
                // 无完整帧头：若末尾残留单个 0xAA，保留等待下一字节
                if (_buffer.Count > 0 && _buffer[^1] == Header1)
                    _buffer.RemoveRange(0, _buffer.Count - 1);
                else
                    _buffer.Clear();
                return;
            }
            if (idx > 0) _buffer.RemoveRange(0, idx);

            // 至少需要：AA 55 + Type(1) + Len(2) = 5 字节
            if (_buffer.Count < 5) return;

            byte type = _buffer[2];
            int len = _buffer[3] | (_buffer[4] << 8);

            int total = 5 + len + 1; // + 校验字节
            if (_buffer.Count < total) return;

            byte check = _buffer[total - 1];
            var frame = new Frame
            {
                Type = type,
                Payload = _buffer.GetRange(5, len).ToArray(),
                Timestamp = DateTime.Now
            };

            byte xor = 0;
            xor ^= type;
            xor ^= (byte)len;
            xor ^= (byte)(len >> 8);
            for (int i = 0; i < len; i++) xor ^= frame.Payload[i];
            frame.CheckOk = xor == check;

            if (frame.CheckOk)
                FrameParsed?.Invoke(frame);
            else
                BadFrameCount++;

            _buffer.RemoveRange(0, total);
        }
    }

    private int FindHeader()
    {
        for (int i = 0; i + 1 < _buffer.Count; i++)
        {
            if (_buffer[i] == Header1 && _buffer[i + 1] == Header2)
                return i;
        }
        return -1;
    }

    /// <summary>构造一帧（上位机→下位机）。</summary>
    public static byte[] BuildFrame(byte type, ReadOnlySpan<byte> payload)
    {
        int len = payload.Length;
        var result = new byte[5 + len + 1];
        result[0] = Header1;
        result[1] = Header2;
        result[2] = type;
        result[3] = (byte)len;
        result[4] = (byte)(len >> 8);
        payload.CopyTo(result.AsSpan(5, len));

        byte xor = 0;
        xor ^= type;
        xor ^= (byte)len;
        xor ^= (byte)(len >> 8);
        for (int i = 0; i < len; i++) xor ^= result[5 + i];
        result[5 + len] = xor;
        return result;
    }
}
