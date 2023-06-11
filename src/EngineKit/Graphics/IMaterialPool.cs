using System;

namespace EngineKit.Graphics;

public interface IMaterialPool : IDisposable
{
    IShaderStorageBuffer MaterialBuffer { get; }
    
    PooledMaterial GetOrAdd(Material material);
}