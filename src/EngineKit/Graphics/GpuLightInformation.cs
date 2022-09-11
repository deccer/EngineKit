using System.Numerics;
using System.Runtime.InteropServices;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct GpuLightInformation
{
    public Vector4 DirectionalLightPosition { get; set; }

    public Vector4 DirectionalLightColor { get; set; }
}