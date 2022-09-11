using System.Runtime.InteropServices;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Explicit)]
public record struct ClearColorValue
{
    [FieldOffset(0)] public float[] ColorFloat;

    [FieldOffset(0)] public uint[] ColorUnsignedInteger;

    [FieldOffset(0)] public int[] ColorSignedInteger;
}