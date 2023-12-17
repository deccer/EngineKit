using System.Collections.Generic;
using System.Linq;

namespace EngineKit.Graphics;

internal sealed class MeshPool : IMeshPool
{
    private readonly IDictionary<MeshPrimitive, MeshId> _pooledMeshes;

    public MeshPool(Label label, IGraphicsContext graphicsContext, int vertexBufferCapacity, int indexBufferCapacity)
    {
        _pooledMeshes = new Dictionary<MeshPrimitive, MeshId>(1024);
        
        VertexBuffer = graphicsContext.CreateVertexBuffer<VertexPositionNormalUvTangent>(label);
        VertexBuffer.AllocateStorage(vertexBufferCapacity, StorageAllocationFlags.Dynamic);
        IndexBuffer = graphicsContext.CreateIndexBuffer<uint>(label);
        IndexBuffer.AllocateStorage(indexBufferCapacity, StorageAllocationFlags.Dynamic);
    }

    public IBuffer VertexBuffer { get; }

    public IBuffer IndexBuffer { get; }

    public void Dispose()
    {
        VertexBuffer.Dispose();
        IndexBuffer.Dispose();
    }

    public MeshId GetOrAdd(MeshPrimitive meshPrimitive)
    {
        if (_pooledMeshes.TryGetValue(meshPrimitive, out var pooledMesh))
        {
            return pooledMesh;
        }

        var indexOffset = _pooledMeshes.Values.Sum(pm => pm.IndexCount);
        var indexCount = meshPrimitive.IndexCount;
        var vertexOffset = _pooledMeshes.Values.Sum(pm => pm.VertexCount);
        var vertexCount = meshPrimitive.VertexCount;
        
        pooledMesh = new MeshId(
            (uint)indexCount,
            (uint)indexOffset,
            vertexCount,
            vertexOffset,
            meshPrimitive.BoundingBox.Max,
            meshPrimitive.BoundingBox.Min,
            meshPrimitive.MaterialName);

        var vertices = meshPrimitive.GetVertices();
        VertexBuffer.Update(ref vertices, pooledMesh.VertexOffset);
        IndexBuffer.Update(meshPrimitive.Indices.ToArray(), (int)pooledMesh.IndexOffset);

        _pooledMeshes.Add(meshPrimitive, pooledMesh);
        return pooledMesh;
    }
}