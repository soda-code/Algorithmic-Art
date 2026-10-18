using System.Globalization;

namespace AlgorithmicArt.Data;

/// <summary>CSV 导入器：读取数值列 / 坐标点。</summary>
public static class CsvImporter
{
    /// <summary>读取指定列的浮点数值（自动跳过空行与无法解析的行）。</summary>
    public static double[]? ReadColumn(string path, int column = 0)
    {
        try
        {
            var values = new List<double>();
            foreach (var line in File.ReadLines(path))
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0) continue;

                var parts = trimmed.Split(',', ';', '\t');
                if (column >= parts.Length) continue;

                if (double.TryParse(parts[column].Trim(), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out var v))
                    values.Add(v);
            }
            return values.Count > 0 ? values.ToArray() : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>读取指定列的整数值。</summary>
    public static int[]? ReadIntColumn(string path, int column = 0)
    {
        var d = ReadColumn(path, column);
        if (d == null) return null;
        return d.Select(v => (int)Math.Round(v)).ToArray();
    }

    /// <summary>读取两列（x,y）坐标点。</summary>
    public static List<PointF>? ReadPoints(string path)
    {
        try
        {
            var points = new List<PointF>();
            foreach (var line in File.ReadLines(path))
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0) continue;

                var parts = trimmed.Split(',', ';', '\t');
                if (parts.Length < 2) continue;

                if (double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                    double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                    points.Add(new PointF((float)x, (float)y));
            }
            return points.Count > 0 ? points : null;
        }
        catch
        {
            return null;
        }
    }
}
