// Copyright (c) Amer Koleci and contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

// This file includes code based on code from https://github.com/microsoft/DirectXMath
// The original code is Copyright � Microsoft. All rights reserved. Licensed under the MIT License (MIT).

using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using static EngineKit.Mathematics.VectorUtilities;

namespace EngineKit.Mathematics.PackedVector;

/// <summary>
/// Packed vector type containing two 16-bit signed integer values.
/// </summary>
/// <remarks>Equivalent of XMSHORT2.</remarks>
[StructLayout(LayoutKind.Explicit)]
public readonly struct Short2 : IPackedVector<uint>, IEquatable<Short2>
{
    [FieldOffset(0)]
    private readonly uint _packedValue;

    /// <summary>
    /// The X component of the vector.
    /// </summary>
    [FieldOffset(0)]
    public readonly short X;

    /// <summary>
    /// The Y component of the vector.
    /// </summary>
    [FieldOffset(2)]
    public readonly short Y;

    /// <summary>
    /// Initializes a new instance of the <see cref="Short2"/> struct.
    /// </summary>
    /// <param name="packedValue">The packed value to assign.</param>
    public Short2(uint packedValue)
    {
        Unsafe.SkipInit(out this);

        _packedValue = packedValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Short2"/> struct.
    /// </summary>
    /// <param name="x">The x value.</param>
    /// <param name="y">The y value.</param>
    public Short2(short x, short y)
    {
        Unsafe.SkipInit(out this);

        X = x;
        Y = y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Short2"/> struct.
    /// </summary>
    /// <param name="x">The x value.</param>
    /// <param name="y">The y value.</param>
    public Short2(float x, float y)
    {
        Unsafe.SkipInit(out this);

        Vector128<float> vector = Vector128.Create(x, y, 0.0f, 0.0f);
        if (Sse41.IsSupported)
        {
            // Bounds check
            Vector128<float> result = Clamp(vector, ShortMin, ShortMax);
            // Convert to int with rounding
            Vector128<int> vInt = Sse2.ConvertToVector128Int32(result);
            // Pack the ints into shorts
            Vector128<short> vShort = Sse2.PackSignedSaturate(vInt, vInt);

            X = vShort.GetElement(0);
            Y = vShort.GetElement(1);
        }
        else if (AdvSimd.IsSupported)
        {
            Vector128<float> result = AdvSimd.Max(vector, ShortMin);
            result = AdvSimd.Min(result, AdvSimd.DuplicateToVector128(32767.0f));
            Vector128<int> vInt32 = AdvSimd.ConvertToInt32RoundToZero(result);
            Vector64<short> vInt16 = AdvSimd.ExtractNarrowingSaturateLower(vInt32);

            X = vInt16.GetElement(0);
            Y = vInt16.GetElement(1);
            //vst1_lane_u32(&pDestination->v, vreinterpret_u32_s16(vInt16), 0);
        }
        else
        {
            Vector128<float> result = Clamp(vector, ShortMin, ShortMax);
            vector = Round(vector);

            X = (short)vector.GetX();
            Y = (short)vector.GetY();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Short2"/> struct.
    /// </summary>
    /// <param name="vector">The <see cref="Vector2"/> containing X and Y value.</param>
    public Short2(Vector2 vector)
        : this(vector.X, vector.Y)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Short2"/> struct.
    /// </summary>
    /// <param name="vector">The <see cref="Vector4"/> containing X and Y value.</param>
    public Short2(Vector4 vector)
        : this(vector.X, vector.Y)
    {
    }

    /// <summary>
    /// Gets the packed value.
    /// </summary>
    public uint PackedValue => _packedValue;

    /// <summary>
    /// Expands the packed representation to a <see cref="Vector2"/>.
    /// </summary>
    public Vector2 ToVector2() => new(X, Y);

    Vector4 IPackedVector.ToVector4()
    {
        Vector2 vector = ToVector2();
        return new Vector4(vector.X, vector.Y, 0.0f, 1.0f);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Short2 other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Short2 other) => PackedValue.Equals(other.PackedValue);

    /// <summary>
    /// Compares two <see cref="Short2"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="Short2"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="Short2"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Short2 left, Short2 right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="Short2"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="Short2"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="Short2"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Short2 left, Short2 right) => !left.Equals(right);

    /// <inheritdoc/>
    public override int GetHashCode() => PackedValue.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => PackedValue.ToString("X8", CultureInfo.InvariantCulture);
}
