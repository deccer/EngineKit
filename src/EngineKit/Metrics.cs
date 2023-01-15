using System;
using System.Linq;

namespace EngineKit;

internal sealed class Metrics : IMetrics
{
    private static readonly int[] _framesBuffer = new int[100];

    private static int _counter;

    public uint FrameCounter { get; set; }

    public uint FramesPerSecond { get; set; }

    public uint UpdatesPerSecond { get; set; }

    public float UpdateRate { get; set; }

    public double SwapBufferDuration { get; set; }

    public float DeltaTime { get; set; }

    public bool ShowFramesPerSecond { get; set; } = true;

    public int ShowFramesPerSecondInterval { get; set; } = 1000;

    public long CurrentTime { get; set; }

    public void CollectFrameSample()
    {
        if (_counter == 1)
        {
            Array.Fill(_framesBuffer, (int)FramesPerSecond);
        }
        _framesBuffer[_counter++ % 100] = (int)FramesPerSecond;
    }

    public int GetAverageFps()
    {
        return _framesBuffer.Sum() / 100;
    }

    public int GetLow1PercentFps()
    {
        return _framesBuffer.Min();
    }
}