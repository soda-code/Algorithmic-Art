using AlgorithmicArt.Core.Algorithm.Algorithms;

namespace AlgorithmicArt.Core.Algorithm;

/// <summary>算法注册表：按分类顺序注册全部算法。</summary>
public static class AlgorithmRegistry
{
    public static void RegisterAll(AlgorithmManager manager)
    {
        // 查找算法
        manager.Register(new SequentialSearchAlgorithm());
        manager.Register(new BinarySearchAlgorithm());
        manager.Register(new InterpolationSearchAlgorithm());
        manager.Register(new FibonacciSearchAlgorithm());
        manager.Register(new JumpSearchAlgorithm());
        manager.Register(new ExponentialSearchAlgorithm());
        manager.Register(new BlockSearchAlgorithm());
        manager.Register(new HashSearchAlgorithm());

        // 排序算法
        manager.Register(new BubbleSortAlgorithm());
        manager.Register(new SelectionSortAlgorithm());
        manager.Register(new InsertionSortAlgorithm());
        manager.Register(new MergeSortAlgorithm());
        manager.Register(new QuickSortAlgorithm());
        manager.Register(new HeapSortAlgorithm());

        // 信号算法
        manager.Register(new SignalGenerateAlgorithm());
        manager.Register(new MovingAverageAlgorithm());
        manager.Register(new SecondOrderFilterAlgorithm());
        manager.Register(new KalmanFilterAlgorithm());
        manager.Register(new FftAlgorithm());
        manager.Register(new DftAlgorithm());

        // 控制算法
        manager.Register(new SweepAlgorithm());
        manager.Register(new PidStepAlgorithm());

        // 机器学习算法
        manager.Register(new KMeansAlgorithm());
        manager.Register(new KNearestNeighborsAlgorithm());
        manager.Register(new LinearRegressionAlgorithm());
        manager.Register(new GradientDescentAlgorithm());

        // 线性代数算法
        manager.Register(new MatrixAdditionAlgorithm());
        manager.Register(new MatrixSubtractionAlgorithm());
        manager.Register(new MatrixScaleAlgorithm());
        manager.Register(new MatrixMultiplicationAlgorithm());
        manager.Register(new MatrixTransposeAlgorithm());
        manager.Register(new MatrixInverseAlgorithm());
        manager.Register(new GaussianEliminationAlgorithm());
        manager.Register(new LuDecompositionAlgorithm());
        manager.Register(new PowerIterationAlgorithm());
        manager.Register(new PcaAlgorithm());
    }
}
