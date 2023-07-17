using System;

namespace EngineKit.Graphics;

public interface IGraphicsContext : IDisposable
{
    bool BindComputePipeline(IComputePipeline computePipeline);

    bool BindGraphicsPipeline(IGraphicsPipeline graphicsPipeline);

    void BeginRenderToSwapchain(SwapchainDescriptor swapchainDescriptor);

    void BeginRenderToFramebuffer(FramebufferDescriptor framebufferDescriptor);

    void BlitFramebufferToSwapchain(
        int sourceWidth,
        int sourceHeight,
        int targetWidth,
        int targetHeight);

    bool TryMapBuffer(IBuffer buffer, MemoryAccess memoryAccess, out nint bufferPtr);

    void UnmapBuffer(IBuffer buffer);

    IIndexBuffer CreateIndexBuffer<TIndex>(Label label)
        where TIndex : unmanaged;

    IIndexBuffer CreateIndexBuffer(
        Label label,
        MeshPrimitive[] meshPrimitives);

    IIndirectBuffer CreateIndirectBuffer(Label label);

    IShaderStorageBuffer CreateShaderStorageBuffer<TShaderStorageData>(Label label)
        where TShaderStorageData : unmanaged;

    IUniformBuffer CreateUniformBuffer<TUniformData>(Label label)
        where TUniformData: unmanaged;

    IVertexBuffer CreateVertexBuffer<TVertex>(Label label)
        where TVertex : unmanaged;

    IVertexBuffer CreateVertexBuffer(
        Label label,
        MeshPrimitive[] meshPrimitives,
        VertexType targetVertexType);

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
        bool generateMipmaps = true,
        bool flipVertical = true,
        bool flipHorizontal = false);

    ITexture? CreateTextureFromMemory(ImageInformation image,
        Format format,
        Label label,
        bool generateMipmaps = true,
        bool flipVertical = true,
        bool flipHorizontal = false);

    ITexture? CreateTextureCubeFromFiles(
        Label label,
        string[] filePaths,
        bool flipVertical = true,
        bool flipHorizontal = false);

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
    
    IMeshPool CreateMeshPool(Label label, int vertexBufferCapacity, int indexBufferCapacity);
    
    IMaterialPool CreateMaterialPool(Label label, int materialBufferCapacity, ISamplerLibrary samplerLibrary);

    void EndRender();

    void InsertMemoryBarrier(BarrierMask mask);

    uint CreateFramebuffer(FramebufferDescriptor framebufferDescriptor);

    void RemoveFramebuffer(FramebufferDescriptor framebufferDescriptor);

    void Finish();
    
    FramebufferDescriptor CreateSingleFramebufferDescriptorFromTexture(ITexture texture);
    
    void ClearResourceBindings();
}