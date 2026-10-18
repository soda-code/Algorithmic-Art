namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>归并排序。</summary>
public class MergeSortAlgorithm : IAlgorithm
{
    public string Category => "排序算法";
    public string Group => "排序";
    public string Name => "归并排序";
    public string Description => "分治：递归拆分后合并两个有序序列";

    public long EstimateSteps(int n) => (long)(n * Math.Log2(Math.Max(n, 2)) * 2);

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "mergeSort(A, lo, hi)",
        "  mid = (lo + hi) / 2",
        "  mergeSort(lo, mid); mergeSort(mid+1, hi)",
        "  merge two halves"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var arr = ctx.Array;
        if (arr == null || arr.Length < 2) return;

        var work = (int[])arr.Clone();
        var tmp = new int[work.Length];
        MergeSort(work, tmp, 0, work.Length - 1, recorder);

        Array.Copy(work, arr, arr.Length);
        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            Extra = (int[])work.Clone(),
            SortedLow = 0,
            SortedHigh = arr.Length - 1,
            CodeLine = 3,
            Description = "归并排序完成"
        });
    }

    private static void MergeSort(int[] a, int[] tmp, int lo, int hi, StepRecorder recorder)
    {
        if (lo >= hi) return;
        int mid = (lo + hi) / 2;
        MergeSort(a, tmp, lo, mid, recorder);
        MergeSort(a, tmp, mid + 1, hi, recorder);
        Merge(a, tmp, lo, mid, hi, recorder);
    }

    private static void Merge(int[] a, int[] tmp, int lo, int mid, int hi, StepRecorder recorder)
    {
        int i = lo, j = mid + 1, k = lo;

        while (i <= mid && j <= hi)
        {
            recorder.Record(() => new AnimStep
            {
                Op = AnimOp.Compare,
                Indices = new[] { i, j },
                Extra = (int[])a.Clone(),
                CodeLine = 3,
                Description = $"比较 A[{i}]={a[i]} 与 A[{j}]={a[j]}"
            });

            if (a[i] <= a[j]) tmp[k++] = a[i++];
            else tmp[k++] = a[j++];
        }

        while (i <= mid) tmp[k++] = a[i++];
        while (j <= hi) tmp[k++] = a[j++];

        for (int t = lo; t <= hi; t++) a[t] = tmp[t];

        recorder.Record(() => new AnimStep
        {
            Op = AnimOp.Assign,
            Extra = (int[])a.Clone(),
            CodeLine = 3,
            Description = $"合并区间 [{lo},{hi}]"
        });
    }
}
