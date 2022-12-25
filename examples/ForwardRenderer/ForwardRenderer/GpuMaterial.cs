using OpenTK.Mathematics;

namespace ForwardRenderer;

public struct GpuMaterial
{
    public Vector4 BaseColor;

    public ulong BaseColorTextureHandle;
    public Vector2i _padding;
}