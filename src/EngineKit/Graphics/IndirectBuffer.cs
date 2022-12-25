using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal sealed class IndirectBuffer : Buffer<GpuIndirectElementData>, IIndirectBuffer
{
    internal IndirectBuffer(Label label)
        : base(BufferTarget.IndirectDrawBuffer, label)
    {
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.IndirectDrawBuffer.ToGL(), Id);
    }
}