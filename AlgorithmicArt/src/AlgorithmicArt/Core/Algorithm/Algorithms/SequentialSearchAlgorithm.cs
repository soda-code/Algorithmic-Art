namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>顺序查找。</summary>
public class SequentialSearchAlgorithm : IAlgorithm
{
    public string Category => "查找算法";
    public string Group => "查找";
    public string Name => "顺序查找";
    public string Description => "从头到尾逐个比较，直到找到目标";
    public SceneKind Scene => SceneKind.SortedArray;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "for i = 0 to n-1",
        "  if A[i] == target",
        "    return i",
        "return -1"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var arr = ctx.Array;
        if (arr == null || arr.Length == 0) return;

        int target = (int)ctx.Parameters.GetValueOrDefault("Target", arr[arr.Length / 2]);

        for (int i = 0; i < arr.Length; i++)
        {
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Compare,
                Indices = new[] { i },
                CodeLine = 1,
                Description = $"比较 A[{i}]={arr[i]} 与目标 {target}"
            });

            if (arr[i] == target)
            {
                recorder.Record(new AnimStep
                {
                    Op = AnimOp.Highlight,
                    Indices = new[] { i },
                    CodeLine = 2,
                    Description = $"找到目标 {target}，索引 {i}"
                });
                return;
            }
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 3,
            Description = $"未找到目标 {target}"
        });
    }
}
