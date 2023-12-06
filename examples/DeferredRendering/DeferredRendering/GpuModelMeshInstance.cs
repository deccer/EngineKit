using System.Numerics;
using EngineKit.Mathematics;

namespace DeferredRendering;

public struct GpuModelMeshInstance
{
    public Matrix4x4 WorldMatrix;

    public Int4 MaterialId;
}