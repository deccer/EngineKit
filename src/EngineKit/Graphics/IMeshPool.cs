using System;

namespace EngineKit.Graphics;

public interface IMeshPool : IDisposable
{
    IVertexBuffer VertexBuffer { get; }
    
    IIndexBuffer IndexBuffer { get; }

    PooledMesh GetOrAdd(MeshPrimitive meshPrimitive);
}