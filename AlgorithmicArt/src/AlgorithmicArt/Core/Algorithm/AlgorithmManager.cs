namespace AlgorithmicArt.Core.Algorithm;

/// <summary>算法管理器：注册 / 查询 / 执行算法，保持注册顺序。</summary>
public class AlgorithmManager
{
    private readonly Dictionary<string, IAlgorithm> _algorithms = new(StringComparer.Ordinal);
    private readonly List<IAlgorithm> _ordered = new();

    public IReadOnlyList<string> Categories { get; private set; } = Array.Empty<string>();

    public void Register(IAlgorithm algorithm)
    {
        var key = BuildKey(algorithm.Category, algorithm.Name);
        if (!_algorithms.ContainsKey(key))
            _ordered.Add(algorithm);
        _algorithms[key] = algorithm;
        RefreshCategories();
    }

    public IAlgorithm? Get(string category, string name) =>
        _algorithms.TryGetValue(BuildKey(category, name), out var algo) ? algo : null;

    public IReadOnlyList<IAlgorithm> GetByCategory(string category) =>
        _ordered.Where(a => a.Category == category).ToList();

    public IReadOnlyList<string> GetNames(string category) =>
        _ordered.Where(a => a.Category == category).Select(a => a.Name).ToList();

    /// <summary>执行算法：清空录制器 → 运行 → 返回步骤。</summary>
    public IReadOnlyList<AnimStep> Run(IAlgorithm algorithm, AlgorithmContext ctx, StepRecorder recorder)
    {
        recorder.Clear();
        algorithm.Run(ctx, recorder);
        return recorder.Trace;
    }

    private static string BuildKey(string category, string name) => $"{category}/{name}";

    private void RefreshCategories() =>
        Categories = _ordered
            .Select(a => a.Category)
            .Distinct(StringComparer.Ordinal)
            .ToList();
}
