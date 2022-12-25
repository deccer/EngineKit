using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal sealed class ShaderStorageBuffer<T> : Buffer<T>, IShaderStorageBuffer
    where T : unmanaged
{
    internal ShaderStorageBuffer(Label label)
        : base(BufferTarget.ShaderStorageBuffer, label)
    {
    }

    public void Bind(uint bindingIndex)
    {
        GL.BindBufferBase(BufferTarget.ShaderStorageBuffer.ToGL(), bindingIndex, Id);
    }
}