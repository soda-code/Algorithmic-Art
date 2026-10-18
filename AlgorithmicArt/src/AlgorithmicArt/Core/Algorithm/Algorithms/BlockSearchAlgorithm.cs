namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>分块查找：先按块最大值定位所在块，再在块内顺序查找。</summary>
public class BlockSearchAlgorithm : IAlgorithm
{
    public string Category => "查找算法";
    public string Group => "查找";
    public string Name => "分块查找";
    public string Description => "用块索引表定位目标所在块，再块内顺序扫描";
    public SceneKind Scene => SceneKind.SortedArray;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "块大小 = ⌊√n⌋",
        "建立索引：每块最大值",
        "比较 target 与各块最大值，定位块号",
        "在块内顺序查找",
        "return 索引或 -1"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var arr = ctx.Array;
        if (arr == null || arr.Length == 0) return;

        int target = (int)ctx.Parameters.GetValueOrDefault("Target", arr[arr.Length / 2]);
        int n = arr.Length;
        int blockSize = Math.Max(1, (int)Math.Sqrt(n));
        int blocks = (n + blockSize - 1) / blockSize;

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 0,
            Description = $"数组长度 {n}，分 {blocks} 块，每块 {blockSize} 个元素"
        });

        // 建立块索引
        var maxOf = new int[blocks];
        for (int b = 0; b < blocks; b++)
        {
            int start = b * blockSize;
            int end = Math.Min(start + blockSize, n) - 1;
            maxOf[b] = arr[end]; // 有序数组，块末即块内最大值

            var range = Enumerable.Range(start, end - start + 1).ToArray();
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Visit,
                Indices = range,
                CodeLine = 1,
                Description = $"索引块 {b}：[{start},{end}]，最大值 {maxOf[b]}"
            });
        }

        // 定位块
        int block = -1;
        for (int b = 0; b < blocks; b++)
        {
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Compare,
                CodeLine = 2,
                Description = $"比较目标 {target} 与块 {b} 最大值 {maxOf[b]}"
            });

            if (target <= maxOf[b])
            {
                block = b;
                break;
            }
        }

        if (block < 0)
        {
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Highlight,
                CodeLine = 4,
                Description = $"目标 {target} 大于所有块最大值，未找到"
            });
            return;
        }

        int bs = block * blockSize;
        int be = Math.Min(bs + blockSize, n) - 1;

        recorder.Record(new AnimStep
        {
            Op = AnimOp.SetRange,
            CodeLine = 3,
            Description = $"目标位于块 {block} [{bs},{be}]，块内顺序查找"
        });

        for (int i = bs; i <= be; i++)
        {
            recorder.Record(new AnimStep
            {
                Op = AnimOp.Compare,
                Indices = new[] { i },
                CodeLine = 3,
                Description = $"顺序检查 A[{i}]={arr[i]}"
            });

            if (arr[i] == target)
            {
                recorder.Record(new AnimStep
                {
                    Op = AnimOp.Highlight,
                    Indices = new[] { i },
                    CodeLine = 4,
                    Description = $"找到目标 {target}，索引 {i}"
                });
                return;
            }

            if (arr[i] > target) break; // 有序数组，已超过目标
        }

        recorder.Record(new AnimStep
        {
            Op = AnimOp.Highlight,
            CodeLine = 4,
            Description = $"未找到目标 {target}"
        });
    }
}
