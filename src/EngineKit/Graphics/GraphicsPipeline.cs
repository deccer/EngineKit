using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

public sealed class GraphicsPipeline : Pipeline, IGraphicsPipeline
{
private readonly GraphicsPipelineDescriptor _graphicsPipelineDescriptor;

    internal IInputLayout? CurrentInputLayout;
    private IVertexBuffer? _currentVertexBuffer;
    private IIndexBuffer? _currentIndexBuffer;

    internal GraphicsPipeline(GraphicsPipelineDescriptor graphicsPipelineDescriptor)
    {
        _graphicsPipelineDescriptor = graphicsPipelineDescriptor;
        ShaderProgram = new ShaderProgram(
            graphicsPipelineDescriptor.VertexShaderSource,
            graphicsPipelineDescriptor.FragmentShaderSource,
            graphicsPipelineDescriptor.PipelineProgramLabel);
    }

    public override void Dispose()
    {
        base.Dispose();
        CurrentInputLayout?.Dispose();
    }

    public override void Bind()
    {
        base.Bind();
        CurrentInputLayout!.Bind();
    }

    public void BindVertexBuffer(
        IVertexBuffer vertexBuffer,
        uint binding,
        uint offset)
    {
        if (_currentVertexBuffer != vertexBuffer)
        {
            _currentVertexBuffer = vertexBuffer;
            vertexBuffer.Bind(CurrentInputLayout!, binding, offset);
        }
    }

    public void BindIndexBuffer(IIndexBuffer indexBuffer)
    {
        if (_currentIndexBuffer != indexBuffer)
        {
            _currentIndexBuffer = indexBuffer;
            indexBuffer.Bind(CurrentInputLayout!);
        }
    }

    public void DrawArraysInstanced(
        int vertexCount,
        int vertexOffset,
        int instanceCount,
        uint instanceOffset)
    {
        GL.DrawArraysInstancedBaseInstance(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            vertexOffset,
            vertexCount,
            instanceCount,
            instanceOffset);
    }

    public void DrawArrays(int vertexCount, int vertexOffset)
    {
        GL.DrawArraysInstancedBaseInstance(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            vertexOffset,
            vertexCount,
            1,
            0);
    }

    public void DrawElements(int elementCount, int offset)
    {
        GL.DrawElements(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            elementCount,
            GL.IndexElementType.UnsignedInt,
            offset);
    }

    public void DrawElementsInstanced(int elementCount, int elementOffset, int instanceCount)
    {
        GL.DrawElementsInstanced(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            elementCount,
            GL.IndexElementType.UnsignedInt,
            elementOffset,
            instanceCount);
    }

    public void DrawElementsInstancedBaseVertex(
        int elementCount,
        int elementOffset,
        int instanceCount,
        int baseVertex)
    {
        GL.DrawElementsInstancedBaseVertex(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            elementCount,
            GL.IndexElementType.UnsignedInt,
            elementOffset,
            instanceCount,
            baseVertex);
    }

    public void DrawElementsInstancedBaseVertexBaseInstance(
        int elementCount,
        int elementOffset,
        int instanceCount,
        int baseVertex,
        int baseInstance)
    {
        GL.DrawElementsInstancedBaseVertexBaseInstance(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            elementCount,
            GL.IndexElementType.UnsignedInt,
            elementOffset,
            instanceCount,
            baseVertex,
            baseInstance);
    }

    public void DrawElementsIndirect(
        IIndirectBuffer indirectBuffer,
        int indirectElementIndex)
    {
        indirectBuffer.Bind();
        GL.DrawElementsIndirect(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            GL.IndexElementType.UnsignedInt,
            indirectElementIndex * indirectBuffer.Stride);
    }

    public void MultiDrawElementsIndirect(IIndirectBuffer indirectBuffer, int primitiveCount)
    {
        indirectBuffer.Bind();
        GL.MultiDrawElementsIndirect(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            GL.IndexElementType.UnsignedInt,
            nint.Zero,
            primitiveCount,
            indirectBuffer.Stride);
    }
}