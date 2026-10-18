namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>指数查找：先以 2 的幂扩展边界，再在区间内二分。</summary>
public class ExponentialSearchAlgorithm : IAlgorithm
{
    public string Category => "查找算法";
    public string Group => "查找";
    public string Name => "指数查找";
    public string Description => "按 2 的幂快速扩展上界，锁定区间后二分查找";
    public SceneKind Scene => SceneKind.SortedArray;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "if A[0] == target: return 0",
        "bound = 1",
        "while bound < n and A[bound] < target",
        "  bound *= 2",
        "lo = bound/2, hi = min(bound, n-1)",
        "在 [lo, hi] 内二分查找",
        "return -1"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var arr = ctx.Array;
        if (arr == null || arr.Length == 0) return;

        int target = (int)ctx.Parameters.GetValueOrDefault("Target", arr[arr.Length / 2]);
        int n = arr.Length;

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Compare,
            Indices = new[] { 0 },
            CodeLine = 0,
            Description = $"检查首元素 A[0]={arr[0]}"
        });

        if (arr[0] == target)
        {
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Highlight,
                Indices = new[] { 0 },
                CodeLine = 0,
                Description = $"找到目标 {target}，索引 0"
            });
            return;
        }

        int bound = 1;
        while (bound < n && arr[bound] < target)
        {
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Compare,
                Indices = new[] { bound },
                CodeLine = 2,
                Description = $"A[{bound}]={arr[bound]} < 目标 {target}，上界扩展到 {bound * 2}"
            });
            bound *= 2;
        }

        int lo = bound / 2;
        int hi = Math.Min(bound, n - 1);

        recorder.Record(new AnimStep
        {
            Op = AnimOp.SetRange,
            CodeLine = 4,
            Description = $"锁定区间 [{lo},{hi}]，开始二分查找"
        });

        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;

            recorder.Record(new AnimStep
            {
                Op = AnimOp.Compare,
                Indices = new[] { mid },
                CodeLine = 5,
                Description = $"二分：mid={mid}，A[mid]={arr[mid]}"
            });

            if (arr[mid] == target)
            {
                recorder.Record(new AnimStep
                {
                    Op = AnimOp.Highlight,
                    Indices = new[] { mid },
                    CodeLine = 5,
                    Description = $"找到目标 {target}，索引 {mid}"
                });
                return;
            }

            if (arr[mid] < target) lo = mid + 1;
            else hi = mid - 1;
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 6,
            Description = $"未找到目标 {target}"
        });
    }
}
