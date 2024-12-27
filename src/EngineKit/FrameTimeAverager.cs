namespace EngineKit;

internal class FrameTimeAverager
{
    private readonly double _decayRate;
    private readonly double _timeLimit;

    private double _accumulatedTime = 0;
    private int _frameCount = 0;

    public double CurrentAverageFrameTime { get; private set; }

    public double CurrentAverageFramesPerSecond => 1000.0 / CurrentAverageFrameTime;

    public FrameTimeAverager(double maxTimeMilliseconds)
    {
        _timeLimit = maxTimeMilliseconds;
        _decayRate = 0.3;
    }

    public void Reset()
    {
        _accumulatedTime = 0;
        _frameCount = 0;
    }

    public void AddTime(double frameTimeInMs)
    {
        _accumulatedTime += frameTimeInMs;
        _frameCount++;
        if (_accumulatedTime >= _timeLimit)
        {
            Average();
        }
    }

    private void Average()
    {
        var total = _accumulatedTime;
        CurrentAverageFrameTime = CurrentAverageFrameTime * _decayRate + total / _frameCount * (1 - _decayRate);
        _accumulatedTime = 0;
        _frameCount = 0;
    }
}
