namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>斐波那契查找：用斐波那契数列分割有序数组。</summary>
public class FibonacciSearchAlgorithm : IAlgorithm
{
    public string Category => "查找算法";
    public string Group => "查找";
    public string Name => "斐波那契查找";
    public string Description => "用斐波那契数作为分割点，缩小查找范围";
    public SceneKind Scene => SceneKind.SortedArray;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "找到最小的 fibM ≥ n",
        "offset = -1",
        "while fibM > 1",
        "  i = min(offset + fibM2, n-1)",
        "  if A[i] < target: 右移窗口",
        "  else if A[i] > target: 左缩窗口",
        "  else: return i",
        "检查最后一个元素"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var arr = ctx.Array;
        if (arr == null || arr.Length == 0) return;

        int target = (int)ctx.Parameters.GetValueOrDefault("Target", arr[arr.Length / 2]);
        int n = arr.Length;

        int fibM2 = 0;   // F(m-2)
        int fibM1 = 1;   // F(m-1)
        int fibM = fibM2 + fibM1; // F(m)

        while (fibM < n)
        {
            fibM2 = fibM1;
            fibM1 = fibM;
            fibM = fibM2 + fibM1;
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 0,
            Description = $"选择分割基准 F(m)={fibM}，F(m-1)={fibM1}，F(m-2)={fibM2}"
        });

        int offset = -1;

        while (fibM > 1)
        {
            int i = Math.Min(offset + fibM2, n - 1);

            recorder.Record(new AnimStep
            {
                Op = AnimOp.Compare,
                Indices = new[] { i },
                CodeLine = 3,
                Description = $"比较 A[{i}]={arr[i]} 与目标 {target}（F={fibM}）"
            });

            if (arr[i] < target)
            {
                fibM = fibM1;
                fibM1 = fibM2;
                fibM2 = fibM - fibM1;
                offset = i;

                recorder.Record(new AnimStep
                {
                    Op = AnimOp.SetRange,
                    CodeLine = 4,
                    Description = $"A[{i}] < 目标，窗口右移，offset={offset}"
                });
            }
            else if (arr[i] > target)
            {
                fibM = fibM2;
                fibM1 -= fibM2;
                fibM2 = fibM - fibM1;

                recorder.Record(new AnimStep
                {
                    Op = AnimOp.SetRange,
                    CodeLine = 5,
                    Description = $"A[{i}] > 目标，窗口左缩，F(m)={fibM}"
                });
            }
            else
            {
                recorder.Record(new AnimStep
                {
                    Op = AnimOp.Highlight,
                    Indices = new[] { i },
                    CodeLine = 6,
                    Description = $"找到目标 {target}，索引 {i}"
                });
                return;
            }
        }

        if (fibM1 == 1 && offset + 1 < n && arr[offset + 1] == target)
        {
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Highlight,
                Indices = new[] { offset + 1 },
                CodeLine = 7,
                Description = $"命中最后一个候选，索引 {offset + 1}"
            });
            return;
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 7,
            Description = $"未找到目标 {target}"
        });
    }
}
