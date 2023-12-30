using System;

namespace EngineKit.Graphics;

public interface IBuffer : IDisposable
{
    Label Label { get; }
    
    uint Id { get; }
    
    nuint SizeInBytes { get; }
    
    nint MappedPointer { get; }
    
    void UpdateData(nint data, nuint offset, nuint sizeInBytes);
    
    //void UpdateElement<TElement>(TElement element, nuint elementOffset) where TElement : unmanaged;

    void UpdateElement<TElement>(in TElement element, nuint elementOffset) where TElement : unmanaged;

    //void UpdateElements<TElement>(TElement[] elements, nuint elementOffset) where TElement : unmanaged;
    
    void UpdateElements<TElement>(in TElement[] elements, nuint elementOffset) where TElement : unmanaged;
    
    void UpdateElements<TElement>(Span<TElement> elements, nuint elementOffset) where TElement : unmanaged;
    
    void ClearAll();
    
    void ClearWith(BufferClearInfo bufferClearInfo);
}