using System;
using System.Buffers;

namespace EngineKit.Graphics;

public interface ITexture : IHasTextureId, IDisposable
{
    TextureCreateDescriptor TextureCreateDescriptor { get; }

    ulong TextureHandle { get; }

    void MakeResident();

    void MakeResident(ISampler sampler);

    void MakeNonResident();

    TextureView CreateTextureView();
    
    TextureView CreateTextureView(SwizzleMapping swizzleMapping);
    
    TextureView CreateTextureView(TextureViewDescriptor textureViewDescriptor);

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