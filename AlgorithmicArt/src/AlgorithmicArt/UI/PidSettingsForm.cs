using AlgorithmicArt.Data;

namespace AlgorithmicArt.UI;

/// <summary>PID 循迹参数设定对话框（含 Ziegler-Nichols 计算）。</summary>
public class PidSettingsForm : Form
{
    private readonly NumericUpDown _numKp = new();
    private readonly NumericUpDown _numKi = new();
    private readonly NumericUpDown _numKd = new();
    private readonly NumericUpDown _numSpeed = new();
    private readonly NumericUpDown _numLookAhead = new();
    private readonly NumericUpDown _numIntegralLimit = new();
    private readonly NumericUpDown _numMaxSteer = new();
    private readonly NumericUpDown _numMaxSteps = new();
    private readonly NumericUpDown _numKu = new();
    private readonly NumericUpDown _numTu = new();

    public PidSettingsForm(PidSettings settings)
    {
        Text = "PID 循迹参数设定";
        Size = new Size(500, 580);
        MinimumSize = new Size(460, 520);
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
            RowCount = 10,
            Padding = new Padding(20, 18, 20, 8),
            BackColor = Color.White
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));

        Configure(_numKp, 0, 50, (decimal)settings.Kp, 2, 0.1m);
        Configure(_numKi, 0, 50, (decimal)settings.Ki, 3, 0.005m);
        Configure(_numKd, 0, 50, (decimal)settings.Kd, 2, 0.05m);
        Configure(_numSpeed, 0.2m, 10, (decimal)settings.Speed, 1, 0.1m);
        Configure(_numLookAhead, 0.5m, 20, (decimal)settings.LookAhead, 1, 0.5m);
        Configure(_numIntegralLimit, 0.5m, 50, (decimal)settings.IntegralLimit, 1, 0.5m);
        Configure(_numMaxSteer, 0.05m, 1.5m, (decimal)settings.MaxSteer, 2, 0.05m);
        Configure(_numMaxSteps, 50, 5000, settings.MaxSteps, 0, 50);
        Configure(_numKu, 0.1m, 100, (decimal)settings.Ku, 2, 0.1m);
        Configure(_numTu, 0.1m, 1000, (decimal)settings.Tu, 1, 1m);

        AddRow(layout, 0, "比例系数 Kp", _numKp);
        AddRow(layout, 1, "积分系数 Ki", _numKi);
        AddRow(layout, 2, "微分系数 Kd", _numKd);
        AddRow(layout, 3, "行驶速度", _numSpeed);
        AddRow(layout, 4, "前视距离", _numLookAhead);
        AddRow(layout, 5, "积分限幅", _numIntegralLimit);
        AddRow(layout, 6, "最大转向角 (rad)", _numMaxSteer);
        AddRow(layout, 7, "最大步数", _numMaxSteps);
        AddRow(layout, 8, "临界增益 Ku", _numKu);
        AddRow(layout, 9, "临界周期 Tu", _numTu);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 56,
            Padding = new Padding(12, 10, 12, 10),
            BackColor = Color.White
        };

        var btnCancel = new Button { Text = "取消", Width = 84, DialogResult = DialogResult.Cancel };
        var btnOk = new Button { Text = "确定", Width = 84 };
        btnOk.Click += (_, _) =>
        {
            settings.Kp = (double)_numKp.Value;
            settings.Ki = (double)_numKi.Value;
            settings.Kd = (double)_numKd.Value;
            settings.Speed = (double)_numSpeed.Value;
            settings.LookAhead = (double)_numLookAhead.Value;
            settings.IntegralLimit = (double)_numIntegralLimit.Value;
            settings.MaxSteer = (double)_numMaxSteer.Value;
            settings.MaxSteps = (int)_numMaxSteps.Value;
            settings.Ku = (double)_numKu.Value;
            settings.Tu = (double)_numTu.Value;
            DialogResult = DialogResult.OK;
        };

        var btnZn = new Button { Text = "按 Z-N 计算", Width = 116 };
        btnZn.Click += (_, _) => ApplyZieglerNichols();

        buttons.Controls.Add(btnCancel);
        buttons.Controls.Add(btnOk);
        buttons.Controls.Add(btnZn);

        Controls.Add(layout);
        Controls.Add(buttons);
        AcceptButton = btnOk;
        CancelButton = btnCancel;
    }

    /// <summary>Ziegler-Nichols 经典 PID：Kp=0.6Ku, Ti=Tu/2, Td=Tu/8。</summary>
    private void ApplyZieglerNichols()
    {
        double ku = (double)_numKu.Value;
        double tu = (double)_numTu.Value;
        if (ku <= 0 || tu <= 0) return;

        double kp = 0.6 * ku;
        double ti = tu / 2.0;
        double td = tu / 8.0;
        double ki = ti > 1e-6 ? kp / ti : 0;
        double kd = kp * td;

        _numKp.Value = Clamp((decimal)kp, _numKp);
        _numKi.Value = Clamp((decimal)ki, _numKi);
        _numKd.Value = Clamp((decimal)kd, _numKd);
    }

    private static decimal Clamp(decimal v, NumericUpDown n) => Math.Clamp(v, n.Minimum, n.Maximum);

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
