namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>选择排序。</summary>
public class SelectionSortAlgorithm : IAlgorithm
{
    public string Category => "排序算法";
    public string Group => "排序";
    public string Name => "选择排序";
    public string Description => "每轮选出最小元素放到未排序区首位";

    public long EstimateSteps(int n) => (long)n * n / 2;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "for i = 0 to n-2",
        "  min = i",
        "  for j = i+1 to n-1",
        "    if A[j] < A[min]: min = j",
        "  swap(A[i], A[min])"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var arr = ctx.Array;
        if (arr == null || arr.Length < 2) return;

        var work = (int[])arr.Clone();
        int n = work.Length;

        for (int i = 0; i < n - 1; i++)
        {
            int minIdx = i;
            for (int j = i + 1; j < n; j++)
            {
                recorder.Record(() => new AnimStep
                {
                    Op = AnimOp.Compare,
                    Indices = new[] { j, minIdx },
                    Extra = (int[])work.Clone(),
                    SortedLow = 0,
                    SortedHigh = i - 1,
                    CodeLine = 3,
                    Description = $"比较 A[{j}]={work[j]} 与当前最小 A[{minIdx}]={work[minIdx]}"
                });

                if (work[j] < work[minIdx]) minIdx = j;
            }

            if (minIdx != i)
            {
                (work[i], work[minIdx]) = (work[minIdx], work[i]);
                recorder.Record(() => new AnimStep
                {
                    Op = AnimOp.Swap,
                    Indices = new[] { i, minIdx },
                    Extra = (int[])work.Clone(),
                    SortedLow = 0,
                    SortedHigh = i,
                    CodeLine = 4,
                    Description = $"交换 A[{i}] 与 A[{minIdx}]"
                });
            }
        }

        Array.Copy(work, arr, n);
        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            Extra = (int[])work.Clone(),
            SortedLow = 0,
            SortedHigh = n - 1,
            CodeLine = 4,
            Description = "选择排序完成"
        });
    }
}
