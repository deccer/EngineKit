using System;

namespace EngineKit.Graphics;

public interface IUIRenderer : IUIRendererLoader, IDisposable
{
    void WindowResized(int width, int height);

    void BeginLayout();

    void EndLayout();

    void Update(float deltaSeconds);
}