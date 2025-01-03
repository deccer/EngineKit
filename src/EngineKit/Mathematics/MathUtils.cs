﻿using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace EngineKit.Mathematics;

public static class MathUtils
{
    public static Vector3 Exp(float edge, Vector3 value)
    {
        return new Vector3(
            value.X < edge ? 0.0f : 1.0f,
            value.Y < edge ? 0.0f : 1.0f,
            value.Z < edge ? 0.0f : 1.0f);
    }

    public static Vector2 Clamp(Vector2 v, Vector2 mn, Vector2 mx)
    {
        return new Vector2((v.X < mn.X)
            ? mn.X
            : (v.X > mx.X)
                ? mx.X
                : v.X, (v.Y < mn.Y) ? mn.Y : (v.Y > mx.Y) ? mx.Y : v.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(T lhs, T rhs) where T : IComparable<T>
    {
        return lhs.CompareTo(rhs) < 0 ? lhs : rhs;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Max<T>(T lhs, T rhs) where T : IComparable<T>
    {
        return lhs.CompareTo(rhs) >= 0 ? lhs : rhs;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
    {
        if (val.CompareTo(min) < 0) return min;
        else if (val.CompareTo(max) > 0) return max;
        else return val;
    }

    public static int ClampForEnum<T>(this int i) where T : struct, Enum 
    {
        return i.Clamp(0, Enum.GetValues<T>().Length - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Fmod(float v, float mod)
    {
        return v - mod * (float)Math.Floor(v / mod);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Fmod(double v, double mod)
    {
        return v - mod * Math.Floor(v / mod);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float NormalizeAndClamp(float value, float min, float max)
    {
        return MathF.Max(0, MathF.Min(1, (value - min) / (max - min)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double NormalizeAndClamp(double value, double min, double max)
    {
        return Math.Max(0, Math.Min(1, (value - min) / (max - min)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RemapAndClamp(float value, float inMin, float inMax, float outMin, float outMax)
    {
        var factor = (value - inMin) / (inMax - inMin);
        var v = factor * (outMax - outMin) + outMin;
        if (outMin > outMax)
        {
            Swapper.Swap(ref outMin, ref outMax);
        }
        return v.Clamp(outMin, outMax);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
    {
        var factor = (value - inMin) / (inMax - inMin);
        var v = factor * (outMax - outMin) + outMin;
        return v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double RemapAndClamp(double value, double inMin, double inMax, double outMin, double outMax)
    {
        var factor = (value - inMin) / (inMax - inMin);
        var v = factor * (outMax - outMin) + outMin;
        if (v > outMax)
        {
            v = outMax;
        }
        else if (v < outMin)
        {
            v = outMin;
        }

        return v;
    }

    public static Vector2 Min(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.X < rhs.X ? lhs.X : rhs.X, lhs.Y < rhs.Y ? lhs.Y : rhs.Y);
    }

    public static Vector2 Floor(Vector2 v)
    {
        return new Vector2((float)Math.Floor(v.X), (float)Math.Floor(v.Y));
    }

    public static Vector2 Max(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.X >= rhs.X ? lhs.X : rhs.X, lhs.Y >= rhs.Y ? lhs.Y : rhs.Y);
    }

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        return new Vector2(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
    }

    public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
    {
        return new Vector3(a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t,
            a.Z + (b.Z - a.Z) * t);
    }

    public static Vector4 Lerp(Vector4 a, Vector4 b, float t)
    {
        return new Vector4(a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t,
            a.Z + (b.Z - a.Z) * t,
            a.W + (b.W - a.W) * t);
    }

    public static double Lerp(double a, double b, double t)
    {
        return (double)(a + (b - a) * t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Lerp(int a, int b, float t)
    {
        return (int)(a + (b - a) * t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Log2(double value)
    {
        return Math.Log10(value) / Math.Log10(2.0);
    }

    public static float RoundValue(float i, float stepsPerUnit, float stepRatio)
    {
        var u = 1 / stepsPerUnit;
        var v = stepRatio / (2 * stepsPerUnit);
        var m = i % u;
        var r = m - (m < v
            ? 0
            : (m > (u - v))
                ? u
                : ((m - v) / (1 - 2 * stepsPerUnit * v)));
        var y = i - r;
        return y;
    }

    /// <summary>
    /// Smooth damps a value with a "critically damped spring" similar to unity's SmoothDamp helper method.
    /// See https://stackoverflow.com/a/5100956 
    /// </summary>
    public static float SpringDamp(
        float target,
        float current,
        ref float velocity,
        float springConstant = 2,
        float timeStep = 1 / 60f)
    {
        //const float springConstant = 0.41f;
        var currentToTarget = target - current;
        var springForce = currentToTarget * springConstant;
        var dampingForce = -velocity * 2 * MathF.Sqrt(springConstant);
        var force = springForce + dampingForce;
        velocity += force * timeStep;
        var displacement = velocity * timeStep;
        return current + displacement;
    }

    /// <summary>
    /// Return true if a boolean changed from false to true
    /// </summary>
    public static bool WasTriggered(bool newState, ref bool current)
    {
        if (newState == current)
        {
            return false;
        }

        current = newState;
        return newState;
    }

    /// <summary>
    /// Return true if a boolean changed from false to true
    /// </summary>
    public static bool WasReleased(bool newState, ref bool current)
    {
        if (newState == current)
        {
            return false;
        }

        current = newState;
        return !newState;
    }

    /// <summary>
    /// Checks for NaN or Infinity, and sets the float to the provided default value if either.
    /// </summary>
    /// <returns>True if NaN or Infinity</returns>
    public static bool ApplyDefaultIfInvalid(ref float val, float defaultValue)
    {
        var isInvalid = float.IsNaN(val) || float.IsInfinity(val);
        val = isInvalid ? defaultValue : val;
        return isInvalid;
    }

    public static bool ApplyDefaultIfInvalid(ref Vector2 val, Vector2 defaultValue)
    {
        var isInvalid = float.IsNaN(val.X) || float.IsInfinity(val.X) ||
                        float.IsNaN(val.Y) || float.IsInfinity(val.Y);
        val = isInvalid ? defaultValue : val;
        return isInvalid;
    }

    public static bool ApplyDefaultIfInvalid(ref Vector3 val, Vector3 defaultValue)
    {
        var isInvalid = float.IsNaN(val.X) || float.IsInfinity(val.X) ||
                        float.IsNaN(val.Y) || float.IsInfinity(val.Y) ||
                        float.IsNaN(val.Z) || float.IsInfinity(val.Z);
        val = isInvalid ? defaultValue : val;
        return isInvalid;
    }

    /// <summary>
    /// Checks for NaN or Infinity, and sets the double to the provided default value if either.
    /// </summary>
    /// <returns>True if NaN or Infinity</returns>
    public static bool ApplyDefaultIfInvalid(ref double val, double defaultValue)
    {
        var isInvalid = double.IsNaN(val) || double.IsInfinity(val);
        val = isInvalid ? defaultValue : val;
        return isInvalid;
    }
}