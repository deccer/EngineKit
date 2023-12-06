// Copyright (c) Amer Koleci and contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

// This file includes code based on code from https://github.com/microsoft/DirectXMath
// The original code is Copyright � Microsoft. All rights reserved. Licensed under the MIT License (MIT).

using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static EngineKit.Mathematics.Vector2Utilities;

namespace EngineKit.Mathematics.PackedVector;

/// <summary>
/// Packed vector type containing two 8 bit signed normalized integer components.
/// </summary>
/// <remarks>Equivalent of XMBYTEN2.</remarks>
[StructLayout(LayoutKind.Explicit)]
public readonly struct Byte2Normalized : IPackedVector<ushort>, IEquatable<Byte2Normalized>
{
    [FieldOffset(0)]
    private readonly ushort _packedValue;

    /// <summary>
    /// The X component of the vector.
    /// </summary>
    [FieldOffset(0)]
    public readonly sbyte X;

    /// <summary>
    /// The Y component of the vector.
    /// </summary>
    [FieldOffset(1)]
    public readonly sbyte Y;

    /// <summary>
    /// Initializes a new instance of the <see cref="Byte2Normalized"/> struct.
    /// </summary>
    /// <param name="packedValue">The packed value to assign.</param>
    public Byte2Normalized(ushort packedValue)
    {
        Unsafe.SkipInit(out this);

        _packedValue = packedValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Byte2Normalized"/> struct.
    /// </summary>
    /// <param name="x">The x value.</param>
    /// <param name="y">The y value.</param>
    public Byte2Normalized(sbyte x, sbyte y)
    {
        Unsafe.SkipInit(out this);

        X = x;
        Y = y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Byte2Normalized"/> struct.
    /// </summary>
    /// <param name="x">The x value.</param>
    /// <param name="y">The y value.</param>
    public Byte2Normalized(float x, float y)
    {
        Unsafe.SkipInit(out this);

        Vector2 vector = Vector2.Clamp(new Vector2(x, y), NegativeOne, Vector2.One);
        vector = Vector2.Multiply(vector, ByteMax);
        vector = Round(vector);

        X = (sbyte)vector.X;
        Y = (sbyte)vector.Y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Byte2Normalized"/> struct.
    /// </summary>
    /// <param name="vector">The <see cref="Vector2"/> containing X and Y value.</param>
    public Byte2Normalized(in Vector2 vector)
        : this(vector.X, vector.Y)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Byte2Normalized"/> struct.
    /// </summary>
    /// <param name="vector">The <see cref="Vector4"/> containing X and Y value.</param>
    public Byte2Normalized(in Vector4 vector)
        : this(vector.X, vector.Y)
    {
    }

    /// <summary>
    /// Gets the packed value.
    /// </summary>
    public ushort PackedValue => _packedValue;

    /// <summary>
    /// Expands the packed representation to a <see cref="Vector2"/>.
    /// </summary>
    public Vector2 ToVector2() => new(
        (X == -128) ? -1.0f : (X * (1.0f / 127.0f)),
        (Y == -128) ? -1.0f : (Y * (1.0f / 127.0f))
        );

    Vector4 IPackedVector.ToVector4()
    {
        Vector2 vector = ToVector2();
        return new Vector4(vector.X, vector.Y, 0.0f, 1.0f);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Byte2Normalized other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Byte2Normalized other) => PackedValue.Equals(other.PackedValue);

    /// <summary>
    /// Compares two <see cref="Byte2Normalized"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="Byte2Normalized"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="Byte2Normalized"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Byte2Normalized left, Byte2Normalized right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="Byte2Normalized"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="Byte2Normalized"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="Byte2Normalized"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Byte2Normalized left, Byte2Normalized right) => !left.Equals(right);

    /// <inheritdoc/>
    public override readonly int GetHashCode() => PackedValue.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => PackedValue.ToString("X8", CultureInfo.InvariantCulture);
}
