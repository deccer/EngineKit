using EngineKit.Mathematics;

namespace ComplexExample;

public struct GpuObjectData
{
    public Matrix WorldMatrix;
    public Vector4 AabbMin;
    public Vector4 AabbMax;
    public uint VertexOffset;
    public uint VertexCount;
    public uint IndexOffset;
    public uint IndexCount;
    public int MaterialIndex;
    public int _padding1;
    public int _padding2;
    public int _padding3;
}