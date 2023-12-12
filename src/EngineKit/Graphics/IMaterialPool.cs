using System;

namespace EngineKit.Graphics;

public interface IMaterialPool : IDisposable
{
    IBuffer MaterialBuffer { get; }
    
    MaterialId GetOrAdd(Material material);
}