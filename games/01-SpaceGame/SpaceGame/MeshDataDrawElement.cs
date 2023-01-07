namespace SpaceGame;

public readonly struct MeshDataDrawElement
{
    public MeshDataDrawElement(int indexOffset, int indexCount, int vertexOffset, int vertexCount)
    {
        IndexOffset = indexOffset;
        IndexCount = indexCount;
        VertexOffset = vertexOffset;
        VertexCount = vertexCount;
    }
    
    public readonly int IndexOffset;
    
    public readonly int IndexCount;
    
    public readonly int VertexOffset;
    
    public readonly int VertexCount;
}