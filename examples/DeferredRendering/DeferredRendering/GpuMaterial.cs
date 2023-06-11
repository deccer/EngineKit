using EngineKit.Mathematics;

namespace DeferredRendering;

public struct GpuMaterial
{
    public Color4 BaseColor;

    public ulong BaseColorTexture;

    public Point _padding;
}