using System.IO.Ports;

namespace AlgorithmicArt.Communication;

/// <summary>串口参数。</summary>
public class SerialConfig
{
    public string PortName { get; set; } = "COM1";
    public int BaudRate { get; set; } = 115200;
    public int DataBits { get; set; } = 8;
    public Parity Parity { get; set; } = Parity.None;
    public StopBits StopBits { get; set; } = StopBits.One;
}
