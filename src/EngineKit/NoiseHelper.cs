using System;

namespace EngineKit;

public static class NoiseHelper
{
    public static float PerlinNoise(float value, float period, int octaves, int seed)
    {
        var noiseSum = 0.0f;

        var frequency = period;
        var amplitude = 0.5f;
        for (var octave = 0; octave < octaves - 1; octave++)
        {
            var v = value * frequency + seed * 12.468f;
            var a = Noise((int)v, seed);
            var b = Noise((int)v + 1, seed);
            var t = InterpolationHelper.Fade(v - (float)Math.Floor(v));
            noiseSum += InterpolationHelper.Lerp(a, b, t) * amplitude;
            frequency *= 2;
            amplitude *= 0.5f;
        }

        return noiseSum;
    }

    private static float Noise(int x, int seed)
    {
        int n = x + seed * 137;
        n = (n << 13) ^ n;
        return (float)(1.0 - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0);
    }

}