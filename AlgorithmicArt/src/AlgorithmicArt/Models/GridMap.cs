namespace AlgorithmicArt.Models;

/// <summary>栅格单元状态。</summary>
public enum CellState
{
    Empty,
    Obstacle,
    Scanned,
    Path,
    Start,
    Goal
}

/// <summary>二维栅格地图（扫地 / 寻路）。</summary>
public class GridMap
{
    public int Width { get; }
    public int Height { get; }
    public CellState[] Cells { get; }

    public GridMap(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        Width = width;
        Height = height;
        Cells = new CellState[width * height];
    }

    public int Index(int x, int y) => y * Width + x;

    public CellState Get(int x, int y) => Cells[Index(x, y)];

    public void Set(int x, int y, CellState state) => Cells[Index(x, y)] = state;

    public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
}
