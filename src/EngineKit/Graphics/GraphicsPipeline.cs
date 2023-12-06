using EngineKit.Extensions;
using EngineKit.Graphics.Shaders;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

public sealed class GraphicsPipeline : Pipeline, IGraphicsPipeline
{
    internal IInputLayout? CurrentInputLayout;

    private readonly GraphicsPipelineDescriptor _graphicsPipelineDescriptor;
    private IVertexBuffer? _currentVertexBuffer;
    private IIndexBuffer? _currentIndexBuffer;

    internal GraphicsPipeline(GraphicsPipelineDescriptor graphicsPipelineDescriptor, ShaderProgram shaderProgram)
    {
        _graphicsPipelineDescriptor = graphicsPipelineDescriptor;
        ShaderProgram = shaderProgram;
        Label = graphicsPipelineDescriptor.PipelineProgramLabel;
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
        IVertexBuffer? vertexBuffer,
        uint binding,
        uint offset)
    {
        if (_currentVertexBuffer != vertexBuffer)
        {
            vertexBuffer?.Bind(CurrentInputLayout!, binding, offset);
            _currentVertexBuffer = vertexBuffer;
        }
    }

    public void BindIndexBuffer(IIndexBuffer? indexBuffer)
    {
        if (_currentIndexBuffer != indexBuffer)
        {
            indexBuffer?.Bind(CurrentInputLayout!);
            _currentIndexBuffer = indexBuffer;
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

    public void DrawArrays(uint vertexCount, int vertexOffset = 0)
    {
        GL.DrawArrays(_graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(), 0, vertexCount);
    }

    public void DrawElements(int elementCount, int offset = 0)
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
            elementOffset * sizeof(uint),
            instanceCount,
            baseVertex,
            baseInstance);
    }

    public void DrawElementsIndirect(
        IIndirectBuffer indirectBuffer,
        int indirectElementIndex = 0)
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
    
    public void VertexUniform(int location, float value)
    {
        GL.ProgramUniform(ShaderProgram.VertexShader.Id, location, value);
    }
    
    public void VertexUniform(int location, int value)
    {
        GL.ProgramUniform(ShaderProgram.VertexShader.Id, location, value);
    }
    
    public void FragmentUniform(int location, float value)
    {
        GL.ProgramUniform(ShaderProgram.FragmentShader.Id, location, value);
    }
    
    public void FragmentUniform(int location, int value)
    {
        GL.ProgramUniform(ShaderProgram.FragmentShader.Id, location, value);
    }
}