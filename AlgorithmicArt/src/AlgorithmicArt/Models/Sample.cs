namespace AlgorithmicArt.Models;

/// <summary>采样点（多通道）。</summary>
public class Sample
{
    public DateTime Timestamp { get; set; }
    public double[] Values { get; set; }

    public Sample(DateTime timestamp, double[] values)
    {
        Timestamp = timestamp;
        Values = values;
    }
}
