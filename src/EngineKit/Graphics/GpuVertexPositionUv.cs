using System.Numerics;
using System.Runtime.InteropServices;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct GpuVertexPositionUv
{
    public static readonly unsafe uint Stride = (uint)sizeof(GpuVertexPositionUv);

    public GpuVertexPositionUv(Vector3 position, Vector2 uv)
    {
        Position = position;
        Uv = uv;
    }

    public readonly Vector3 Position;

    public readonly Vector2 Uv;
}
