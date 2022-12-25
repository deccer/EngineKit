﻿using System;

namespace EngineKit.Graphics;

public interface IBuffer : IDisposable
{
    uint Id { get; }

    int Stride { get; }

    int Count { get; }

    void AllocateStorage(int sizeInBytes, StorageAllocationFlags storageAllocationFlags);

    void AllocateStorage<TElement>(TElement element, StorageAllocationFlags storageAllocationFlags)
        where TElement : unmanaged;

    void AllocateStorage<TElement>(TElement[] elements, StorageAllocationFlags storageAllocationFlags)
        where TElement : unmanaged;

    void Update(nint dataPtr, int offsetInBytes, int sizeInBytes);

    void Update<T>(T item, int elementOffset)
        where T : unmanaged;

    void Update<T>(T[] data, int elementOffset)
        where T : unmanaged;
}