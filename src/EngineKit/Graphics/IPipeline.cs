using System;

namespace EngineKit.Graphics;

public interface IPipeline : IDisposable
{
    void Bind();

    void BindImage(
        ITexture texture,
        uint unit,
        int level,
        MemoryAccess memoryAccess,
        Format format);
    
    void BindImage(
        ITexture texture,
        uint unit,
        int level,
        int layer,
        MemoryAccess memoryAccess,
        Format format);
    
    void BindSampledTexture(
        ISampler sampler,
        ITexture texture,
        uint bindingIndex);

    void BindSampledTexture(
        ISampler sampler,
        uint textureId,
        uint bindingIndex);

    void BindUniformBuffer(
        IUniformBuffer buffer,
        uint bindingIndex);

    void BindShaderStorageBuffer(
        IShaderStorageBuffer buffer,
        uint bindingIndex);

    void BindTexture(ITexture texture, uint bindingIndex);
}