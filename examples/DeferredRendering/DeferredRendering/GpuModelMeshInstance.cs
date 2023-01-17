using OpenTK.Mathematics;

namespace DeferredRendering;

public struct GpuModelMeshInstance
{
    public Matrix4 WorldMatrix;

    public Vector4i MaterialId;
}