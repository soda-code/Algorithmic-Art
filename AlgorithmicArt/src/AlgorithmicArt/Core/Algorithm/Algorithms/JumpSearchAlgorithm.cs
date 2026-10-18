namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>跳跃查找：按 √n 步长跳跃定位区间，再在块内顺序查找。</summary>
public class JumpSearchAlgorithm : IAlgorithm
{
    public string Category => "查找算法";
    public string Group => "查找";
    public string Name => "跳跃查找";
    public string Description => "以 √n 为步长跳跃，先定位可能区间，再块内线性扫描";
    public SceneKind Scene => SceneKind.SortedArray;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "step = ⌊√n⌋, prev = 0, curr = 0",
        "while curr < n and A[curr] < target",
        "  prev = curr; curr += step",
        "块区间 = [prev, min(curr, n-1)]",
        "for i = prev to 块右端",
        "  if A[i] == target: return i",
        "return -1"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var arr = ctx.Array;
        if (arr == null || arr.Length == 0) return;

        int target = (int)ctx.Parameters.GetValueOrDefault("Target", arr[arr.Length / 2]);
        int n = arr.Length;
        int step = Math.Max(1, (int)Math.Sqrt(n));

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 0,
            Description = $"数组长度 {n}，跳跃步长 = ⌊√n⌋ = {step}"
        });

        int prev = 0, curr = 0;

        // 跳跃阶段
        while (curr < n && arr[curr] < target)
        {
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Compare,
                Indices = new[] { curr },
                CodeLine = 1,
                Description = $"跳跃检查 A[{curr}]={arr[curr]} < 目标 {target}，继续跳 {step}"
            });

            prev = curr;
            curr += step;
        }

        int end = Math.Min(curr, n - 1);
        recorder.Record(new AnimStep
        {
            Op = AnimOp.SetRange,
            CodeLine = 3,
            Description = $"目标位于块 [{prev},{end}]，转为块内顺序查找"
        });

        // 块内线性扫描
        for (int i = prev; i <= end && i < n; i++)
        {
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Compare,
                Indices = new[] { i },
                CodeLine = 4,
                Description = $"顺序检查 A[{i}]={arr[i]}"
            });

            if (arr[i] == target)
            {
                recorder.Record(new AnimStep
                {
                    Op = AnimOp.Highlight,
                    Indices = new[] { i },
                    CodeLine = 5,
                    Description = $"找到目标 {target}，索引 {i}"
                });
                return;
            }
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 6,
            Description = $"未找到目标 {target}"
        });
    }
}
