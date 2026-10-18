namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>快速排序。</summary>
public class QuickSortAlgorithm : IAlgorithm
{
    public string Category => "排序算法";
    public string Group => "排序";
    public string Name => "快速排序";
    public string Description => "选枢轴分区，递归排序左右子区间";

    public long EstimateSteps(int n) => (long)(n * Math.Log2(Math.Max(n, 2)) * 3);

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "quickSort(A, lo, hi)",
        "  pivot = A[hi]",
        "  i = lo - 1",
        "  for j = lo to hi-1",
        "    if A[j] < pivot: swap(A[++i], A[j])",
        "  swap(A[i+1], A[hi])",
        "  recurse left and right"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var arr = ctx.Array;
        if (arr == null || arr.Length < 2) return;

        var work = (int[])arr.Clone();
        QuickSort(work, 0, work.Length - 1, recorder);

        Array.Copy(work, arr, arr.Length);
        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            Extra = (int[])work.Clone(),
            SortedLow = 0,
            SortedHigh = arr.Length - 1,
            CodeLine = 6,
            Description = "快速排序完成"
        });
    }

    private static void QuickSort(int[] a, int lo, int hi, StepRecorder recorder)
    {
        if (lo >= hi) return;
        int p = Partition(a, lo, hi, recorder);
        QuickSort(a, lo, p - 1, recorder);
        QuickSort(a, p + 1, hi, recorder);
    }

    private static int Partition(int[] a, int lo, int hi, StepRecorder recorder)
    {
        int pivot = a[hi];
        int i = lo - 1;

        recorder.Record(() => new AnimStep
        {
            Op = AnimOp.Highlight,
            Indices = new[] { hi },
            Extra = (int[])a.Clone(),
            CodeLine = 1,
            Description = $"选择枢轴 A[{hi}]={pivot}"
        });

        for (int j = lo; j < hi; j++)
        {
            recorder.Record(() => new AnimStep
            {
                Op = AnimOp.Compare,
                Indices = new[] { j, hi },
                Extra = (int[])a.Clone(),
                CodeLine = 3,
                Description = $"比较 A[{j}]={a[j]} 与枢轴 {pivot}"
            });

            if (a[j] < pivot)
            {
                i++;
                if (i != j)
                {
                    (a[i], a[j]) = (a[j], a[i]);
                    recorder.Record(() => new AnimStep
                    {
                        Op = AnimOp.Swap,
                        Indices = new[] { i, j },
                        Extra = (int[])a.Clone(),
                        CodeLine = 4,
                        Description = $"交换 A[{i}] 与 A[{j}]"
                    });
                }
            }
        }

        (a[i + 1], a[hi]) = (a[hi], a[i + 1]);
        recorder.Record(() => new AnimStep
        {
            Op = AnimOp.Swap,
            Indices = new[] { i + 1, hi },
            Extra = (int[])a.Clone(),
            CodeLine = 5,
            Description = $"枢轴归位到索引 {i + 1}"
        });

        return i + 1;
    }
}
