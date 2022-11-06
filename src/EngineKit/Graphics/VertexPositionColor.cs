using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct VertexPositionColor
{
    public VertexPositionColor(Vector3 position, Vector3 color)
    {
        Position = position;
        Color = color;
    }

    public readonly Vector3 Position;

    public readonly Vector3 Color;
}