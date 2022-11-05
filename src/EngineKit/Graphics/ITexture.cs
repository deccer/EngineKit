using System;
using System.Buffers;

namespace EngineKit.Graphics;

public interface ITexture : IDisposable
{
    Format Format { get; }

    uint Id { get; }

    ulong MakeResident();

    TextureView CreateTextureView();

    void GenerateMipmaps();

    void Update(
        TextureUpdateDescriptor textureUpdateDescriptor,
        IntPtr pixelPtr);

    void Update(
        TextureUpdateDescriptor textureUpdateDescriptor,
        MemoryHandle memoryHandle);

    void Update<TPixel>(
        TextureUpdateDescriptor textureUpdateDescriptor,
        in TPixel pixelData) where TPixel : unmanaged;
}