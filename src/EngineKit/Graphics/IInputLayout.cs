using System;

namespace EngineKit.Graphics;

public interface IInputLayout : IDisposable
{
    uint Id { get; }

    void Bind();
}