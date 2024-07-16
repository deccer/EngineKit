using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EngineKit.Graphics.RHI;

namespace EngineKit.Graphics;

internal sealed class MeshPool : IMeshPool
{
    private readonly Dictionary<MeshPrimitive, PooledMesh> _pooledMeshes;

    public MeshPool(
        Label label,
        IGraphicsContext graphicsContext,
        uint maxVertexCount,
        uint maxIndexCount)
    {
        _pooledMeshes = new Dictionary<MeshPrimitive, PooledMesh>(1024);

        VertexBuffer = graphicsContext.CreateUntypedBuffer(label + "Vertices", (nuint)(maxVertexCount * Unsafe.SizeOf<GpuVertexPositionNormalUvTangent>()), BufferStorageFlags.DynamicStorage);
        IndexBuffer = graphicsContext.CreateUntypedBuffer(label + "Indices", (nuint)(maxIndexCount * Unsafe.SizeOf<uint>()), BufferStorageFlags.DynamicStorage);
    }

    public IBuffer VertexBuffer { get; }

    public IBuffer IndexBuffer { get; }

    public uint VertexBufferStride => GpuVertexPositionNormalUvTangent.Stride;

    public void Dispose()
    {
        VertexBuffer.Dispose();
        IndexBuffer.Dispose();
    }

    public PooledMesh GetOrAdd(MeshPrimitive meshPrimitive)
    {
        if (_pooledMeshes.TryGetValue(meshPrimitive, out var pooledMesh))
        {
            return pooledMesh;
        }

        var indexOffset = _pooledMeshes.Values.Sum(pm => pm.IndexCount);
        var indexCount = meshPrimitive.IndexCount;
        var vertexOffset = (uint)_pooledMeshes.Values.Sum(pm => pm.VertexCount);
        var vertexCount = meshPrimitive.VertexCount;

        pooledMesh = new PooledMesh(
            (uint)indexCount,
            (uint)indexOffset,
            vertexCount,
            vertexOffset,
            meshPrimitive.BoundingBox.Max,
            meshPrimitive.BoundingBox.Min,
            meshPrimitive.MaterialName);

        var vertices = meshPrimitive.GetVertices();
        VertexBuffer.UpdateElements(vertices, pooledMesh.VertexOffset);
        IndexBuffer.UpdateElements(meshPrimitive.Indices.ToArray(), pooledMesh.IndexOffset);

        _pooledMeshes.Add(meshPrimitive, pooledMesh);
        return pooledMesh;
    }
}
