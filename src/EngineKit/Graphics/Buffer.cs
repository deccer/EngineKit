using System;
using System.Runtime.CompilerServices;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal abstract class Buffer : IBuffer
{
    protected Buffer()
    {
        Id = GL.CreateBuffer();
    }

    public uint Id { get; }

    public int Stride { get; protected set; }

    public int Count { get; protected set; }

    public void Dispose()
    {
        GL.DeleteBuffer(Id);
    }

    public void Resize(uint newSize)
    {
        GL.NamedBufferData(Id, newSize, IntPtr.Zero, GL.BufferUsage.DynamicDraw);
    }

    public void Update(IntPtr dataPtr, uint size, int offset)
    {
        GL.NamedBufferSubData(Id, offset, size, dataPtr);
    }

    public void Update<TElement>(TElement data, int offset)
        where TElement : unmanaged
    {
        GL.NamedBufferSubData(Id, offset * Stride, data);
        Count = 1;
    }

    public void Update<TElement>(TElement[] data, int offset)
        where TElement : unmanaged
    {
        GL.NamedBufferSubData(Id, offset * Stride, data);
        Count = data.Length;
    }

    public static implicit operator uint(Buffer buffer)
    {
        return buffer.Id;
    }
}

internal class Buffer<TElement> : Buffer
    where TElement : unmanaged
{
    internal Buffer(
        BufferTarget bufferTarget,
        Label label,
        TElement data)
        : this(bufferTarget, label)
    {
        GL.NamedBufferStorage(Id, data, GL.BufferStorageMask.DynamicStorageBit | GL.BufferStorageMask.MapWriteBit);
        Stride = Unsafe.SizeOf<TElement>();
        Count = 1;
    }

    internal Buffer(
        BufferTarget bufferTarget,
        Label label,
        TElement[] data)
        : this(bufferTarget, label)
    {
        GL.NamedBufferStorage(Id, data, GL.BufferStorageMask.DynamicStorageBit | GL.BufferStorageMask.MapWriteBit);
        Count = data.Length;
        Stride = Unsafe.SizeOf<TElement>();
    }

    internal Buffer(
        BufferTarget bufferTarget,
        Label label,
        uint size)
        : this(bufferTarget, label)
    {
        Stride = Unsafe.SizeOf<TElement>();
        unsafe
        {
            GL.NamedBufferStorage(
                Id,
                (uint)(size * sizeof(TElement)),
                nint.Zero,
                GL.BufferStorageMask.DynamicStorageBit | GL.BufferStorageMask.MapWriteBit);
        }
    }

    private Buffer(BufferTarget bufferTarget, Label? label = null)
    {
        var innerLabel = $"{GetBufferNamePrefix(bufferTarget)}_{typeof(TElement).Name}";
        if (!string.IsNullOrEmpty(label))
        {
            innerLabel += $"_{label}";
        }

        GL.ObjectLabel(GL.ObjectIdentifier.Buffer, Id, innerLabel);
    }

    private static string GetBufferNamePrefix(BufferTarget bufferTarget)
    {
        return bufferTarget switch
        {
            BufferTarget.VertexBuffer => "Buffer_VB",
            BufferTarget.IndexBuffer => "Buffer_IB",
            BufferTarget.ShaderStorageBuffer => "Buffer_SS",
            BufferTarget.UniformBuffer => "Buffer_UB",
            BufferTarget.IndirectDrawBuffer => "Buffer_ID",
            _ => throw new ArgumentOutOfRangeException(nameof(bufferTarget), bufferTarget, null)
        };
    }
}