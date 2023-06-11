namespace EngineKit;

internal sealed class Metrics : IMetrics
{
    public double AverageFrameTime { get; set; }
    
    public long FrameCounter { get; set; }
}