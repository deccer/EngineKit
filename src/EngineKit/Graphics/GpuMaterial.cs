using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct GpuMaterial
{
    public Color4 Diffuse { get; set; }

    public Color4 Emissive { get; set; }

    public Vector4i BaseColorTextureId { get; set; }

    public Vector4i NormalTextureId { get; set; }
}