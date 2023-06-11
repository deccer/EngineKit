using EngineKit.Mathematics;

namespace ForwardRendering;

public struct GpuMaterial
{
    public Vector4 BaseColor;

    public ulong BaseColorTextureHandle;
    public Point _padding;
}