using AlgorithmicArt.Data;
using AlgorithmicArt.Models;

namespace AlgorithmicArt.Core.Algorithm;

/// <summary>场景构建器：按场景类型生成初始数据。</summary>
public static class SceneBuilder
{
    public static AlgorithmContext Build(SceneKind kind, int n, FilterSettings? filter = null,
        PidSettings? pid = null, int matrixRows = 3, int matrixCols = 3)
    {
        matrixRows = Math.Clamp(matrixRows, 2, 6);
        matrixCols = Math.Clamp(matrixCols, 2, 6);

        filter ??= new FilterSettings();
        pid ??= new PidSettings();
        var ctx = new AlgorithmContext();

        switch (kind)
        {
            case SceneKind.RandomArray:
                ctx.Array = GenerateArray(n);
                ctx.Parameters["Target"] = ctx.Array[ctx.Array.Length / 2];
                break;

            case SceneKind.SortedArray:
                ctx.Array = GenerateArray(n);
                Array.Sort(ctx.Array);
                ctx.Parameters["Target"] = ctx.Array[ctx.Array.Length / 2];
                break;

            case SceneKind.Signal:
                ctx.Signal = GenerateSignal(n);
                ctx.Parameters["Window"] = filter.MovingWindow;
                ctx.Parameters["Fs"] = filter.SampleRate;
                ctx.Parameters["Fc"] = filter.CutoffFreq;
                ctx.Parameters["Quality"] = filter.Quality;
                ctx.Parameters["KalmanQ"] = filter.KalmanQ;
                ctx.Parameters["KalmanR"] = filter.KalmanR;
                break;

            case SceneKind.GridMap:
                ctx.Map = GenerateMap(n);
                break;

            case SceneKind.Trajectory:
                ctx.Path = GeneratePath();
                ctx.Trace = new List<PointF>();
                ctx.Parameters["Kp"] = pid.Kp;
                ctx.Parameters["Ki"] = pid.Ki;
                ctx.Parameters["Kd"] = pid.Kd;
                ctx.Parameters["Speed"] = pid.Speed;
                ctx.Parameters["LookAhead"] = pid.LookAhead;
                ctx.Parameters["IntegralLimit"] = pid.IntegralLimit;
                ctx.Parameters["MaxSteer"] = pid.MaxSteer;
                ctx.Parameters["MaxSteps"] = pid.MaxSteps;
                break;

            case SceneKind.StepResponse:
                // 阶跃响应对象时间常数约 2s，参数适度放大
                ctx.Parameters["Kp"] = pid.Kp * 2.0;
                ctx.Parameters["Ki"] = pid.Ki * 10.0;
                ctx.Parameters["Kd"] = pid.Kd;
                ctx.Parameters["IntegralLimit"] = pid.IntegralLimit;
                ctx.Parameters["MaxSteer"] = pid.MaxOutput;
                break;

            case SceneKind.MlClusters:
                ctx.MlPoints = GenerateClusters(n);
                ctx.ClusterOf = Enumerable.Repeat(-1, ctx.MlPoints.Length).ToArray();
                ctx.Parameters["K"] = 3;
                break;

            case SceneKind.MlRegression:
                ctx.MlPoints = GenerateRegressionPoints(n);
                ctx.ClusterOf = Enumerable.Repeat(-1, ctx.MlPoints.Length).ToArray();
                ctx.Parameters["Epochs"] = 60;
                break;

            case SceneKind.MlClassification:
            {
                var (points, query) = GenerateClassificationPoints(n);
                ctx.MlPoints = points;
                ctx.QueryPoint = query;
                ctx.Parameters["K"] = 5;
                break;
            }

            case SceneKind.MatrixMultiply:
                // A(rows×cols) × B(cols×rows) = C(rows×rows)
                ctx.MatrixA = GenerateMatrix(matrixRows, matrixCols, 1, 6, 21);
                ctx.MatrixB = GenerateMatrix(matrixCols, matrixRows, 1, 6, 22);
                break;

            case SceneKind.MatrixArithmetic:
                ctx.MatrixA = GenerateMatrix(matrixRows, matrixCols, 1, 9, 61);
                ctx.MatrixB = GenerateMatrix(matrixRows, matrixCols, 1, 9, 62);
                ctx.Parameters["Scalar"] = 3;
                break;

            case SceneKind.MatrixTranspose:
                ctx.MatrixA = GenerateMatrix(matrixRows, matrixCols, 1, 9, 63);
                break;

            case SceneKind.MatrixInverse:
            {
                // 方阵 + 对角占优，保证可逆
                var inv = GenerateMatrix(matrixRows, matrixRows, 1, 5, 64);
                for (int i = 0; i < matrixRows; i++) inv[i, i] += 8;
                ctx.MatrixA = inv;
                break;
            }

            case SceneKind.LinearSystem:
            {
                // 对角占优，保证消元过程稳定、有唯一解
                var a = GenerateMatrix(matrixRows, matrixRows, 2, 9, 31);
                for (int i = 0; i < matrixRows; i++) a[i, i] += 10;
                ctx.MatrixA = a;
                ctx.Vector = Enumerable.Range(0, matrixRows).Select(i => (double)(7 + i * 2)).ToArray();
                break;
            }

            case SceneKind.LuDecomposition:
            {
                var a = GenerateMatrix(matrixRows, matrixRows, 1, 5, 41);
                for (int i = 0; i < matrixRows; i++) a[i, i] += 12;
                ctx.MatrixA = a;
                break;
            }

            case SceneKind.PowerIteration:
                // 对称矩阵，主特征值 3，特征向量方向 (1,1)
                ctx.MatrixA = new double[,] { { 2, 1 }, { 1, 2 } };
                ctx.Vector = new double[] { 1, 0 };
                break;

            case SceneKind.PcaScatter:
                ctx.MlPoints = GeneratePcaPoints(n);
                break;
        }

        return ctx;
    }

    private static int[] GenerateArray(int n)
    {
        n = Math.Max(4, n);
        var rnd = new Random(42);
        var arr = new int[n];
        for (int i = 0; i < n; i++) arr[i] = rnd.Next(1, 1000);
        return arr;
    }

    private static double[] GenerateSignal(int n)
    {
        n = Math.Max(16, n);
        var sig = new double[n];
        var rnd = new Random(9);
        for (int i = 0; i < n; i++)
        {
            double clean = Math.Sin(2 * Math.PI * 5 * i / n);
            double noise = (rnd.NextDouble() - 0.5) * 0.6;
            sig[i] = clean + noise;
        }
        return sig;
    }

    private static GridMap GenerateMap(int n)
    {
        int size = Math.Clamp(n, 10, 40);
        var map = new GridMap(size, size);
        var rnd = new Random(7);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (rnd.NextDouble() < 0.12)
                    map.Set(x, y, CellState.Obstacle);
            }
        }
        map.Set(0, 0, CellState.Start);
        return map;
    }

    private static List<PointF> GeneratePath()
    {
        var path = new List<PointF>();
        for (int i = 0; i <= 100; i++)
        {
            float t = i / 100f;
            float x = 10 + 80 * t;
            float y = 50 + 30 * (float)Math.Sin(t * Math.PI * 2);
            path.Add(new PointF(x, y));
        }
        return path;
    }

    // ---------- 机器学习场景 ----------

    /// <summary>三个高斯簇，用于 K-Means 聚类演示。</summary>
    private static MlPoint[] GenerateClusters(int n)
    {
        int total = Math.Clamp(n, 12, 60);
        var rnd = new Random(11);
        var centers = new[] { new PointF(24, 74), new PointF(52, 28), new PointF(80, 66) };
        var list = new List<MlPoint>(total);

        for (int i = 0; i < total; i++)
        {
            int c = i % centers.Length;
            double angle = rnd.NextDouble() * Math.PI * 2;
            double radius = Math.Sqrt(rnd.NextDouble()) * 13;
            float x = (float)(centers[c].X + Math.Cos(angle) * radius);
            float y = (float)(centers[c].Y + Math.Sin(angle) * radius);

            list.Add(new MlPoint(Math.Clamp(x, 2f, 98f), Math.Clamp(y, 2f, 98f), c));
        }

        return list.ToArray();
    }

    /// <summary>沿 y = 0.62x + 14 分布并叠加噪声，用于线性回归演示。</summary>
    private static MlPoint[] GenerateRegressionPoints(int n)
    {
        int total = Math.Clamp(n, 10, 60);
        var rnd = new Random(13);
        var list = new List<MlPoint>(total);

        for (int i = 0; i < total; i++)
        {
            float x = 6 + (float)rnd.NextDouble() * 86;
            double noise = (rnd.NextDouble() - 0.5) * 20;
            float y = (float)(0.62 * x + 14 + noise);

            list.Add(new MlPoint(x, Math.Clamp(y, 2f, 98f)));
        }

        return list.ToArray();
    }

    // ---------- 线性代数场景 ----------

    /// <summary>生成指定范围的整数矩阵。</summary>
    private static double[,] GenerateMatrix(int rows, int cols, int min, int max, int seed)
    {
        var rnd = new Random(seed);
        var m = new double[rows, cols];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                m[r, c] = rnd.Next(min, max + 1);
        return m;
    }

    /// <summary>沿约 30° 方向分布、带横向噪声的二维点集，用于 PCA 演示。</summary>
    private static MlPoint[] GeneratePcaPoints(int n)
    {
        int total = Math.Clamp(n, 20, 80);
        var rnd = new Random(51);
        var list = new List<MlPoint>(total);

        double ang = Math.PI / 6;
        double cos = Math.Cos(ang), sin = Math.Sin(ang);

        for (int i = 0; i < total; i++)
        {
            double along = (rnd.NextDouble() * 2 - 1) * 30;
            double across = (rnd.NextDouble() - 0.5) * 14;

            float x = (float)(50 + along * cos - across * sin);
            float y = (float)(50 + along * sin + across * cos);

            list.Add(new MlPoint(Math.Clamp(x, 4f, 96f), Math.Clamp(y, 4f, 96f)));
        }

        return list.ToArray();
    }

    /// <summary>两类散点（左下 / 右上）+ 交界处待测点，用于 KNN 分类演示。</summary>
    private static (MlPoint[] Points, PointF Query) GenerateClassificationPoints(int n)
    {
        int total = Math.Clamp(n, 10, 60);
        var rnd = new Random(17);
        var list = new List<MlPoint>(total);

        for (int i = 0; i < total; i++)
        {
            bool upper = rnd.NextDouble() < 0.5;
            float x = upper
                ? 32 + (float)rnd.NextDouble() * 58
                : 6 + (float)rnd.NextDouble() * 58;
            float y = upper
                ? 46 + (float)rnd.NextDouble() * 46
                : 6 + (float)rnd.NextDouble() * 46;

            list.Add(new MlPoint(x, y, upper ? 1 : 0));
        }

        return (list.ToArray(), new PointF(64, 50));
    }
}
