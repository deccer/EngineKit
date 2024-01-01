using System;
using System.Numerics;

namespace EngineKit.Graphics;

public interface IUIRenderer : IUIRendererLoader, IDisposable
{
    void WindowResized(int width, int height);

    void BeginLayout();

    void EndLayout();

    void ShowDemoWindow();

    void Update(float deltaSeconds);

    bool AddFont(string name, string filePath, float fontSize);
    
    void CharacterInput(char keyChar);
    
    void MouseScroll(Vector2 offset);
}