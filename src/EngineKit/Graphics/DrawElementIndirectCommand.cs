using System.Runtime.InteropServices;

namespace EngineKit.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = sizeof(uint) * 5)]
public struct DrawElementIndirectCommand
{
    public uint IndexCount; // count

    public uint InstanceCount; // primitivecount

    public uint FirstIndex;

    public int BaseVertex;

    public uint BaseInstance;
}