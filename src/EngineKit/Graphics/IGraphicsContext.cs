using System;

namespace EngineKit.Graphics;

public interface IGraphicsContext : IDisposable
{
    bool BindComputePipeline(IComputePipeline computePipeline);

    bool BindGraphicsPipeline(IGraphicsPipeline graphicsPipeline);

    void BeginRenderToSwapchain(SwapchainRenderDescriptor swapchainRenderDescriptor);

    void BeginRenderToFramebuffer(FramebufferRenderDescriptor framebufferRenderDescriptor);

    void BlitFramebufferToSwapchain(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight);

    IShaderStorageBuffer CreateShaderStorageBuffer<TShaderStorageData>(
        Label label,
        TShaderStorageData[] shaderStorageData) where TShaderStorageData : unmanaged;

    IShaderStorageBuffer CreateShaderStorageBuffer<TShaderStorageData>(
        Label label,
        uint size) where TShaderStorageData : unmanaged;

    IIndirectBuffer CreateIndirectBuffer(Label label,
        GpuIndirectElementData[] indirectElementData);

    IVertexBuffer CreateVertexBuffer<TVertex>(
        Label label,
        uint size) where TVertex : unmanaged;

    IVertexBuffer CreateVertexBuffer<TVertex>(
        Label label,
        TVertex[] vertices) where TVertex : unmanaged;

    IVertexBuffer CreateVertexBuffer(
        Label label,
        MeshData[] meshDates,
        VertexType targetVertexType);

    IIndexBuffer CreateIndexBuffer<TIndex>(
        Label label,
        uint size) where TIndex : unmanaged;

    IIndexBuffer CreateIndexBuffer<TIndex>(
        Label label,
        TIndex[] indices) where TIndex : unmanaged;

    IIndexBuffer CreateIndexBuffer(
        Label label,
        MeshData[] meshDates);

    IUniformBuffer CreateUniformBuffer<TUniformData>(
        Label label,
        TUniformData uniformData)
        where TUniformData : unmanaged;

    ISampler CreateSampler(SamplerDescriptor samplerDescriptor);

    SamplerBuilder CreateSamplerBuilder();

    IComputePipelineBuilder CreateComputePipelineBuilder();

    IGraphicsPipelineBuilder CreateGraphicsPipelineBuilder();

    ITexture CreateTexture(TextureCreateDescriptor textureCreateDescriptor);

    ITexture CreateTexture2D(int width, int height, Format format, Label? label = null);

    void EndRender();
}