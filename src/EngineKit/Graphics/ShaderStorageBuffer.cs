using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal sealed class ShaderStorageBuffer<T> : Buffer<T>, IShaderStorageBuffer where T : unmanaged
{
    internal ShaderStorageBuffer(string name, T data)
        : base(BufferTarget.ShaderStorageBuffer, name, data)
    {
    }

    internal ShaderStorageBuffer(string name, T[] data)
        : base(BufferTarget.ShaderStorageBuffer, name, data)
    {
    }

    internal ShaderStorageBuffer(string name, uint size)
        : base(BufferTarget.ShaderStorageBuffer, name, size)
    {
    }

    public void Bind(uint bindingIndex)
    {
        GL.BindBufferBase(BufferTarget.ShaderStorageBuffer.ToGL(), bindingIndex, Id);
    }
}