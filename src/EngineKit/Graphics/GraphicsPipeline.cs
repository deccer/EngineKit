using System;
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
        int firstVertex,
        int instanceCount,
        uint firstInstance)
    {
        GL.DrawArraysInstancedBaseInstance(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            firstVertex,
            vertexCount,
            instanceCount,
            firstInstance);
    }

    public void DrawArrays(int vertexCount, int firstVertex)
    {
        GL.DrawArraysInstancedBaseInstance(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            firstVertex,
            vertexCount,
            1,
            0);
    }

    public void DrawElements(int indexCount, int offset)
    {
        GL.DrawElements(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            indexCount,
            GL.IndexElementType.UnsignedInt,
            (nint)offset);
    }

    public void DrawElementsInstanced(int indexCount, int offset, int instanceCount)
    {
        GL.DrawElementsInstanced(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            indexCount,
            GL.IndexElementType.UnsignedInt,
            (nint)offset,
            instanceCount);
    }

    public void DrawElementsIndirect(
        IIndirectBuffer indirectBuffer,
        int indirectElementIndex)
    {
        indirectBuffer.Bind();
        GL.DrawElementsIndirect(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            GL.IndexElementType.UnsignedInt,
            new IntPtr(indirectElementIndex * indirectBuffer.Stride));
    }

    public void MultiDrawElementsIndirect(IIndirectBuffer indirectBuffer)
    {
        indirectBuffer.Bind();
        GL.MultiDrawElementsIndirect(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            GL.IndexElementType.UnsignedInt,
            IntPtr.Zero,
            indirectBuffer.Count,
            indirectBuffer.Stride);
    }
}