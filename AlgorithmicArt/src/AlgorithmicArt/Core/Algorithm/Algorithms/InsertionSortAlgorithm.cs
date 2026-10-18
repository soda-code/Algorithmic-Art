namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>插入排序。</summary>
public class InsertionSortAlgorithm : IAlgorithm
{
    public string Category => "排序算法";
    public string Group => "排序";
    public string Name => "插入排序";
    public string Description => "逐个把元素插入到前面已排序序列的正确位置";

    public long EstimateSteps(int n) => (long)n * n / 4;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "for i = 1 to n-1",
        "  key = A[i], j = i-1",
        "  while j >= 0 and A[j] > key",
        "    A[j+1] = A[j], j--",
        "  A[j+1] = key"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var arr = ctx.Array;
        if (arr == null || arr.Length < 2) return;

        var work = (int[])arr.Clone();
        int n = work.Length;

        for (int i = 1; i < n; i++)
        {
            int key = work[i];
            int j = i - 1;

            recorder.Record(() => new AnimStep
            {
                Op = AnimOp.Highlight,
                Indices = new[] { i },
                Extra = (int[])work.Clone(),
                SortedLow = 0,
                SortedHigh = i - 1,
                CodeLine = 1,
                Description = $"取出 A[{i}]={key} 作为待插入元素"
            });

            while (j >= 0 && work[j] > key)
            {
                work[j + 1] = work[j];
                recorder.Record(() => new AnimStep
                {
                    Op = AnimOp.Assign,
                    Indices = new[] { j, j + 1 },
                    Extra = (int[])work.Clone(),
                    SortedLow = 0,
                    SortedHigh = i - 1,
                    CodeLine = 3,
                    Description = $"A[{j}]={work[j]} 后移到位置 {j + 1}"
                });
                j--;
            }

            work[j + 1] = key;
            recorder.Record(() => new AnimStep
            {
                Op = AnimOp.Assign,
                Indices = new[] { j + 1 },
                Extra = (int[])work.Clone(),
                SortedLow = 0,
                SortedHigh = i,
                CodeLine = 4,
                Description = $"将 {key} 插入到位置 {j + 1}"
            });
        }

        Array.Copy(work, arr, n);
        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            Extra = (int[])work.Clone(),
            SortedLow = 0,
            SortedHigh = n - 1,
            CodeLine = 4,
            Description = "插入排序完成"
        });
    }
}
