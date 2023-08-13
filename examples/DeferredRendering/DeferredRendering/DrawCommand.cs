using EngineKit.Mathematics;

namespace DeferredRendering;

public class DrawCommand
{
    public string Name { get; set; }

    public Matrix WorldMatrix { get; set; }

    public int IndexCount;

    public int IndexOffset;

    public int VertexOffset;
}