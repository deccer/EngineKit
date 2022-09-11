namespace EngineKit;

public interface IMetrics
{
    uint FrameCounter { get; set; }

    uint FramesPerSecond { get; set; }

    uint UpdatesPerSecond { get; set; }

    float UpdateRate { get; set; }

    double SwapBufferDuration { get; set; }
}