using OpenTK.Mathematics;

namespace DeferredRendering;

public struct GpuMaterial
{
    public Color4 BaseColor;

    public ulong BaseColorTexture;
}