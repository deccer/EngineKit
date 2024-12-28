using System;

namespace EngineKit.Graphics;

public interface IRenderer : IDisposable
{
    bool Load();

    void Render(float deltaTime, float elapsedTime);

    void RenderUi(float deltaTime, float elapsedTime);
    
    void ResizeIfNecessary();
}
