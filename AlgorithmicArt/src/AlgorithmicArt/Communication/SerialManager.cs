using System.IO.Ports;

namespace AlgorithmicArt.Communication;

/// <summary>串口管理器：打开/关闭/收发，事件广播。</summary>
public class SerialManager : IDisposable
{
    private readonly ProtocolParser _parser = new();
    private readonly object _sync = new();
    private SerialPort? _port;

    private long _rxBytes;
    private long _txBytes;

    public bool AutoReconnect { get; set; }
    public bool IsOpen => _port?.IsOpen ?? false;
    public SerialConfig? CurrentConfig { get; private set; }

    public long RxBytes => Interlocked.Read(ref _rxBytes);
    public long TxBytes => Interlocked.Read(ref _txBytes);
    public long BadFrameCount => _parser.BadFrameCount;

    /// <summary>原始字节流（后台线程触发）。</summary>
    public event Action<byte[]>? RawDataReceived;
    /// <summary>完整协议帧（后台线程触发）。</summary>
    public event Action<Frame>? FrameReceived;
    /// <summary>连接状态变化（可能后台线程触发）。</summary>
    public event Action<bool>? ConnectionChanged;
    /// <summary>运行日志（可能后台线程触发）。</summary>
    public event Action<string>? MessageLogged;

    public SerialManager()
    {
        _parser.FrameParsed += frame =>
        {
            FrameReceived?.Invoke(frame);
            MessageLogged?.Invoke($"解析帧 Type=0x{frame.Type:X2} Len={frame.Payload.Length}");
        };
    }

    public bool Open(SerialConfig cfg)
    {
        lock (_sync)
        {
            CloseCore();
            var port = new SerialPort(cfg.PortName, cfg.BaudRate, cfg.Parity, cfg.DataBits, cfg.StopBits)
            {
                ReadTimeout = 500,
                WriteTimeout = 500
            };
            try
            {
                port.Open();
            }
            catch (Exception ex)
            {
                MessageLogged?.Invoke($"打开串口失败: {ex.Message}");
                port.Dispose();
                return false;
            }

            _port = port;
            CurrentConfig = cfg;
            _port.DataReceived += OnDataReceived;
            _port.ErrorReceived += OnErrorReceived;
            ConnectionChanged?.Invoke(true);
            MessageLogged?.Invoke($"串口已打开 {cfg.PortName} @ {cfg.BaudRate}");
            return true;
        }
    }

    public void Close() => CloseCore();

    private void CloseCore()
    {
        lock (_sync)
        {
            if (_port == null) return;
            try
            {
                if (_port.IsOpen) _port.Close();
            }
            catch { /* ignore */ }
            _port.DataReceived -= OnDataReceived;
            _port.ErrorReceived -= OnErrorReceived;
            _port.Dispose();
            _port = null;
            ConnectionChanged?.Invoke(false);
            MessageLogged?.Invoke("串口已关闭");
        }
    }

    public void Send(byte[] data)
    {
        lock (_sync)
        {
            if (_port == null || !_port.IsOpen)
            {
                MessageLogged?.Invoke("发送失败：串口未打开");
                return;
            }
            _port.Write(data, 0, data.Length);
            Interlocked.Add(ref _txBytes, data.Length);
        }
    }

    public void SendFrame(FrameType type, byte[] payload) => Send(ProtocolParser.BuildFrame((byte)type, payload));

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        SerialPort? port;
        lock (_sync)
        {
            port = _port;
            if (port == null) return;
        }

        int available = port.BytesToRead;
        if (available <= 0) return;
        var buf = new byte[available];
        int read = port.Read(buf, 0, available);
        if (read <= 0) return;
        if (read < available) Array.Resize(ref buf, read);

        Interlocked.Add(ref _rxBytes, read);
        RawDataReceived?.Invoke(buf);
        _parser.Feed(buf);
    }

    private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {
        MessageLogged?.Invoke($"串口错误: {e.EventType}");
        if (AutoReconnect && CurrentConfig != null)
        {
            var cfg = CurrentConfig;
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                Open(cfg);
            });
        }
    }

    public void Dispose() => CloseCore();
}
