using System.Numerics;
using System.Runtime.InteropServices;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct GpuVertexPositionColorUv
{
    public static readonly unsafe uint Stride = (uint)sizeof(GpuVertexPositionColorUv);
    
    public GpuVertexPositionColorUv(Vector3 position, Vector3 color, Vector2 uv)
    {
        Position = position;
        Color = color;
        Uv = uv;
    }

    public readonly Vector3 Position;

    public readonly Vector3 Color;

    public readonly Vector2 Uv;
}