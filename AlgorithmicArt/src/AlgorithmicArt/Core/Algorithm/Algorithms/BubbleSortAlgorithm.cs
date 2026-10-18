namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>冒泡排序。</summary>
public class BubbleSortAlgorithm : IAlgorithm
{
    public string Category => "排序算法";
    public string Group => "排序";
    public string Name => "冒泡排序";
    public string Description => "重复比较相邻元素并交换，使较大值冒泡到末尾";

    public long EstimateSteps(int n) => (long)n * n / 2;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "for i = 0 to n-2",
        "  for j = 0 to n-i-2",
        "    if A[j] > A[j+1]: swap",
        "  record(A)"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var arr = ctx.Array;
        if (arr == null || arr.Length < 2) return;

        var work = (int[])arr.Clone();
        int n = work.Length;

        for (int i = 0; i < n - 1; i++)
        {
            bool swapped = false;
            for (int j = 0; j < n - 1 - i; j++)
            {
                recorder.Record(() => new AnimStep
                {
                    Op = AnimOp.Compare,
                    Indices = new[] { j, j + 1 },
                    Extra = (int[])work.Clone(),
                    SortedLow = n - i,
                    SortedHigh = n - 1,
                    CodeLine = 1,
                    Description = $"比较 A[{j}]={work[j]} 与 A[{j + 1}]={work[j + 1]}"
                });

                if (work[j] > work[j + 1])
                {
                    (work[j], work[j + 1]) = (work[j + 1], work[j]);
                    swapped = true;
                    recorder.Record(() => new AnimStep
                    {
                        Op = AnimOp.Swap,
                        Indices = new[] { j, j + 1 },
                        Extra = (int[])work.Clone(),
                        SortedLow = n - i,
                        SortedHigh = n - 1,
                        CodeLine = 2,
                        Description = $"交换 A[{j}] 与 A[{j + 1}]"
                    });
                }
            }
            if (!swapped) break;
        }

        Array.Copy(work, arr, n);
        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            Extra = (int[])work.Clone(),
            SortedLow = 0,
            SortedHigh = n - 1,
            CodeLine = 3,
            Description = "冒泡排序完成"
        });
    }
}
