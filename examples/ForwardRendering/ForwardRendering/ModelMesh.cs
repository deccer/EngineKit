using System.Numerics;

namespace ForwardRendering;

public struct ModelMesh
{
    public string MeshName;

    public int VertexOffset;

    public int VertexCount;

    public int IndexOffset;

    public int IndexCount;

    public Matrix4x4 WorldMatrix;

    public ulong TextureHandle;
}