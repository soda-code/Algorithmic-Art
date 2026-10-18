using System.IO.Ports;
using System.Text;
using AlgorithmicArt.Communication;
using AlgorithmicArt.Core.Dispatch;
using AlgorithmicArt.Data;
using AlgorithmicArt.UI;
using AlgorithmicArt.UI.Controls;

namespace AlgorithmicArt.UI.Pages;

/// <summary>串口上位机页面：串口设置 / 接收显示 / 发送指令 / 实时曲线 / 日志。</summary>
public class SerialPage : UserControl
{
    private readonly SerialManager _serial;
    private readonly DataDispatcher _dispatcher;
    private readonly SimpleLogger _logger;
    private readonly AppConfig _config;

    // 子面板
    private readonly Panel _setupPanel = new();
    private readonly Panel _receivePanel = new();
    private readonly Panel _sendPanel = new();
    private readonly Panel _curvePanel = new();
    private readonly Panel _logPanel = new();

    // 串口设置
    private readonly RoundedDropDown _cbPort = new();
    private readonly RoundedDropDown _cbBaud = new();
    private readonly RoundedDropDown _cbDataBits = new();
    private readonly RoundedDropDown _cbParity = new();
    private readonly RoundedDropDown _cbStopBits = new();
    private readonly RoundedButton _btnOpen = new();
    private readonly CheckBox _chkAutoReconnect = new();
    private readonly Label _lblStatus = new();

    // 接收显示
    private readonly RoundedDropDown _cbReceiveFormat = new();
    private readonly RoundedButton _btnReceivePause = new();
    private readonly RoundedButton _btnReceiveClear = new();
    private readonly Label _lblRxTx = new();
    private readonly RichTextBox _rtbReceive = new();
    private bool _receivePaused;

    // 发送
    private readonly TextBox _txtSend = new();
    private readonly RoundedDropDown _cbSendFormat = new();
    private readonly RoundedButton _btnSend = new();
    private readonly CheckBox _chkLoopSend = new();
    private readonly System.Windows.Forms.Timer _loopTimer = new();

    // 曲线
    private readonly ChartPanel _chart = new();

    // 日志
    private readonly RichTextBox _rtbLog = new();
    private readonly RoundedButton _btnLogClear = new();

    private static readonly (string Text, byte Cmd)[] QuickCommands =
    {
        ("启动", 0x01), ("停止", 0x02), ("复位", 0x03), ("查询", 0x04)
    };

    public SerialPage(SerialManager serial, DataDispatcher dispatcher, SimpleLogger logger, AppConfig config)
    {
        _serial = serial;
        _dispatcher = dispatcher;
        _logger = logger;
        _config = config;

        BuildSetupPanel();
        BuildReceivePanel();
        BuildSendPanel();
        BuildCurvePanel();
        BuildLogPanel();

        var panels = new[] { _setupPanel, _receivePanel, _sendPanel, _curvePanel, _logPanel };
        foreach (var p in panels)
        {
            p.Dock = DockStyle.Fill;
            p.BackColor = Theme.CardBack;
            Controls.Add(p);
        }

        WireEvents();
        ApplyConfig();
        ShowSection("setup");
    }

    public void ShowSection(string key)
    {
        _setupPanel.Visible = key == "setup";
        _receivePanel.Visible = key == "receive";
        _sendPanel.Visible = key == "send";
        _curvePanel.Visible = key == "curve";
        _logPanel.Visible = key == "log";
    }

    // ---------- 构建 ----------

    private void BuildSetupPanel()
    {
        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(12),
            AutoScroll = true,
            BackColor = Theme.CardBack
        };

        flow.Controls.Add(MakeRow(MakeLabel("端口"), _cbPort, MakeLabel("波特率"), _cbBaud, MakeLabel("数据位"), _cbDataBits));
        flow.Controls.Add(MakeRow(MakeLabel("校验"), _cbParity, MakeLabel("停止位"), _cbStopBits));

        var row2 = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Theme.CardBack
        };
        _btnOpen.Text = "打开串口";
        _btnOpen.Size = new Size(110, 30);
        _btnOpen.ApplyPrimary();
        _chkAutoReconnect.Text = "自动重连";
        _chkAutoReconnect.AutoSize = true;
        row2.Controls.Add(_btnOpen);
        row2.Controls.Add(_chkAutoReconnect);
        flow.Controls.Add(row2);

        _lblStatus.Text = "状态：未连接";
        _lblStatus.AutoSize = true;
        _lblStatus.ForeColor = Color.Gray;
        flow.Controls.Add(_lblStatus);

        _setupPanel.Controls.Add(flow);
    }

    private FlowLayoutPanel MakeRow(params Control[] controls)
    {
        var row = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 4),
            BackColor = Theme.CardBack
        };
        row.Controls.AddRange(controls);
        return row;
    }

    private void BuildReceivePanel()
    {
        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(8, 6, 8, 6),
            BackColor = Theme.CardBack
        };
        top.Controls.Add(MakeLabel("格式"));
        _cbReceiveFormat.Items.AddRange(new object[] { "十六进制", "文本" });
        _cbReceiveFormat.SelectedIndex = 0;
        _cbReceiveFormat.Width = 80;
        top.Controls.Add(_cbReceiveFormat);

        _btnReceivePause.Text = "暂停";
        _btnReceivePause.Width = 70;
        _btnReceivePause.ApplySecondary();
        _btnReceiveClear.Text = "清空";
        _btnReceiveClear.Width = 70;
        _btnReceiveClear.ApplySecondary();
        top.Controls.Add(_btnReceivePause);
        top.Controls.Add(_btnReceiveClear);

        _lblRxTx.Text = "接收:0  发送:0";
        _lblRxTx.AutoSize = true;
        _lblRxTx.Margin = new Padding(12, 6, 0, 0);
        top.Controls.Add(_lblRxTx);

        _rtbReceive.ReadOnly = true;
        _rtbReceive.BorderStyle = BorderStyle.None;
        _rtbReceive.BackColor = Color.White;
        _rtbReceive.Font = new Font("Consolas", 9f);
        _rtbReceive.Dock = DockStyle.Fill;

        _receivePanel.Controls.Add(_rtbReceive);
        _receivePanel.Controls.Add(top);
    }

    private void BuildSendPanel()
    {
        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(8, 6, 8, 6),
            BackColor = Theme.CardBack
        };
        top.Controls.Add(MakeLabel("格式"));
        _cbSendFormat.Items.AddRange(new object[] { "十六进制", "文本" });
        _cbSendFormat.SelectedIndex = 0;
        _cbSendFormat.Width = 80;
        top.Controls.Add(_cbSendFormat);

        _btnSend.Text = "发送";
        _btnSend.Width = 80;
        _btnSend.ApplyPrimary();
        _chkLoopSend.Text = "循环发送(500ms)";
        _chkLoopSend.AutoSize = true;
        _chkLoopSend.Margin = new Padding(12, 6, 0, 0);
        top.Controls.Add(_btnSend);
        top.Controls.Add(_chkLoopSend);

        _txtSend.Multiline = true;
        _txtSend.BorderStyle = BorderStyle.None;
        _txtSend.Dock = DockStyle.Top;
        _txtSend.Height = 90;
        _txtSend.Font = new Font("Consolas", 9f);
        _txtSend.Text = "AA 55 04 01 00 01";

        var quick = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(8, 6, 8, 6),
            BackColor = Theme.CardBack
        };
        quick.Controls.Add(MakeLabel("快捷指令"));
        foreach (var (text, _) in QuickCommands)
        {
            var b = new RoundedButton { Text = text, Width = 64 };
            b.ApplySecondary();
            b.Click += (_, _) => SendQuickCommand(b.Text);
            quick.Controls.Add(b);
        }

        _sendPanel.Controls.Add(quick);
        _sendPanel.Controls.Add(_txtSend);
        _sendPanel.Controls.Add(top);
    }

    private void BuildCurvePanel()
    {
        _chart.Dock = DockStyle.Fill;
        _curvePanel.Controls.Add(_chart);
    }

    private void BuildLogPanel()
    {
        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(8, 6, 8, 6),
            BackColor = Theme.CardBack
        };
        _btnLogClear.Text = "清空";
        _btnLogClear.Width = 80;
        _btnLogClear.ApplySecondary();
        top.Controls.Add(_btnLogClear);

        _rtbLog.ReadOnly = true;
        _rtbLog.BorderStyle = BorderStyle.None;
        _rtbLog.BackColor = Color.White;
        _rtbLog.Font = new Font("Consolas", 9f);
        _rtbLog.Dock = DockStyle.Fill;

        _logPanel.Controls.Add(_rtbLog);
        _logPanel.Controls.Add(top);
    }

    private static Label MakeLabel(string text)
    {
        return new Label { Text = text, AutoSize = true, Margin = new Padding(0, 6, 4, 0) };
    }

    private static string LevelText(LogLevel level) => level switch
    {
        LogLevel.Info => "信息",
        LogLevel.Warn => "警告",
        LogLevel.Error => "错误",
        _ => "信息"
    };

    // ---------- 事件 ----------

    private void WireEvents()
    {
        _btnOpen.Click += (_, _) => ToggleOpen();
        _btnReceiveClear.Click += (_, _) => _rtbReceive.Clear();
        _btnReceivePause.Click += (_, _) =>
        {
            _receivePaused = !_receivePaused;
            _btnReceivePause.Text = _receivePaused ? "继续" : "暂停";
        };
        _btnLogClear.Click += (_, _) => _rtbLog.Clear();
        _btnSend.Click += (_, _) => DoSend();
        _loopTimer.Interval = 500;
        _loopTimer.Tick += (_, _) => DoSend();
        _chkLoopSend.CheckedChanged += (_, _) =>
        {
            if (_chkLoopSend.Checked) _loopTimer.Start(); else _loopTimer.Stop();
        };

        _serial.RawDataReceived += data => RunOnUi(() => OnRawData(data));
        _serial.FrameReceived += frame => RunOnUi(() =>
        {
            _dispatcher.Dispatch(frame);
            AppendReceive($"[帧 Type=0x{frame.Type:X2} Len={frame.Payload.Length} {(frame.CheckOk ? "OK" : "BAD")}]");
        });
        _serial.ConnectionChanged += open => RunOnUi(() => OnConnectionChanged(open));
        _serial.MessageLogged += msg => RunOnUi(() => AppendLog(msg));
        _logger.EntryAdded += entry => RunOnUi(() =>
            AppendLog($"[{entry.Time:HH:mm:ss}] [{LevelText(entry.Level)}] {entry.Message}"));
        _dispatcher.SampleArrived += sample => RunOnUi(() => _chart.AddSample(sample.Values));
    }

    private void ApplyConfig()
    {
        _cbBaud.Items.AddRange(new object[] { "9600", "19200", "38400", "57600", "115200", "230400", "460800", "921600" });
        _cbDataBits.Items.AddRange(new object[] { "8", "7" });
        _cbParity.Items.AddRange(new object[] { "无", "奇", "偶" });
        _cbStopBits.Items.AddRange(new object[] { "1", "1.5", "2" });

        _cbPort.ClearItems();
        try
        {
            foreach (var name in SerialPort.GetPortNames())
                _cbPort.Items.Add(name);
        }
        catch { /* ignore */ }

        _cbPort.Text = _config.Serial.PortName;
        _cbBaud.Text = _config.Serial.BaudRate.ToString();
        _cbDataBits.Text = _config.Serial.DataBits.ToString();
        _cbParity.Text = ParityToText(_config.Serial.Parity);
        _cbStopBits.Text = StopBitsToText(_config.Serial.StopBits);
        _chkAutoReconnect.Checked = _config.AutoReconnect;
        _serial.AutoReconnect = _config.AutoReconnect;
    }

    // ---------- 行为 ----------

    private void ToggleOpen()
    {
        if (_serial.IsOpen)
        {
            _serial.Close();
            return;
        }
        var cfg = GetSerialConfig();
        _serial.Open(cfg);
    }

    public SerialConfig GetSerialConfig()
    {
        return new SerialConfig
        {
            PortName = _cbPort.Text.Trim(),
            BaudRate = int.TryParse(_cbBaud.Text, out var baud) ? baud : 115200,
            DataBits = int.TryParse(_cbDataBits.Text, out var db) ? db : 8,
            Parity = TextToParity(_cbParity.Text),
            StopBits = TextToStopBits(_cbStopBits.Text)
        };
    }

    public bool AutoReconnect => _chkAutoReconnect.Checked;

    private void DoSend()
    {
        byte[]? data;
        if (_cbSendFormat.SelectedIndex == 0) // HEX
        {
            data = ParseHex(_txtSend.Text);
            if (data == null)
            {
                AppendLog("[错误] 十六进制指令格式错误");
                return;
            }
        }
        else
        {
            data = Encoding.ASCII.GetBytes(_txtSend.Text);
        }
        _serial.Send(data);
    }

    private void SendQuickCommand(string text)
    {
        foreach (var (t, cmd) in QuickCommands)
        {
            if (t == text)
            {
                _serial.SendFrame(FrameType.Control, new byte[] { cmd });
                return;
            }
        }
    }

    private void OnRawData(byte[] data)
    {
        if (_receivePaused) return;
        _lblRxTx.Text = $"接收:{_serial.RxBytes}  发送:{_serial.TxBytes}";
        AppendReceive(FormatBytes(data));
    }

    private void OnConnectionChanged(bool open)
    {
        _btnOpen.Text = open ? "关闭串口" : "打开串口";
        _lblStatus.Text = open ? $"状态：已连接 {_serial.CurrentConfig?.PortName}" : "状态：未连接";
        _lblStatus.ForeColor = open ? Color.Green : Color.Gray;
    }

    private string FormatBytes(byte[] data)
    {
        if (_cbReceiveFormat.SelectedIndex == 0) // HEX
        {
            var sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("X2")).Append(' ');
                if ((i + 1) % 16 == 0) sb.AppendLine();
            }
            return sb.ToString();
        }
        else
        {
            var sb = new StringBuilder();
            foreach (var b in data)
                sb.Append(b >= 32 && b < 127 ? (char)b : '.');
            return sb.ToString();
        }
    }

    private void AppendReceive(string text)
    {
        if (_rtbReceive.TextLength > 100_000) _rtbReceive.Clear();
        _rtbReceive.AppendText(text + Environment.NewLine);
        _rtbReceive.SelectionStart = _rtbReceive.TextLength;
        _rtbReceive.ScrollToCaret();
    }

    private void AppendLog(string text)
    {
        if (_rtbLog.TextLength > 50_000) _rtbLog.Clear();
        _rtbLog.AppendText(text + Environment.NewLine);
        _rtbLog.SelectionStart = _rtbLog.TextLength;
        _rtbLog.ScrollToCaret();
    }

    private void RunOnUi(Action action)
    {
        if (IsDisposed || Disposing) return;
        if (InvokeRequired)
        {
            try { BeginInvoke(action); } catch { /* ignore */ }
        }
        else
        {
            action();
        }
    }

    private static byte[]? ParseHex(string text)
    {
        var clean = text.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace(",", "");
        if (clean.Length % 2 != 0) return null;
        var result = new byte[clean.Length / 2];
        for (int i = 0; i < result.Length; i++)
        {
            if (!byte.TryParse(clean.AsSpan(i * 2, 2),
                    System.Globalization.NumberStyles.HexNumber, null, out result[i]))
                return null;
        }
        return result;
    }

    private static string ParityToText(Parity p) => p switch
    {
        Parity.None => "无",
        Parity.Odd => "奇",
        Parity.Even => "偶",
        _ => "无"
    };

    private static Parity TextToParity(string t) => t switch
    {
        "奇" => Parity.Odd,
        "偶" => Parity.Even,
        _ => Parity.None
    };

    private static string StopBitsToText(StopBits s) => s switch
    {
        StopBits.One => "1",
        StopBits.OnePointFive => "1.5",
        StopBits.Two => "2",
        _ => "1"
    };

    private static StopBits TextToStopBits(string t) => t switch
    {
        "1.5" => StopBits.OnePointFive,
        "2" => StopBits.Two,
        _ => StopBits.One
    };
}
