namespace EngineKit;

internal sealed class Metrics : IMetrics
{
    public uint FrameCounter { get; set; }

    public uint FramesPerSecond { get; set; }

    public uint UpdatesPerSecond { get; set; }

    public float UpdateRate { get; set; }

    public double SwapBufferDuration { get; set; }

    public float DeltaTime { get; set; }

    public bool ShowFramesPerSecond { get; set; } = true;

    public long CurrentTime { get; set; }
}