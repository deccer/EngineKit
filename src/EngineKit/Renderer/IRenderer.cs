using System;

namespace EngineKit.Renderer;

public interface IRenderer : IDisposable
{
    bool Load();
}