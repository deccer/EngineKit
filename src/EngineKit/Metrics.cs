namespace EngineKit;

internal sealed class Metrics : IMetrics
{
    public uint FrameCounter { get; set; }
    
    public uint FramesPerSecond { get; set; }
    
    public uint UpdatesPerSecond { get; set; }
    
    public float UpdateRate { get; set; }
    
    public double SwapBufferDuration { get; set; }
}