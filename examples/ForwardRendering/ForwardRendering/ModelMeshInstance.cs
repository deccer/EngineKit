using System.Numerics;

namespace ForwardRendering;

public struct ModelMeshInstance
{
    public ModelMesh ModelMesh;

    public Matrix4x4 World;
}