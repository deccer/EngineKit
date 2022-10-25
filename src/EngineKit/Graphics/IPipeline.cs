using System;

namespace EngineKit.Graphics;

public interface IPipeline : IDisposable
{
    void Bind();

    void BindUniformBuffer(
        IUniformBuffer buffer,
        uint bindingIndex);

    void BindShaderStorageBuffer(
        IShaderStorageBuffer buffer,
        uint bindingIndex);
}