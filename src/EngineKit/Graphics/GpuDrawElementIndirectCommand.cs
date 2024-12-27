using System.Runtime.InteropServices;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = sizeof(uint) * 5)]
public struct GpuDrawElementIndirectCommand
{
    public uint IndexCount; // count

    public uint InstanceCount; // primitivecount

    public uint FirstIndex;

    public uint BaseVertex;

    public uint BaseInstance;
}