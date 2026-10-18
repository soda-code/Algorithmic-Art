namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>哈希查找。</summary>
public class HashSearchAlgorithm : IAlgorithm
{
    public string Category => "查找算法";
    public string Group => "查找";
    public string Name => "哈希查找";
    public string Description => "建立哈希表后按值直接定位";
    public SceneKind Scene => SceneKind.SortedArray;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "table = {}",
        "for i = 0 to n-1: table[A[i]] = i",
        "if target in table: return table[target]",
        "return -1"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var arr = ctx.Array;
        if (arr == null || arr.Length == 0) return;

        int target = (int)ctx.Parameters.GetValueOrDefault("Target", arr[arr.Length / 2]);
        var table = new Dictionary<int, int>();

        for (int i = 0; i < arr.Length; i++)
        {
            if (!table.ContainsKey(arr[i])) table[arr[i]] = i;
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Visit,
                Indices = new[] { i },
                CodeLine = 1,
                Description = $"A[{i}]={arr[i]} 写入哈希表"
            });
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Compare,
            CodeLine = 2,
            Description = $"哈希查找目标 {target}"
        });

        if (table.TryGetValue(target, out var idx))
        {
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Highlight,
                Indices = new[] { idx },
                CodeLine = 2,
                Description = $"哈希命中，索引 {idx}"
            });
        }
        else
        {
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Highlight,
                CodeLine = 3,
                Description = "哈希表中未找到目标"
            });
        }
    }
}
