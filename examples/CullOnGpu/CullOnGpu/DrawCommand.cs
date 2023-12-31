using System.Numerics;

namespace CullOnGpu;

public class DrawCommand
{
    public string? Name { get; init; }

    public Matrix4x4 WorldMatrix { get; init; }
    public int MaterialIndex { get; set; }

    public int IndexCount;

    public int IndexOffset;

    public int VertexOffset;
}