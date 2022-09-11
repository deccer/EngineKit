using System;
using System.Buffers;

namespace EngineKit.Graphics;

public interface ITexture : IDisposable
{
    Format Format { get; }

    uint Id { get; }

    ulong MakeResident();

    TextureView CreateTextureView();

    void Upload(
        TextureUpdateDescriptor textureUpdateDescriptor,
        IntPtr pixelPtr);

    void Upload(
        TextureUpdateDescriptor textureUpdateDescriptor,
        MemoryHandle memoryHandle);

    void Upload<TPixel>(
        TextureUpdateDescriptor textureUpdateDescriptor,
        in TPixel pixelData) where TPixel : unmanaged;
}