using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct GpuInstanceData
{
    public Matrix4 ModelToWorldMatrix;

    public Vector4i MaterialIndex;
}