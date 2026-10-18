namespace AlgorithmicArt.Core.Algorithm;

/// <summary>PID 阶跃响应仿真（一阶惯性对象）。</summary>
public static class PidSimulator
{
    public static List<PidStepSample> Simulate(
        double kp, double ki, double kd,
        double integralLimit = 5.0, double maxU = 5.0,
        int steps = 220, double dt = 0.05,
        double plantGain = 1.0, double plantTau = 2.0, double sp = 1.0)
    {
        var samples = new List<PidStepSample>(steps);
        double pv = 0, integral = 0, prevError = 0;
        double td = kp > 1e-6 ? kd / kp : 0;
        double tf = Math.Max(0.02, td / 10.0);
        double dFilt = 0;

        for (int k = 0; k < steps; k++)
        {
            double error = sp - pv;

            // 微分一阶滤波（Tf ≈ Td/10）
            double dRaw = error - prevError;
            dFilt += dt / (tf + dt) * (dRaw - dFilt);
            prevError = error;

            double pTerm = kp * error;
            double iTerm = integral;
            double dTerm = kd * dFilt;

            double uUnsat = pTerm + iTerm + dTerm;
            double u = Math.Clamp(uUnsat, -maxU, maxU);

            // 条件积分抗饱和
            bool saturated = Math.Abs(uUnsat) >= maxU;
            if (!saturated || (uUnsat > 0 && error < 0) || (uUnsat < 0 && error > 0))
                integral = Math.Clamp(integral + ki * error * dt, -integralLimit, integralLimit);

            // 一阶惯性对象
            pv += dt / plantTau * (-pv + plantGain * u);

            samples.Add(new PidStepSample
            {
                Sp = (float)sp,
                Pv = (float)pv,
                Error = (float)error,
                P = (float)pTerm,
                I = (float)integral,
                D = (float)dTerm,
                U = (float)u
            });
        }

        return samples;
    }

    /// <summary>响应指标：超调、调节时间、稳态误差、上升时间。</summary>
    public static (float Overshoot, float SettleTime, float SteadyError, float RiseTime) Metrics(
        IReadOnlyList<PidStepSample> samples, float dt = 0.05f)
    {
        if (samples.Count < 2) return (0, 0, 0, 0);

        float sp = samples[0].Sp;
        float pvMax = samples.Max(s => s.Pv);
        float overshoot = sp > 1e-6f ? Math.Max(0f, (pvMax - sp) / sp * 100f) : 0f;
        float ess = Math.Abs(sp - samples[^1].Pv);

        float tol = sp * 0.05f;
        int settleIdx = 0;
        for (int i = samples.Count - 1; i >= 0; i--)
        {
            if (Math.Abs(samples[i].Pv - sp) > tol)
            {
                settleIdx = i + 1;
                break;
            }
        }
        float ts = settleIdx * dt;

        float t10 = -1, t90 = -1;
        for (int i = 0; i < samples.Count; i++)
        {
            if (t10 < 0 && samples[i].Pv >= sp * 0.1f) t10 = i * dt;
            if (t90 < 0 && samples[i].Pv >= sp * 0.9f) { t90 = i * dt; break; }
        }
        float tr = t10 >= 0 && t90 >= 0 ? Math.Max(0f, t90 - t10) : 0f;

        return (overshoot, ts, ess, tr);
    }
}
