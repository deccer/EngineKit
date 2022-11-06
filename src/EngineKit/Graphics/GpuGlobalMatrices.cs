using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct GpuGlobalMatrices
{
    public Matrix4 CameraToClipMatrix;

    public Matrix4 WorldToCameraMatrix;

    public Vector4i DrawIndex;
}