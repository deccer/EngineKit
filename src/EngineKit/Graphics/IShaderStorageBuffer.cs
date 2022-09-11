namespace EngineKit.Graphics;

public interface IShaderStorageBuffer : IBuffer
{
    void Bind(uint bindingIndex);
}