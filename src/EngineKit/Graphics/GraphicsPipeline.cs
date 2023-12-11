using EngineKit.Extensions;
using EngineKit.Graphics.Shaders;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

public sealed class GraphicsPipeline : Pipeline, IGraphicsPipeline
{
    private readonly GraphicsPipelineDescriptor _graphicsPipelineDescriptor;
    private readonly IInputLayout _currentInputLayout;
    private IBuffer? _currentVertexBuffer;
    private IBuffer? _currentIndexBuffer;

    internal GraphicsPipeline(
        GraphicsPipelineDescriptor graphicsPipelineDescriptor,
        ShaderProgram shaderProgram,
        IInputLayout currentInputLayout)
    {
        _graphicsPipelineDescriptor = graphicsPipelineDescriptor;
        _currentInputLayout = currentInputLayout;
        ShaderProgram = shaderProgram;
        Label = graphicsPipelineDescriptor.PipelineProgramLabel;
    }

    public override void Bind()
    {
        base.Bind();
        _currentInputLayout.Bind();
    }

    public void BindAsVertexBuffer(IBuffer vertexBuffer,
        uint binding,
        int offset = Offset.Zero)
    {
        if (_currentVertexBuffer != vertexBuffer)
        {
            GL.VertexArrayVertexBuffer(
                _currentInputLayout.Id,
                binding,
                vertexBuffer.Id,
                offset,
                vertexBuffer.Stride);
            _currentVertexBuffer = vertexBuffer;
        }
    }

    public void BindAsIndexBuffer(IBuffer indexBuffer)
    {
        if (_currentIndexBuffer != indexBuffer)
        {
            GL.VertexArrayElementBuffer(
                _currentInputLayout.Id,
                indexBuffer.Id);
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
        IBuffer indirectBuffer,
        int indirectElementIndex = 0)
    {
        GL.BindBuffer(BufferTarget.DrawIndirectBuffer.ToGL(), indirectBuffer.Id);
        GL.DrawElementsIndirect(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            GL.IndexElementType.UnsignedInt,
            indirectElementIndex * indirectBuffer.Stride);
    }

    public void MultiDrawElementsIndirect(IBuffer drawIndirectBuffer, int drawCount)
    {
        GL.BindBuffer(BufferTarget.DrawIndirectBuffer.ToGL(), drawIndirectBuffer.Id);
        GL.MultiDrawElementsIndirect(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            GL.IndexElementType.UnsignedInt,
            nint.Zero,
            drawCount,
            drawIndirectBuffer.Stride);
    }

    public void MultiDrawElementsIndirectCount(
        IBuffer drawElementsIndirectBuffer,
        IBuffer drawCountBuffer,
        int maxDrawCount)
    {
        GL.BindBuffer(BufferTarget.DrawIndirectBuffer.ToGL(), drawElementsIndirectBuffer.Id);
        GL.BindBuffer(BufferTarget.ParameterBuffer.ToGL(), drawCountBuffer.Id);
        GL.MultiDrawElementsIndirectCount(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            GL.IndexElementType.UnsignedInt,
            nint.Zero,
            maxDrawCount,
            drawElementsIndirectBuffer.Stride);
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