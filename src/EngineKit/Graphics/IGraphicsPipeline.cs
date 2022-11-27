namespace EngineKit.Graphics;

public interface IGraphicsPipeline : IPipeline
{
    void BindVertexBuffer(
        IVertexBuffer vertexBuffer,
        uint binding,
        uint offset);

    void BindIndexBuffer(IIndexBuffer indexBuffer);

    void DrawElementsIndirect(
        IIndirectBuffer indirectBuffer,
        int indirectElementIndex);

    void DrawArraysInstanced(
        int firstVertex,
        int vertexCount,
        int instanceCount,
        uint firstInstance);

    void DrawArrays(
        int vertexCount,
        int firstVertex);

    void DrawElements(
        int indexCount,
        int offset);

    void DrawElementsInstanced(
        int indexCount,
        int offset,
        int instanceCount);

    void MultiDrawElementsIndirect(IIndirectBuffer indirectBuffer);
}