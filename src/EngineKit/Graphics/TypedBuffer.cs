using EngineKit.Extensions;
using EngineKit.Native.OpenGL;

namespace EngineKit.Graphics;

internal class TypedBuffer<TElement> : Buffer where TElement : unmanaged
{
    internal unsafe TypedBuffer(
        Label label,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
        : base(label, (nuint)sizeof(TElement), bufferStorageFlags)
    {
    }

    internal unsafe TypedBuffer(
        Label label,
        uint elementCount,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
        : base(label, (nuint)(sizeof(TElement) * elementCount), bufferStorageFlags)
    {
    }
    
    internal unsafe TypedBuffer(
        Label label,
        TElement element,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
        : base(label, bufferStorageFlags)
    {
        SizeInBytes = (uint)sizeof(TElement);

        GL.NamedBufferStorage(Id, element, bufferStorageFlags.ToGL());            

        if ((bufferStorageFlags & BufferStorageFlags.MemoryMapped) == BufferStorageFlags.MemoryMapped)
        {
            const MapFlags mapFlags = MapFlags.Read | MapFlags.Write | MapFlags.Persistent | MapFlags.Coherent;
            MappedPointer = GL.MapBufferRange(Id, nuint.Zero, SizeInBytes, mapFlags.ToGL());
        }
    }
    
    internal unsafe TypedBuffer(
        Label label,
        in TElement element,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
        : base(label, bufferStorageFlags)
    {
        SizeInBytes = (uint)sizeof(TElement);

        GL.NamedBufferStorage(Id, in element, bufferStorageFlags.ToGL());            

        if ((bufferStorageFlags & BufferStorageFlags.MemoryMapped) == BufferStorageFlags.MemoryMapped)
        {
            const MapFlags mapFlags = MapFlags.Read | MapFlags.Write | MapFlags.Persistent | MapFlags.Coherent;
            MappedPointer = GL.MapBufferRange(Id, nuint.Zero, SizeInBytes, mapFlags.ToGL());
        }
    }
    
    internal unsafe TypedBuffer(
        Label label,
        TElement[] elements,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
        : base(label, bufferStorageFlags)
    {
        SizeInBytes = (uint)(sizeof(TElement) * elements.Length);

        GL.NamedBufferStorage(Id, elements, bufferStorageFlags.ToGL());            

        if ((bufferStorageFlags & BufferStorageFlags.MemoryMapped) == BufferStorageFlags.MemoryMapped)
        {
            const MapFlags mapFlags = MapFlags.Read | MapFlags.Write | MapFlags.Persistent | MapFlags.Coherent;
            MappedPointer = GL.MapBufferRange(Id, nuint.Zero, SizeInBytes, mapFlags.ToGL());
        }
    }
    
    internal unsafe TypedBuffer(
        Label label,
        in TElement[] elements,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
        : base(label, bufferStorageFlags)
    {
        SizeInBytes = (uint)(sizeof(TElement) * elements.Length);

        GL.NamedBufferStorage(Id, in elements, bufferStorageFlags.ToGL());            

        if ((bufferStorageFlags & BufferStorageFlags.MemoryMapped) == BufferStorageFlags.MemoryMapped)
        {
            const MapFlags mapFlags = MapFlags.Read | MapFlags.Write | MapFlags.Persistent | MapFlags.Coherent;
            MappedPointer = GL.MapBufferRange(Id, nuint.Zero, SizeInBytes, mapFlags.ToGL());
        }
    }
}