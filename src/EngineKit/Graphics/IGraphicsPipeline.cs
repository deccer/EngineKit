namespace EngineKit.Graphics;

public interface IGraphicsPipeline : IPipeline
{
    void BindVertexBuffer(
        IVertexBuffer? vertexBuffer,
        uint binding,
        uint offset);

    void BindIndexBuffer(IIndexBuffer? indexBuffer);

    void DrawArrays(
        uint vertexCount,
        int vertexOffset = 0);

    void DrawArraysInstanced(
        int firstVertex,
        int vertexOffset,
        int instanceCount,
        uint instanceOffset);

    void DrawElements(
        int elementCount,
        int offset = 0);

    void DrawElementsIndirect(
        IIndirectBuffer indirectBuffer,
        int indirectElementIndex= 0);

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