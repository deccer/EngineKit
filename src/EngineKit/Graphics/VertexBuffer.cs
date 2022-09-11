using System;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal sealed class VertexBuffer<TVertex> : Buffer<TVertex>, IVertexBuffer where TVertex: unmanaged
{
    internal VertexBuffer(Label label, TVertex data)
        : base(BufferTarget.VertexBuffer, label, data)
    {
    }

    internal VertexBuffer(Label label, TVertex[] data)
        : base(BufferTarget.VertexBuffer, label, data)
    {
    }

    internal VertexBuffer(Label label, uint size)
        : base(BufferTarget.VertexBuffer, label, size)
    {
    }

    public void Bind(IInputLayout inputLayout, uint bindingIndex)
    {
        GL.VertexArrayVertexBuffer(
            inputLayout.Id,
            bindingIndex,
            Id,
            IntPtr.Zero,
            Stride);
    }

    public void Bind(IInputLayout inputLayout, uint bindingIndex, uint offset)
    {
        GL.VertexArrayVertexBuffer(
            inputLayout.Id,
            bindingIndex,
            Id,
            new IntPtr(offset),
            Stride);
    }
}