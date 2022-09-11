using System.Runtime.InteropServices;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct GpuInstanceData
{
    public Matrix ModelToWorldMatrix;

    public Int4 MaterialIndex;
}