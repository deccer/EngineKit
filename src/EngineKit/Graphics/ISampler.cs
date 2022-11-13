using System;

namespace EngineKit.Graphics;

public interface ISampler : IDisposable
{
    uint Id { get; }
}