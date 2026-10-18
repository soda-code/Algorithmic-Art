namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>二分查找（要求有序数组）。</summary>
public class BinarySearchAlgorithm : IAlgorithm
{
    public string Category => "查找算法";
    public string Group => "查找";
    public string Name => "二分查找";
    public string Description => "在有序数组中折半查找目标";
    public SceneKind Scene => SceneKind.SortedArray;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "lo = 0, hi = n-1",
        "while lo <= hi",
        "  mid = (lo + hi) / 2",
        "  if A[mid] == target: return mid",
        "  if A[mid] < target: lo = mid + 1",
        "  else: hi = mid - 1",
        "return -1"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var arr = ctx.Array;
        if (arr == null || arr.Length == 0) return;

        int target = (int)ctx.Parameters.GetValueOrDefault("Target", arr[arr.Length / 2]);
        int lo = 0, hi = arr.Length - 1;

        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Compare,
                Indices = new[] { mid },
                CodeLine = 2,
                Description = $"mid={mid}，A[mid]={arr[mid]}，目标 {target}"
            });

            if (arr[mid] == target)
            {
                recorder.Record(new AnimStep
                {
                    Op = AnimOp.Highlight,
                    Indices = new[] { mid },
                    CodeLine = 3,
                    Description = $"找到目标 {target}，索引 {mid}"
                });
                return;
            }

            if (arr[mid] < target)
            {
                lo = mid + 1;
                recorder.Record(new AnimStep
                {
                    Op = AnimOp.SetRange,
                    CodeLine = 5,
                    Description = $"A[mid] < 目标，搜索右半区 [{lo},{hi}]"
                });
            }
            else
            {
                hi = mid - 1;
                recorder.Record(new AnimStep
                {
                    Op = AnimOp.SetRange,
                    CodeLine = 6,
                    Description = $"A[mid] > 目标，搜索左半区 [{lo},{hi}]"
                });
            }
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 7,
            Description = $"未找到目标 {target}"
        });
    }
}
