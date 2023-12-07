using System;

namespace EngineKit.Graphics;

public interface IMaterialPool : IDisposable
{
    IBuffer MaterialBuffer { get; }
    
    PooledMaterial GetOrAdd(Material material);
}