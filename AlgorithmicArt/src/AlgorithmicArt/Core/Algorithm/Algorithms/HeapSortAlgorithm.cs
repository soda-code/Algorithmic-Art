namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>堆排序。</summary>
public class HeapSortAlgorithm : IAlgorithm
{
    public string Category => "排序算法";
    public string Group => "排序";
    public string Name => "堆排序";
    public string Description => "建大顶堆后逐个取出堆顶放到末尾";

    public long EstimateSteps(int n) => (long)(n * Math.Log2(Math.Max(n, 2)) * 4);

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "build max-heap",
        "for i = n-1 downto 1",
        "  swap(A[0], A[i])",
        "  heapify(A, i, 0)"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var arr = ctx.Array;
        if (arr == null || arr.Length < 2) return;

        var work = (int[])arr.Clone();
        int n = work.Length;

        for (int i = n / 2 - 1; i >= 0; i--)
            Heapify(work, n, i, recorder);

        for (int i = n - 1; i > 0; i--)
        {
            (work[0], work[i]) = (work[i], work[0]);
            recorder.Record(() => new AnimStep
            {
                Op = AnimOp.Swap,
                Indices = new[] { 0, i },
                Extra = (int[])work.Clone(),
                SortedLow = i,
                SortedHigh = n - 1,
                CodeLine = 2,
                Description = $"堆顶与 A[{i}] 交换"
            });
            Heapify(work, i, 0, recorder);
        }

        Array.Copy(work, arr, n);
        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            Extra = (int[])work.Clone(),
            SortedLow = 0,
            SortedHigh = n - 1,
            CodeLine = 3,
            Description = "堆排序完成"
        });
    }

    private static void Heapify(int[] a, int n, int i, StepRecorder recorder)
    {
        int largest = i;
        int l = 2 * i + 1, r = 2 * i + 2;

        if (l < n && a[l] > a[largest]) largest = l;
        if (r < n && a[r] > a[largest]) largest = r;

        if (largest != i)
        {
            (a[i], a[largest]) = (a[largest], a[i]);
            recorder.Record(() => new AnimStep
            {
                Op = AnimOp.Swap,
                Indices = new[] { i, largest },
                Extra = (int[])a.Clone(),
                CodeLine = 3,
                Description = $"调整堆：交换 A[{i}] 与 A[{largest}]"
            });
            Heapify(a, n, largest, recorder);
        }
    }
}
