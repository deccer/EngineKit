using System.Numerics;
using System.Runtime.InteropServices;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct GpuVertexPosition
{
    public static readonly unsafe uint Stride = (uint)sizeof(GpuVertexPosition);
    
    public GpuVertexPosition(Vector3 position)
    {
        Position = position;
    }

    public readonly Vector3 Position;
}