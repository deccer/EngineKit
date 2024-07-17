using System;

namespace EngineKit.Graphics;

public interface IMeshPool : IDisposable
{
    IBuffer VertexBuffer { get; }

    IBuffer IndexBuffer { get; }

    uint VertexBufferStride { get; }

    PooledMesh GetOrAdd(MeshPrimitive meshPrimitive);
}
