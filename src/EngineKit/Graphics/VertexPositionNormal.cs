using System.Numerics;
using System.Runtime.InteropServices;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct VertexPositionNormal
{
    public VertexPositionNormal(Vector3 position, Vector3 normal)
    {
        Position = position;
        Normal = normal;
    }

    public readonly Vector3 Position;

    public readonly Vector3 Normal;
}