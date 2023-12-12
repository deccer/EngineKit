using System;

namespace EngineKit.Graphics;

public interface IMeshPool : IDisposable
{
    IBuffer VertexBuffer { get; }
    
    IBuffer IndexBuffer { get; }

    MeshId GetOrAdd(MeshPrimitive meshPrimitive);
}