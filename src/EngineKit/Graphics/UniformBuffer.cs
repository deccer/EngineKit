using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal sealed class UniformBuffer<T> : Buffer<T>, IUniformBuffer where T : unmanaged
{
    internal UniformBuffer(Label label, T data)
        : base(BufferTarget.UniformBuffer, label, data)
    {
    }

    internal UniformBuffer(Label label, T[] data)
        : base(BufferTarget.UniformBuffer, label, data)
    {
    }

    internal UniformBuffer(Label label, uint size)
        : base(BufferTarget.UniformBuffer, label, size)
    {
    }

    public void Bind(uint bindingIndex)
    {
        GL.BindBufferBase(BufferTarget.UniformBuffer.ToGL(), bindingIndex, Id);
    }
}