using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace EngineKit;

public static class Interpolation
{
    public static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerp(float a, float b, float t)
    {
        return (float)(a + (b - a) * t);
    }

    public static float SmootherStep(float min, float max, float value)
    {
        var t = Math.Max(0, Math.Min(1, (value - min) / (max - min)));
        return Fade(t);
    }

    public static float SmoothStep(float min, float max, float value)
    {
        var x = Math.Max(0, Math.Min(1, (value - min) / (max - min)));
        return x * x * x;
    }

    public static double SmoothStep(double min, double max, double value)
    {
        var x = Math.Max(0, Math.Min(1, (value - min) / (max - min)));
        return x * x * x;
    }

    public static float Step(float edge, float value)
    {
        return value < edge ? 0.0f : (float)1.0;
    }

    public static Vector3 Step(float edge, Vector3 value)
    {
        return new Vector3(
            value.X < edge ? 0.0f : 1.0f,
            value.Y < edge ? 0.0f : 1.0f,
            value.Z < edge ? 0.0f : 1.0f);
    }
}
