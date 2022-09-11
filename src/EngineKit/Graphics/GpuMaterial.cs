using System.Runtime.InteropServices;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct GpuMaterial
{
    public Vector4 Diffuse { get; set; }

    public Vector4 Emissive { get; set; }

    public Int4 BaseColorTextureId { get; set; }

    public Int4 NormalTextureId { get; set; }
}