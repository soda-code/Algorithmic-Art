namespace AlgorithmicArt.Models;

/// <summary>机器人/车辆位姿。</summary>
public struct RobotPose
{
    public double X { get; set; }
    public double Y { get; set; }
    /// <summary>朝向（弧度）。</summary>
    public double Heading { get; set; }

    public RobotPose(double x, double y, double heading)
    {
        X = x;
        Y = y;
        Heading = heading;
    }
}
