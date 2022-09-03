using System;

namespace EngineKit;

public interface IApplication : IDisposable
{
    void Run();
}