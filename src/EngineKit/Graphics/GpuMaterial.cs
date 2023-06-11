using System.Runtime.InteropServices;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct GpuMaterial
{
    public Color4 BaseColor;

    public Color4 Emissive;

    public Int4 BaseColorTextureId;

    public Int4 NormalTextureId;

    public Int4 SpecularTextureId;

    public Int4 MetalnessRoughnessTextureId;
}