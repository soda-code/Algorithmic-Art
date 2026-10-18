namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>插值查找（要求有序且分布较均匀）。</summary>
public class InterpolationSearchAlgorithm : IAlgorithm
{
    public string Category => "查找算法";
    public string Group => "查找";
    public string Name => "插值查找";
    public string Description => "按目标值比例估算位置，适合均匀分布的有序数组";
    public SceneKind Scene => SceneKind.SortedArray;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "lo = 0, hi = n-1",
        "while lo <= hi and target in [A[lo], A[hi]]",
        "  mid = lo + (target-A[lo])/(A[hi]-A[lo])*(hi-lo)",
        "  if A[mid] == target: return mid",
        "  if A[mid] < target: lo = mid+1",
        "  else: hi = mid-1",
        "return -1"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var arr = ctx.Array;
        if (arr == null || arr.Length == 0) return;

        int target = (int)ctx.Parameters.GetValueOrDefault("Target", arr[arr.Length / 2]);
        int lo = 0, hi = arr.Length - 1;

        while (lo <= hi && target >= arr[lo] && target <= arr[hi])
        {
            int mid = arr[hi] == arr[lo]
                ? lo
                : lo + (int)((double)(target - arr[lo]) / (arr[hi] - arr[lo]) * (hi - lo));
            mid = Math.Clamp(mid, lo, hi);

            recorder.Record(new AnimStep
            {
                Op = AnimOp.Compare,
                Indices = new[] { mid },
                CodeLine = 2,
                Description = $"估算 mid={mid}，A[mid]={arr[mid]}，目标 {target}"
            });

            if (arr[mid] == target)
            {
                recorder.Record(new AnimStep
                {
                    Op = AnimOp.Highlight,
                    Indices = new[] { mid },
                    CodeLine = 3,
                    Description = $"找到目标，索引 {mid}"
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
