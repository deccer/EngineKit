namespace EngineKit.Graphics;

public interface IIndexBuffer : IBuffer
{
    void Bind(IInputLayout inputLayout);
}