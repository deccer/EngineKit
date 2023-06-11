using System.Runtime.InteropServices;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct GpuGlobalMatrices
{
    public Matrix CameraToClipMatrix;

    public Matrix WorldToCameraMatrix;

    public Int4 DrawIndex;
}