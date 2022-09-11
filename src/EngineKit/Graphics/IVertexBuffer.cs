namespace EngineKit.Graphics;

public interface IVertexBuffer : IBuffer
{
    void Bind(IInputLayout inputLayout, uint bindingIndex);

    void Bind(IInputLayout inputLayout, uint bindingIndex, uint offset);
}