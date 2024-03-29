using System.Numerics;
using System.Runtime.InteropServices;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct VertexPosition
{
    public static readonly unsafe uint Stride = (uint)sizeof(VertexPosition);
    
    public VertexPosition(Vector3 position)
    {
        Position = position;
    }

    public readonly Vector3 Position;
}