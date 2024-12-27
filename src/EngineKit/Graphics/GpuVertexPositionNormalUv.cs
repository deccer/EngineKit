using System.Numerics;
using System.Runtime.InteropServices;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct GpuVertexPositionNormalUv
{
    public static readonly unsafe uint Stride = (uint)sizeof(GpuVertexPositionNormalUv);
    
    public GpuVertexPositionNormalUv(
        Vector3 position,
        Vector3 normal,
        Vector2 uv)
    {
        Position = position;
        Normal = normal;
        Uv = uv;
    }

    public readonly Vector3 Position;

    public readonly Vector3 Normal;

    public readonly Vector2 Uv;
}