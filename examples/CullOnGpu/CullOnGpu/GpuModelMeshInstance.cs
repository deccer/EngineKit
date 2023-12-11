using System.Numerics;
using EngineKit.Mathematics;

namespace CullOnGpu;

public struct GpuModelMeshInstance
{
    public Matrix4x4 WorldMatrix;

    public Int4 MaterialId;
}