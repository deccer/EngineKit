using System.Numerics;

namespace DeferredRendering;

public class DrawCommand
{
    public string? Name { get; init; }

    public Matrix4x4 WorldMatrix { get; init; }

    public int IndexCount;

    public int IndexOffset;

    public int VertexOffset;
}