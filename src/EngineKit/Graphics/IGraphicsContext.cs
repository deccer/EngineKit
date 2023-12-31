using System;
using EngineKit.Mathematics;

namespace EngineKit.Graphics;

public interface IGraphicsContext : IDisposable
{
    bool BindComputePipeline(IComputePipeline computePipeline);

    bool BindGraphicsPipeline(IGraphicsPipeline graphicsPipeline);

    void BeginRenderPass(SwapchainDescriptor swapchainDescriptor);

    void BeginRenderPass(FramebufferDescriptor framebufferDescriptor);

    void BlitFramebufferToSwapchain(
        int sourceWidth,
        int sourceHeight,
        int targetWidth,
        int targetHeight);

    void BlitFramebufferToSwapchain(
        FramebufferDescriptor sourceFramebufferDescriptor,
        FramebufferDescriptor targetFramebufferDescriptor,
        int sourceWidth,
        int sourceHeight,
        int targetWidth,
        int targetHeight);

    IBuffer CreateUntypedBuffer(
        Label label,
        nuint sizeInBytes,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None);

    IBuffer CreateUntypedBuffer(
        Label label,
        nuint sizeInBytes,
        nint dataPtr,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None);

    IBuffer CreateTypedBuffer<TElement>(
        Label label,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
        where TElement : unmanaged;
    
    IBuffer CreateTypedBuffer<TElement>(
        Label label,
        TElement element,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
        where TElement : unmanaged;
    
    IBuffer CreateTypedBuffer<TElement>(
        Label label,
        TElement[] elements,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
        where TElement : unmanaged;

    IBuffer CreateTypedBuffer<TElement>(
        Label label,
        uint elementCount,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
        where TElement : unmanaged;

    IBuffer CreateVertexBuffer(
        Label label,
        MeshPrimitive[] meshPrimitives);

    IBuffer CreateIndexBuffer(
        Label label,
        MeshPrimitive[] meshPrimitives);

    ISampler CreateSampler(SamplerDescriptor samplerDescriptor);

    SamplerBuilder CreateSamplerBuilder();

    IComputePipelineBuilder CreateComputePipelineBuilder();

    IGraphicsPipelineBuilder CreateGraphicsPipelineBuilder();

    ITexture CreateTexture(TextureCreateDescriptor textureCreateDescriptor);

    ITexture CreateTexture2D(
        int width,
        int height,
        Format format,
        Label? label = null);

    ITexture? CreateTextureFromFile(
        string filePath,
        Format format,
        bool generateMipmaps,
        bool flipVertical,
        bool flipHorizontal);

    ITexture? CreateTextureFromMemory(ImageInformation image,
        Format format,
        Label label,
        bool generateMipmaps,
        bool flipVertical,
        bool flipHorizontal);

    ITexture? CreateTextureCubeFromFiles(
        Label label,
        string[] filePaths,
        bool flipVertical,
        bool flipHorizontal);

    IFramebufferDescriptorBuilder GetFramebufferDescriptorBuilder();

    void CopyTexture(
        ITexture sourceTexture,
        int sourceOffsetX,
        int sourceOffsetY,
        int sourceWidth,
        int sourceHeight,
        ITexture targetTexture,
        int targetOffsetX,
        int targetOffsetY,
        int targetWidth,
        int targetHeight,
        FramebufferBit framebufferBit,
        BlitFramebufferFilter interpolationFilter);
    
    IMeshPool CreateMeshPool(
        Label label,
        uint maxVertexCount,
        uint maxIndexCount);
    
    IMaterialPool CreateMaterialPool(
        Label label,
        uint maxMaterialCount,
        ISamplerLibrary samplerLibrary);

    void EndRenderPass();

    void InsertMemoryBarrier(BarrierMask mask);

    uint CreateFramebuffer(FramebufferDescriptor framebufferDescriptor);

    void RemoveFramebuffer(FramebufferDescriptor framebufferDescriptor);

    void Finish();
    
    FramebufferDescriptor CreateSingleFramebufferDescriptorFromTexture(ITexture texture);
    
    void ClearResourceBindings();
    
    void UseViewport(Viewport viewport);
}