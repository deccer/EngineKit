using System;
using EngineKit.Extensions;
using EngineKit.Graphics.Shaders;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

public abstract class Pipeline : IPipeline
{
    protected ShaderProgram ShaderProgram;
    
    public Label Label { get; protected set; }

    public virtual void Bind()
    {
        ShaderProgram.Use();
    }

    public virtual void Dispose()
    {
        ShaderProgram.Dispose();
        GC.SuppressFinalize(this);
    }

    public void BindImage(
        ITexture texture,
        uint unit,
        int level,
        MemoryAccess memoryAccess,
        Format format)
    {
        GL.BindImageTexture(unit, (int)texture.Id, level, false, 0, memoryAccess.ToGL(), format.ToGL());
    }

    public void BindImage(
        ITexture texture,
        uint unit,
        int level,
        int layer,
        MemoryAccess memoryAccess,
        Format format)
    {
        GL.BindImageTexture(unit, (int)texture.Id, level, true, layer, memoryAccess.ToGL(), format.ToGL());
    }

    public void BindAsUniformBuffer(
        IBuffer uniformBuffer,
        uint bindingIndex)
    {
        GL.BindBufferBase(BufferTarget.UniformBuffer.ToGL(), bindingIndex, uniformBuffer.Id);
    }

    public void BindAsShaderStorageBuffer(
        IBuffer shaderStorageBuffer,
        uint bindingIndex)
    {
        GL.BindBufferBase(BufferTarget.ShaderStorageBuffer.ToGL(), bindingIndex, shaderStorageBuffer.Id);
    }

    public void BindTexture(
        ITexture texture,
        uint bindingIndex)
    {
        GL.BindTextureUnit(bindingIndex, texture.Id);
    }

    public void BindSampledTexture(
        ISampler sampler,
        ITexture texture,
        uint bindingIndex)
    {
        GL.BindTextureUnit(bindingIndex, texture.Id);
        GL.BindSampler(bindingIndex, sampler.Id);
    }

    public void BindSampledTexture(
        ISampler sampler,
        uint textureId,
        uint bindingIndex)
    {
        GL.BindTextureUnit(bindingIndex, textureId);
        GL.BindSampler(bindingIndex, sampler.Id);
    }
}