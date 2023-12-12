using System.Numerics;

namespace EngineKit.Graphics;

public readonly struct MeshId
{
    public MeshId(uint indexCount, uint indexOffset, int vertexCount, int vertexOffset, Vector3 aabbMax, Vector3 aabbMin, string? materialName)
    {
        AabbMax = aabbMax;
        AabbMin = aabbMin;
        IndexCount = indexCount;
        IndexOffset = indexOffset;
        VertexCount = vertexCount;
        VertexOffset = vertexOffset;
        MaterialName = materialName;
    }

    public readonly Vector3 AabbMax;

    public readonly Vector3 AabbMin;

    public readonly uint IndexCount;

    public readonly uint IndexOffset;

    public readonly int VertexCount;

    public readonly int VertexOffset;

    public readonly string? MaterialName;
}