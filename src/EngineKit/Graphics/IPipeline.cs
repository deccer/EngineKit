namespace EngineKit.Graphics;

public interface IPipeline
{
    void Bind();

    void BindUniformBuffer(
        IUniformBuffer buffer,
        uint bindingIndex);

    void BindShaderStorageBuffer(
        IShaderStorageBuffer buffer,
        uint bindingIndex);
}