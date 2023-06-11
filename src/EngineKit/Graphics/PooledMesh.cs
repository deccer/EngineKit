namespace EngineKit.Graphics;

public readonly struct PooledMesh
{
    public readonly uint IndexCount;
    
    public readonly uint IndexOffset;
    
    public readonly int VertexCount;
    
    public readonly int VertexOffset;

    public readonly string? MaterialName;

    public PooledMesh(uint indexCount, uint indexOffset, int vertexCount, int vertexOffset, string? materialName)
    {
        IndexCount = indexCount;
        IndexOffset = indexOffset;
        VertexCount = vertexCount;
        VertexOffset = vertexOffset;
        MaterialName = materialName;
    }
}