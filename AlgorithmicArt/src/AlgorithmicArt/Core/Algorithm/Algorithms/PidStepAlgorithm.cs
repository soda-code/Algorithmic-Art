using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>PID 阶跃响应演示（一阶惯性对象）。</summary>
public class PidStepAlgorithm : IAlgorithm
{
    public string Category => "控制算法";
    public string Group => "PID";
    public string Name => "PID 阶跃响应";
    public string Description => "模拟一阶惯性对象，观察超调、调节时间与稳态误差";
    public RendererKind Renderer => RendererKind.PidStep;
    public SceneKind Scene => SceneKind.StepResponse;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "sp = 1.0, pv = 0",
        "for k = 0 to N-1",
        "  e = sp - pv",
        "  P = Kp*e; I += Ki*e*dt; D = Kd*(e-prev)/dt",
        "  u = clamp(P + I + D)",
        "  pv += dt/T * (-pv + K*u)",
        "  record(sp, pv, P, I, D, u)"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        float kp = (float)ctx.Parameters.GetValueOrDefault("Kp", 2.4);
        float ki = (float)ctx.Parameters.GetValueOrDefault("Ki", 0.2);
        float kd = (float)ctx.Parameters.GetValueOrDefault("Kd", 0.3);
        float integralLimit = (float)ctx.Parameters.GetValueOrDefault("IntegralLimit", 5.0);
        float maxU = (float)ctx.Parameters.GetValueOrDefault("MaxSteer", 5.0);

        const int steps = 220;
        const float dt = 0.05f;
        const float K = 1.0f;   // 对象增益
        const float T = 2.0f;   // 时间常数
        const float sp = 1.0f;  // 阶跃目标

        float pv = 0f;
        float integral = 0f, prevError = 0f;
        var samples = new List<PidStepSample>();

        for (int k = 0; k < steps; k++)
        {
            float error = sp - pv;
            float pTerm = kp * error;
            integral = Math.Clamp(integral + ki * error * dt, -integralLimit, integralLimit);
            float dTerm = kd * (error - prevError) / dt;
            prevError = error;

            float u = Math.Clamp(pTerm + integral + dTerm, -maxU, maxU);

            // 一阶惯性对象响应
            pv += dt / T * (-pv + K * u);

            samples.Add(new PidStepSample
            {
                Sp = sp,
                Pv = pv,
                Error = error,
                P = pTerm,
                I = integral,
                D = dTerm,
                U = u
            });

            recorder.Record(new AnimStep
            {
                Op = AnimOp.PlotPoint,
                Points = new[] { new PointF(k, pv) },
                Extra = new PidStepFrame { Samples = samples.ToArray() },
                CodeLine = 5,
                Description = $"k={k}｜SP {sp:0.00} PV {pv:0.00}｜误差 {error:0.00}｜P {pTerm:0.00} I {integral:0.00} D {dTerm:0.00}"
            });
        }
    }
}
