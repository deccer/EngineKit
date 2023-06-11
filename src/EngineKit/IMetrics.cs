namespace EngineKit;

public interface IMetrics
{
    double AverageFrameTime { get; set; }
    
    long FrameCounter { get; set; }
}