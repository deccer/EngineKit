using System.Runtime.InteropServices;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct GpuLightInformation
{
    public Vector4 DirectionalLightPosition { get; set; }

    public Vector4 DirectionalLightColor { get; set; }

    public Int4 LightCount { get; set; }
}