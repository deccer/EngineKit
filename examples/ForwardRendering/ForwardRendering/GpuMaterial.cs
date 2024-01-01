using System.Numerics;
using EngineKit.Mathematics;

namespace ForwardRendering;

public struct GpuMaterial
{
    public Vector4 BaseColor;

    public ulong BaseColorTextureHandle;
    // ReSharper disable once InconsistentNaming
    public Int2 _padding;
}