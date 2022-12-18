using System;
using System.Buffers;

namespace EngineKit.Graphics;

public interface ITexture : IDisposable
{
    Format Format { get; }

    uint Id { get; }

    ulong TextureHandle { get; }

    void MakeResident();

    void MakeNonResident();

    TextureView CreateTextureView();

    void GenerateMipmaps();

    void Update(
        TextureUpdateDescriptor textureUpdateDescriptor,
        nint pixelPtr);

    void Update(
        TextureUpdateDescriptor textureUpdateDescriptor,
        MemoryHandle memoryHandle);

    void Update<TPixel>(
        TextureUpdateDescriptor textureUpdateDescriptor,
        in TPixel pixelData) where TPixel : unmanaged;
}