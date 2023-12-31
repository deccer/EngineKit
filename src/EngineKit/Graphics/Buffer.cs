using System;
using System.Runtime.CompilerServices;
using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal class Buffer : IBuffer
{
    private readonly BufferStorageFlags _bufferStorageFlags;
    
    public Label Label { get; protected set; }

    public uint Id { get; protected set; }
    
    public nuint SizeInBytes { get; protected set; }
    
    public nint MappedPointer { get; protected set; }

    internal Buffer(Label label, BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
    {
        Label = label;
        _bufferStorageFlags = bufferStorageFlags;
        
        Id = GL.CreateBuffer();
        if (!string.IsNullOrEmpty(label))
        {
            GL.ObjectLabel(GL.ObjectIdentifier.Buffer, Id, label);
        }        
    }
    
    internal Buffer(
        Label label,
        nuint size,
        nint dataPtr,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
        : this(label, bufferStorageFlags)
    {
        SizeInBytes = (nuint)MathF.Max(size, 4);
        GL.NamedBufferStorage(Id, SizeInBytes, dataPtr, bufferStorageFlags.ToGL());
        if ((bufferStorageFlags & BufferStorageFlags.MemoryMapped) == BufferStorageFlags.MemoryMapped)
        {
            const MapFlags mapFlags = MapFlags.Read | MapFlags.Write | MapFlags.Persistent | MapFlags.Coherent;
            MappedPointer = GL.MapBufferRange(Id, nuint.Zero, SizeInBytes, mapFlags.ToGL());
        }
    }

    internal Buffer(
        Label label,
        nuint size,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
        : this(label, size, nint.Zero, bufferStorageFlags)
    {
    }

    public void Dispose()
    {
        if (MappedPointer != nint.Zero)
        {
            GL.UnmapBuffer(Id);
        }

        if (Id != 0u)
        {
            GL.DeleteBuffer(Id);
        }
    }

    public unsafe void UpdateData(nint data, nuint offset, nuint sizeInBytes)
    {
        if (!_bufferStorageFlags.HasFlag(BufferStorageFlags.DynamicStorage))
        {
            GL.DebugMessageInsert(GL.DebugSource.Application, GL.DebugType.Error, 0, GL.DebugSeverity.High, $"Buffer {Label} cannot be updated. It has not the DynamicStorage buffer storage flag");
            return;
        }

        GL.NamedBufferSubData(Id, offset, sizeInBytes, (void*)data);
    }
    
    public void UpdateElement<TElement>(TElement element, nuint elementOffset) where TElement : unmanaged
    {
        if (!_bufferStorageFlags.HasFlag(BufferStorageFlags.DynamicStorage))
        {
            GL.DebugMessageInsert(GL.DebugSource.Application, GL.DebugType.Error, 0, GL.DebugSeverity.High, $"Buffer {Label} cannot be updated. It has not the DynamicStorage buffer storage flag");
            return;
        }
        
        GL.NamedBufferSubData(Id, elementOffset * (uint)Unsafe.SizeOf<TElement>(), in element);
    }
    
    public void UpdateElement<TElement>(in TElement element, nuint elementOffset) where TElement : unmanaged
    {
        if (!_bufferStorageFlags.HasFlag(BufferStorageFlags.DynamicStorage))
        {
            GL.DebugMessageInsert(GL.DebugSource.Application, GL.DebugType.Error, 0, GL.DebugSeverity.High, $"Buffer {Label} cannot be updated. It has not the DynamicStorage buffer storage flag");
            return;
        }
        
        GL.NamedBufferSubData(Id, elementOffset * (uint)Unsafe.SizeOf<TElement>(), in element);
    }

    public void UpdateElements<TElement>(TElement[] elements, nuint elementOffset) where TElement : unmanaged
    {
        if (!_bufferStorageFlags.HasFlag(BufferStorageFlags.DynamicStorage))
        {
            GL.DebugMessageInsert(GL.DebugSource.Application, GL.DebugType.Error, 0, GL.DebugSeverity.High, $"Buffer {Label} cannot be updated. It has not the DynamicStorage buffer storage flag");
            return;
        }
        
        GL.NamedBufferSubData(Id, elementOffset * (uint)Unsafe.SizeOf<TElement>(), in elements);
    }
    
    public void UpdateElements<TElement>(Span<TElement> elements, nuint elementOffset) where TElement : unmanaged
    {
        if (!_bufferStorageFlags.HasFlag(BufferStorageFlags.DynamicStorage))
        {
            GL.DebugMessageInsert(GL.DebugSource.Application, GL.DebugType.Error, 0, GL.DebugSeverity.High, $"Buffer {Label} cannot be updated. It has not the DynamicStorage buffer storage flag");
            return;
        }
        
        GL.NamedBufferSubData(Id, elementOffset * (uint)Unsafe.SizeOf<TElement>(), elements);
    }
    
    public void UpdateElements<TElement>(in TElement[] elements, nuint elementOffset) where TElement : unmanaged
    {
        if (!_bufferStorageFlags.HasFlag(BufferStorageFlags.DynamicStorage))
        {
            GL.DebugMessageInsert(GL.DebugSource.Application, GL.DebugType.Error, 0, GL.DebugSeverity.High, $"Buffer {Label} cannot be updated. It has not the DynamicStorage buffer storage flag");
            return;
        }
        
        GL.NamedBufferSubData(Id, elementOffset * (uint)Unsafe.SizeOf<TElement>(), in elements);
    }
    
    public unsafe void ClearAll()
    {
        var clearData = 0;
        GL.ClearNamedBufferSubData(Id, 0, SizeInBytes, &clearData);
    }    

    public unsafe void ClearWith(BufferClearInfo bufferClearInfo)
    {
        var clearSize = bufferClearInfo.Size == EngineKit.SizeInBytes.Whole ? SizeInBytes : bufferClearInfo.Size;
        GL.ClearNamedBufferSubData(Id, bufferClearInfo.Offset, clearSize, &bufferClearInfo.Value);
    }

    public static implicit operator uint(Buffer buffer)
    {
        return buffer.Id;
    }
}