using System.Numerics;
using System.Runtime.InteropServices;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential)]
public readonly struct GpuVertexPositionColor
{
    public static readonly unsafe uint Stride = (uint)sizeof(GpuVertexPositionColor);
    
    public GpuVertexPositionColor(Vector3 position, Vector3 color)
    {
        Position = position;
        Color = color;
    }

    public readonly Vector3 Position;

    public readonly Vector3 Color;
}