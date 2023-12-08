using System;

namespace EngineKit.Graphics;

public interface IBuffer : IDisposable
{
    uint Id { get; }

    int Stride { get; }

    int Count { get; }
    
    bool IsMappable { get; }

    void AllocateStorage(int sizeInBytes, StorageAllocationFlags storageAllocationFlags);

    void AllocateStorage<TElement>(TElement element, StorageAllocationFlags storageAllocationFlags)
        where TElement : unmanaged;

    void AllocateStorage<TElement>(TElement[] elements, StorageAllocationFlags storageAllocationFlags)
        where TElement : unmanaged;

    void Update(nint dataPtr, int offsetInBytes, int sizeInBytes);

    void Update<TElement>(TElement item, int elementOffset = 0)
        where TElement : unmanaged;

    void Update<TElement>(TElement[] data, int elementOffset = 0)
        where TElement : unmanaged;
    
    void Update<TElement>(Span<TElement> data, int elementOffset = 0)
        where TElement : unmanaged;    
}