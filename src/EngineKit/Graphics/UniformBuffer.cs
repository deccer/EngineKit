using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal sealed class UniformBuffer<T> : Buffer<T>, IUniformBuffer
    where T : unmanaged
{
    internal UniformBuffer(Label label)
        : base(BufferTarget.UniformBuffer, label)
    {
    }

    public void Bind(uint bindingIndex)
    {
        GL.BindBufferBase(BufferTarget.UniformBuffer.ToGL(), bindingIndex, Id);
    }
}