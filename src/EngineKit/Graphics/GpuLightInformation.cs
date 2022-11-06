using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct GpuLightInformation
{
    public Vector4 DirectionalLightPosition { get; set; }

    public Vector4 DirectionalLightColor { get; set; }

    public Vector4i LightCount { get; set; }
}