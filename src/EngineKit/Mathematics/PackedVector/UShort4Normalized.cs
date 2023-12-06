// Copyright (c) Amer Koleci and contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static EngineKit.Mathematics.Vector4Utilities;

namespace EngineKit.Mathematics.PackedVector;

/// <summary>
/// Packed vector type containing four 16-bit unsigned normalized integer components.
/// </summary>
/// <remarks>Equivalent of XMUSHORTN4.</remarks>
[StructLayout(LayoutKind.Explicit)]
public readonly struct UShort4Normalized : IPackedVector<ulong>, IEquatable<UShort4Normalized>
{
    [FieldOffset(0)]
    private readonly ulong _packedValue;

    /// <summary>
    /// The X component of the vector.
    /// </summary>
    [FieldOffset(0)]
    public readonly ushort X;

    /// <summary>
    /// The Y component of the vector.
    /// </summary>
    [FieldOffset(2)]
    public readonly ushort Y;

    /// <summary>
    /// The Z component of the vector.
    /// </summary>
    [FieldOffset(4)]
    public readonly ushort Z;

    /// <summary>
    /// The W component of the vector.
    /// </summary>
    [FieldOffset(6)]
    public readonly ushort W;

    /// <summary>
    /// Initializes a new instance of the <see cref="UShort4Normalized"/> struct.
    /// </summary>
    /// <param name="packedValue">The packed value to assign.</param>
    public UShort4Normalized(ulong packedValue)
    {
        Unsafe.SkipInit(out this);

        _packedValue = packedValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UShort4Normalized"/> struct.
    /// </summary>
    /// <param name="x">The x value.</param>
    /// <param name="y">The y value.</param>
    /// <param name="z">The z value.</param>
    /// <param name="w">The w value.</param>
    public UShort4Normalized(ushort x, ushort y, ushort z, ushort w)
    {
        Unsafe.SkipInit(out this);

        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UShort4Normalized"/> struct.
    /// </summary>
    /// <param name="x">The x value.</param>
    /// <param name="y">The y value.</param>
    /// <param name="z">The z value.</param>
    /// <param name="w">The w value.</param>
    public UShort4Normalized(float x, float y, float z, float w)
    {
        Unsafe.SkipInit(out this);

        Vector4 vector = Saturate(new Vector4(x, y, z, w));
        vector = MultiplyAdd(vector, UShortMax, OneHalf);
        vector = Truncate(vector);

        X = (ushort)vector.X;
        Y = (ushort)vector.Y;
        Z = (ushort)vector.Z;
        W = (ushort)vector.W;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UShort4Normalized"/> struct.
    /// </summary>
    /// <param name="vector">The <see cref="Vector4"/> containing X, Y, Z and W value.</param>
    public UShort4Normalized(in Vector4 vector)
        : this(vector.X, vector.Y, vector.Z, vector.W)
    {
    }

    /// <summary>
    /// Gets the packed value.
    /// </summary>
    public ulong PackedValue => _packedValue;

    /// <summary>
    /// Expands the packed representation to a <see cref="Vector4"/>.
    /// </summary>
    public Vector4 ToVector4() => new(
        X / 65535.0f,
        Y / 65535.0f,
        Z / 65535.0f,
        W / 65535.0f
        );


    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is UShort4Normalized other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(UShort4Normalized other) => PackedValue.Equals(other.PackedValue);

    /// <summary>
    /// Compares two <see cref="UShort4Normalized"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="UShort4Normalized"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="UShort4Normalized"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(UShort4Normalized left, UShort4Normalized right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="UShort4Normalized"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="UShort4Normalized"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="UShort4Normalized"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(UShort4Normalized left, UShort4Normalized right) => !left.Equals(right);

    /// <inheritdoc/>
    public override int GetHashCode() => PackedValue.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => PackedValue.ToString("X16", CultureInfo.InvariantCulture);
}
