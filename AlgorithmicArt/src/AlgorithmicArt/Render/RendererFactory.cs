namespace AlgorithmicArt.Render;

/// <summary>渲染器工厂：按渲染器类型创建实例。</summary>
public static class RendererFactory
{
    public static IAnimRenderer? Create(RendererKind kind) => kind switch
    {
        RendererKind.Waveform => new WaveformRenderer(),
        RendererKind.Spectrum => new SpectrumRenderer(),
        RendererKind.Chart => new ChartRenderer(),
        RendererKind.GridMap => new GridMapRenderer(),
        RendererKind.Trajectory => new TrajectoryRenderer(),
        RendererKind.Pid => new PidRenderer(),
        RendererKind.PidStep => new PidStepRenderer(),
        RendererKind.PidCompare => new PidCompareRenderer(),
        RendererKind.Scatter => new ScatterRenderer(),
        RendererKind.Matrix => new MatrixRenderer(),
        RendererKind.Vector => new VectorRenderer(),
        _ => null
    };
}
