using System.Numerics;
using System.Runtime.InteropServices;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct GpuMaterial
{
    public Vector4 BaseColorFactor;
    
    public Vector4 EmissiveFactor;

    public float MetallicFactor;

    public float RoughnessFactor;

    public float AlphaCutOff;

    public int AlphaMode;

    public ulong BaseColorTexture;

    public ulong NormalTexture;
    
    public ulong MetalnessRoughnessTexture;
    
    public ulong SpecularTexture;

    public ulong OcclusionTexture;

    public ulong EmissiveTexture;
}