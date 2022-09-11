using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal sealed class IndexBuffer<TIndex> : Buffer<TIndex>, IIndexBuffer where TIndex: unmanaged
{
    internal IndexBuffer(Label label, TIndex data)
        : base(BufferTarget.IndexBuffer, label, data)
    {
    }

    internal IndexBuffer(Label label, TIndex[] data)
        : base(BufferTarget.IndexBuffer, label, data)
    {
    }

    internal IndexBuffer(Label label, uint size)
        : base(BufferTarget.IndexBuffer, label, size)
    {
    }

    public void Bind(IInputLayout inputLayout)
    {
        GL.VertexArrayElementBuffer(
            inputLayout.Id,
            Id);
    }
}