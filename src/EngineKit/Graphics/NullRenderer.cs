namespace EngineKit.Graphics;

internal sealed class NullRenderer : IRenderer
{
    public void Dispose()
    {
    }

    public bool Load()
    {
        return true;
    }

    public void Render(
        float deltaTime,
        float elapsedTime)
    {
    }

    public void RenderUi(
        float deltaTime,
        float elapsedTime)
    {
    }

    public void ResizeIfNecessary()
    {
    }
}
