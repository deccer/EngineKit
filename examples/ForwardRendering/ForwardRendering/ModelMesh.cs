using EngineKit.Mathematics;

namespace ForwardRendering;

public struct ModelMesh
{
    public string MeshName;

    public int VertexOffset;

    public int VertexCount;

    public int IndexOffset;

    public int IndexCount;

    public Matrix WorldMatrix;

    public ulong TextureHandle;
}