namespace EngineKit.Graphics;

public interface IUniformBuffer : IBuffer
{
    void Bind(uint bindingIndex);
}