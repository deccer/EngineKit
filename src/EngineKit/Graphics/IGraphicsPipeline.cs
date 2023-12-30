using System.Numerics;

namespace EngineKit.Graphics;

public interface IGraphicsPipeline : IPipeline
{
    void BindAsVertexBuffer(
        IBuffer vertexBuffer,
        uint binding,
        uint stride,
        int offset);

    void BindAsIndexBuffer(IBuffer indexBuffer);

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
    
    void DrawElementsIndirect(
        IBuffer drawIndirectBuffer,
        int indirectElementIndex= 0);

    void MultiDrawElementsIndirect(IBuffer drawIndirectBuffer, uint drawCount);

    void MultiDrawElementsIndirectCount(
        IBuffer drawElementsIndirectBuffer,
        IBuffer drawCountBuffer,
        uint maxDrawCount);

    void VertexUniform(int location, float value);

    void VertexUniform(int location, int value);

    void VertexUniform(int location, Vector3 value);

    void FragmentUniform(int location, float value);

    void FragmentUniform(int location, int value);
}