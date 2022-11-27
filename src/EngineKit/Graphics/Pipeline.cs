using CSharpFunctionalExtensions;
using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

public abstract class Pipeline : IPipeline
{
    protected ShaderProgram? ShaderProgram;        

    public virtual void Bind()
    {
        ShaderProgram?.Use();
    }

    public virtual void Dispose()
    {
        ShaderProgram?.Dispose();
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

    public void BindUniformBuffer(
        IUniformBuffer buffer,
        uint bindingIndex)
    {
        buffer.Bind(bindingIndex);
    }

    public void BindShaderStorageBuffer(
        IShaderStorageBuffer buffer,
        uint bindingIndex)
    {
        buffer.Bind(bindingIndex);
    }
    
    public void BindTexture(
        ITexture texture,
        uint bindingIndex)
    {
        GL.BindTextureUnit(bindingIndex, texture.Id);
    }
    
    public void BindSampledTexture(ISampler sampler, ITexture texture, uint bindingIndex)
    {
        GL.BindTextureUnit(bindingIndex, texture.Id);
        GL.BindSampler(bindingIndex, sampler.Id);
    }

    public void BindSampledTexture(ISampler sampler, uint textureId, uint bindingIndex)
    {
        GL.BindTextureUnit(bindingIndex, textureId);
        GL.BindSampler(bindingIndex, sampler.Id);
    }
    

    internal Result LinkPrograms()
    {
        if (ShaderProgram is null)
        {
            return Result.Failure("No shader program available");
        }

        var linkResult = ShaderProgram.Link();

        return linkResult.IsFailure
            ? linkResult
            : Result.Success();
    }
}