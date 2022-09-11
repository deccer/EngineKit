using System;

namespace EngineKit.Graphics;

public interface IBuffer : IDisposable
{
    uint Id { get; }

    int Stride { get; }

    int Count { get; }

    void Resize(uint newSize);

    void Update(IntPtr dataPtr, uint size, int offset);

    void Update<T>(T item, int offset)
        where T : unmanaged;

    void Update<T>(T[] data, int offset)
        where T : unmanaged;
}