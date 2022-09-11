namespace EngineKit.Graphics;

public interface IMeshFactory
{
    IVertexBuffer CreateVertexBuffer(
        MeshData[] meshDates,
        VertexType targetVertexType);

    IIndexBuffer CreateIndexBuffer(MeshData[] meshDates);
}