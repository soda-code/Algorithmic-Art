using AlgorithmicArt.Data;

namespace AlgorithmicArt.UI;

/// <summary>滤波系数设定对话框。</summary>
public class FilterSettingsForm : Form
{
    private readonly NumericUpDown _numWindow = new();
    private readonly NumericUpDown _numFs = new();
    private readonly NumericUpDown _numFc = new();
    private readonly NumericUpDown _numQ = new();
    private readonly NumericUpDown _numKalmanQ = new();
    private readonly NumericUpDown _numKalmanR = new();

    public FilterSettingsForm(FilterSettings settings)
    {
        Text = "滤波系数设定";
        Size = new Size(460, 400);
        MinimumSize = new Size(420, 360);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;
        Font = Theme.BaseFont;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(20, 18, 20, 8),
            BackColor = Color.White
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

        Configure(_numWindow, 1, 101, settings.MovingWindow, 0, 2);
        Configure(_numFs, 1, 100000, (decimal)settings.SampleRate, 0, 100);
        Configure(_numFc, 1, 10000, (decimal)settings.CutoffFreq, 1, 1);
        Configure(_numQ, 0.05m, 10, (decimal)settings.Quality, 3, 0.05m);
        Configure(_numKalmanQ, 0.0001m, 1, (decimal)settings.KalmanQ, 4, 0.0001m);
        Configure(_numKalmanR, 0.0001m, 10, (decimal)settings.KalmanR, 4, 0.001m);

        AddRow(layout, 0, "滑动平均窗口 W", _numWindow);
        AddRow(layout, 1, "采样率 Fs (Hz)", _numFs);
        AddRow(layout, 2, "二阶滤波截止频率 Fc (Hz)", _numFc);
        AddRow(layout, 3, "二阶滤波品质因数 Q", _numQ);
        AddRow(layout, 4, "卡尔曼过程噪声 Q", _numKalmanQ);
        AddRow(layout, 5, "卡尔曼测量噪声 R", _numKalmanR);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 54,
            Padding = new Padding(12, 10, 12, 10),
            BackColor = Color.White
        };

        var btnCancel = new Button { Text = "取消", Width = 84, DialogResult = DialogResult.Cancel };
        var btnOk = new Button { Text = "确定", Width = 84 };
        btnOk.Click += (_, _) =>
        {
            settings.MovingWindow = (int)_numWindow.Value;
            settings.SampleRate = (double)_numFs.Value;
            settings.CutoffFreq = (double)_numFc.Value;
            settings.Quality = (double)_numQ.Value;
            settings.KalmanQ = (double)_numKalmanQ.Value;
            settings.KalmanR = (double)_numKalmanR.Value;
            DialogResult = DialogResult.OK;
        };

        buttons.Controls.Add(btnCancel);
        buttons.Controls.Add(btnOk);

        Controls.Add(layout);
        Controls.Add(buttons);
        AcceptButton = btnOk;
        CancelButton = btnCancel;
    }

    private static void Configure(NumericUpDown n, decimal min, decimal max, decimal value, int decimals, decimal increment)
    {
        n.Minimum = min;
        n.Maximum = max;
        n.DecimalPlaces = decimals;
        n.Increment = increment;
        n.Value = Math.Clamp(value, min, max);
        n.Width = 150;
    }

    private static void AddRow(TableLayoutPanel t, int row, string label, Control input)
    {
        t.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            ForeColor = Theme.TextPrimary
        }, 0, row);
        t.Controls.Add(input, 1, row);
    }
}
