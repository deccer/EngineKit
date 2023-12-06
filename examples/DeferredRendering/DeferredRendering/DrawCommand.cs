using EngineKit.Mathematics;

namespace DeferredRendering;

public class DrawCommand
{
    public string Name { get; init; }

    public Matrix WorldMatrix { get; init; }

    public int IndexCount;

    public int IndexOffset;

    public int VertexOffset;
}