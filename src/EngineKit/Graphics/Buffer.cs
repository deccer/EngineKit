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
    
    public bool IsMappable { get; private set; }

    public void Dispose()
    {
        GL.DeleteBuffer(Id);
    }

    public void AllocateStorage(int sizeInBytes, StorageAllocationFlags storageAllocationFlags)
    {
        GL.NamedBufferStorage(Id, sizeInBytes, nint.Zero, storageAllocationFlags.ToGL());
        SizeInBytes = sizeInBytes;
        Count = SizeInBytes / Stride;
        IsMappable = storageAllocationFlags.HasFlag(StorageAllocationFlags.Mappable);
    }

    public void AllocateStorage<TElement>(TElement element, StorageAllocationFlags storageAllocationFlags)
        where TElement : unmanaged
    {
        GL.NamedBufferStorage(Id, element, storageAllocationFlags.ToGL());
        Count = 1;
        SizeInBytes = Marshal.SizeOf<TElement>();
        IsMappable = storageAllocationFlags.HasFlag(StorageAllocationFlags.Mappable);
    }

    public void AllocateStorage<TElement>(TElement[] elements, StorageAllocationFlags storageAllocationFlags)
        where TElement : unmanaged
    {
        GL.NamedBufferStorage(Id, elements, storageAllocationFlags.ToGL());
        Count = elements.Length;
        SizeInBytes = Marshal.SizeOf<TElement>() * Count;
        IsMappable = storageAllocationFlags.HasFlag(StorageAllocationFlags.Mappable);
    }

    public unsafe void Update(nint dataPtr, int offsetInBytes, int sizeInBytes)
    {
        if (offsetInBytes + sizeInBytes > SizeInBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(offsetInBytes));
        }
        GL.NamedBufferSubData(Id, offsetInBytes, sizeInBytes, (void*)dataPtr);
    }

    public void Update<TElement>(TElement element, int elementOffset = 0)
        where TElement : unmanaged
    {
        if ((elementOffset * Stride) + Stride > SizeInBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(elementOffset));
        }
        GL.NamedBufferSubData(Id, elementOffset * Stride, element);
    }

    public void Update<TElement>(TElement[] data, int elementOffset = 0)
        where TElement : unmanaged
    {
        if ((elementOffset * Stride) + data.Length * Stride > SizeInBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(elementOffset));
        }
        GL.NamedBufferSubData(Id, elementOffset * Stride, data);
    }

    public static implicit operator uint(Buffer buffer)
    {
        return buffer.Id;
    }
}

internal class Buffer<TElement> : Buffer
    where TElement : unmanaged
{
    internal Buffer(BufferTarget bufferTarget, Label? label = null)
    {
        var innerLabel = $"{GetBufferNamePrefix(bufferTarget)}-{typeof(TElement).Name}";
        if (!string.IsNullOrEmpty(label))
        {
            innerLabel += $"-{label}";
        }

        GL.ObjectLabel(GL.ObjectIdentifier.Buffer, Id, innerLabel);
        Stride = Marshal.SizeOf<TElement>();
    }

    private static string GetBufferNamePrefix(BufferTarget bufferTarget)
    {
        return bufferTarget switch
        {
            BufferTarget.VertexBuffer => "Buffer-Vertices",
            BufferTarget.IndexBuffer => "Buffer-Indices",
            BufferTarget.ShaderStorageBuffer => "Buffer-ShaderStorage",
            BufferTarget.UniformBuffer => "Buffer-Uniforms",
            BufferTarget.DrawIndirectBuffer => "Buffer-Indirect",
            _ => throw new ArgumentOutOfRangeException(nameof(bufferTarget), bufferTarget, null)
        };
    }
}