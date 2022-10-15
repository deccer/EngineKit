using System;
using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

public sealed class GraphicsPipeline : Pipeline, IGraphicsPipeline
{
private readonly GraphicsPipelineDescriptor _graphicsPipelineDescriptor;

    internal IInputLayout? CurrentInputLayout;

    internal GraphicsPipeline(GraphicsPipelineDescriptor graphicsPipelineDescriptor)
    {
        _graphicsPipelineDescriptor = graphicsPipelineDescriptor;
        ShaderProgram = new ShaderProgram(
            graphicsPipelineDescriptor.VertexShaderFilePath,
            graphicsPipelineDescriptor.FragmentShaderFilePath,
            graphicsPipelineDescriptor.PipelineProgramLabel);
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
        vertexBuffer.Bind(CurrentInputLayout!, binding, offset);
    }

    public void BindIndexBuffer(IIndexBuffer indexBuffer)
    {
        indexBuffer.Bind(CurrentInputLayout!);
    }

    public void BindInstanceBuffer(IShaderStorageBuffer shaderStorageBuffer, uint bindingIndex)
    {
        shaderStorageBuffer.Bind(bindingIndex);
    }

    public void BindSampledTexture(Sampler sampler, ITexture texture, uint bindingIndex)
    {
        GL.BindTextureUnit(bindingIndex, texture.Id);
        GL.BindSampler(bindingIndex, sampler.Id);
    }

    public void BindSampledTexture(Sampler sampler, uint textureId, uint bindingIndex)
    {
        GL.BindTextureUnit(bindingIndex, textureId);
        GL.BindSampler(bindingIndex, sampler.Id);
    }

    public void DrawArraysInstanced(
        int firstVertex,
        int vertexCount,
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
            offset);
    }

    public void DrawElementsInstanced(int indexCount, int offset, int instanceCount)
    {
        GL.DrawElementsInstanced(
            _graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology.ToGL(),
            indexCount,
            GL.IndexElementType.UnsignedInt,
            offset,
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