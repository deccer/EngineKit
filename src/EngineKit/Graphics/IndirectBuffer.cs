using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal sealed class IndirectBuffer : Buffer<GpuIndirectElementData>, IIndirectBuffer
{
    internal IndirectBuffer(Label label, GpuIndirectElementData data)
        : base(BufferTarget.IndirectDrawBuffer, label, data)
    {
    }

    internal IndirectBuffer(Label label, GpuIndirectElementData[] data)
        : base(BufferTarget.IndirectDrawBuffer, label, data)
    {
    }

    internal IndirectBuffer(Label label, uint size)
        : base(BufferTarget.IndirectDrawBuffer, label, size)
    {
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.IndirectDrawBuffer.ToGL(), Id);
    }
}