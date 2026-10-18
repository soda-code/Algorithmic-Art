using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm;

/// <summary>算法统一接口。</summary>
public interface IAlgorithm
{
    /// <summary>分类：查找 / 排序 / 信号 / 控制。</summary>
    string Category { get; }
    /// <summary>同一分类下的子分组（如「滤波」「汽车」「扫地」）。</summary>
    string Group => "";
    string Name { get; }
    string Description { get; }
    /// <summary>使用的渲染器。</summary>
    RendererKind Renderer => RendererKind.Chart;
    /// <summary>需要的场景类型。</summary>
    SceneKind Scene => SceneKind.RandomArray;
    /// <summary>伪代码（每行一条，行号从 0 起）。</summary>
    IReadOnlyList<string> PseudoCode => Array.Empty<string>();

    /// <summary>估算给定数据量下的步骤数，用于自动计算录制采样间隔（默认线性）。</summary>
    long EstimateSteps(int n) => n;

    void Run(AlgorithmContext ctx, StepRecorder recorder);
}
