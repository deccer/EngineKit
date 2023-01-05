using System;
using System.Runtime.InteropServices;
using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal abstract class Buffer : IBuffer
{
    protected Buffer()
    {
        Id = GL.CreateBuffer();
        SizeInBytes = 0;
    }

    public uint Id { get; }

    public int Stride { get; protected set; }

    public int Count { get; protected set; }

    public int SizeInBytes { get; private set; }

    public void Dispose()
    {
        GL.DeleteBuffer(Id);
    }

    public void AllocateStorage(int sizeInBytes, StorageAllocationFlags storageAllocationFlags)
    {
        GL.NamedBufferStorage(Id, sizeInBytes, nint.Zero, storageAllocationFlags.ToGL());
        SizeInBytes = sizeInBytes;
    }

    public void AllocateStorage<TElement>(TElement element, StorageAllocationFlags storageAllocationFlags)
        where TElement : unmanaged
    {
        GL.NamedBufferStorage(Id, element, storageAllocationFlags.ToGL());
        Count = 1;
        SizeInBytes = Marshal.SizeOf<TElement>();
    }

    public void AllocateStorage<TElement>(TElement[] elements, StorageAllocationFlags storageAllocationFlags)
        where TElement : unmanaged
    {
        GL.NamedBufferStorage(Id, elements, storageAllocationFlags.ToGL());
        Count = elements.Length;
        SizeInBytes = Marshal.SizeOf<TElement>() * Count;
    }

    public unsafe void Update(nint dataPtr, int offsetInBytes, int sizeInBytes)
    {
        GL.NamedBufferSubData(Id, offsetInBytes, sizeInBytes, (void*)dataPtr);
    }

    public void Update<TElement>(TElement element, int elementOffset)
        where TElement : unmanaged
    {
        GL.NamedBufferSubData(Id, elementOffset * Stride, element);
        Count = 1;
    }

    public void Update<TElement>(TElement[] data, int elementOffset)
        where TElement : unmanaged
    {
        GL.NamedBufferSubData(Id, elementOffset * Stride, data);
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
    protected Buffer(BufferTarget bufferTarget, Label? label = null)
    {
        var innerLabel = $"{GetBufferNamePrefix(bufferTarget)}_{typeof(TElement).Name}";
        if (!string.IsNullOrEmpty(label))
        {
            innerLabel += $"_{label}";
        }

        GL.ObjectLabel(GL.ObjectIdentifier.Buffer, Id, innerLabel);
        Stride = Marshal.SizeOf<TElement>();
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