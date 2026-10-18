using AlgorithmicArt.Models;
using AlgorithmicArt.Render;

namespace AlgorithmicArt.Core.Algorithm.Algorithms;

/// <summary>扫地机器人蛇形清扫。</summary>
public class SweepAlgorithm : IAlgorithm
{
    public string Category => "控制算法";
    public string Group => "扫地";
    public string Name => "蛇形清扫";
    public string Description => "逐行左右往复清扫，遇到障碍跳过";
    public RendererKind Renderer => RendererKind.GridMap;
    public SceneKind Scene => SceneKind.GridMap;

    public IReadOnlyList<string> PseudoCode => new[]
    {
        "for y = 0 to H-1",
        "  for each x in row (蛇形)",
        "    if cell != obstacle: mark scanned",
        "    record(x, y)"
    };

    public void Run(AlgorithmContext ctx, StepRecorder recorder)
    {
        var map = ctx.Map;
        if (map == null) return;

        var scanned = new List<int>();

        for (int y = 0; y < map.Height; y++)
        {
            bool leftToRight = y % 2 == 0;
            for (int k = 0; k < map.Width; k++)
            {
                int x = leftToRight ? k : map.Width - 1 - k;
                if (map.Get(x, y) == CellState.Obstacle) continue;

                int idx = map.Index(x, y);
                scanned.Add(idx);

                recorder.Record(new AnimStep
                {
                    Op = AnimOp.MarkGrid,
                    Indices = new[] { idx },
                    Points = new[] { new PointF(x, y) },
                    Extra = scanned.ToArray(),
                    CodeLine = 2,
                    Description = $"清扫格子 ({x},{y})，累计 {scanned.Count}"
                });
            }
        }
    }
}
