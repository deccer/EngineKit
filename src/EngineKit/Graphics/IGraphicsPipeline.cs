namespace EngineKit.Graphics;

public interface IGraphicsPipeline : IPipeline
{
    void BindVertexBuffer(
        IVertexBuffer vertexBuffer,
        uint binding,
        uint offset);

    void BindIndexBuffer(IIndexBuffer indexBuffer);

    void DrawArrays(
        int vertexCount,
        int vertexOffset);

    void DrawArraysInstanced(
        int firstVertex,
        int vertexOffset,
        int instanceCount,
        uint instanceOffset);

    void DrawElements(
        int elementCount,
        int offset);

    void DrawElementsIndirect(
        IIndirectBuffer indirectBuffer,
        int indirectElementIndex);

    void DrawElementsInstanced(
        int elementCount,
        int elementOffset,
        int instanceCount);

    void DrawElementsInstancedBaseVertex(
        int elementCount,
        int elementOffset,
        int instanceCount,
        int baseVertex);

    void DrawElementsInstancedBaseVertexBaseInstance(
        int elementCount,
        int elementOffset,
        int instanceCount,
        int baseVertex,
        int baseInstance);

    void MultiDrawElementsIndirect(IIndirectBuffer indirectBuffer, int primitiveCount);

}