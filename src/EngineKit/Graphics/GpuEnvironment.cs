using System.Runtime.InteropServices;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct GpuEnvironment
{
    public Matrix DirectionalLightViewMatrix;

    public Matrix DirectionalLightProjectionMatrix;

    public Vector4 DirectionalLightColor;

    public Vector4 DirectionalLightDirection;
}