using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct GpuEnvironment
{
    public Matrix4 DirectionalLightViewMatrix;

    public Matrix4 DirectionalLightProjectionMatrix;

    public Vector4 DirectionalLightColor;

    public Vector4 DirectionalLightDirection;
}