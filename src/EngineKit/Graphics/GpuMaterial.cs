using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct GpuMaterial
{
    public Color4 BaseColor;

    public Color4 Emissive;

    public Vector4i BaseColorTextureId;

    public Vector4i NormalTextureId;

    public Vector4i SpecularTextureId;

    public Vector4i MetalnessRoughnessTextureId;
}