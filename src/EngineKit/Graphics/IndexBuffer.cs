using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal sealed class IndexBuffer<TIndex> : Buffer<TIndex>, IIndexBuffer
    where TIndex: unmanaged
{
    internal IndexBuffer(Label label)
        : base(BufferTarget.IndexBuffer, label)
    {
    }

    public void Bind(IInputLayout inputLayout)
    {
        GL.VertexArrayElementBuffer(
            inputLayout.Id,
            Id);
    }
}