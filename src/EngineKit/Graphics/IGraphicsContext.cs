using System;
using CSharpFunctionalExtensions;

namespace EngineKit.Graphics;

public interface IGraphicsContext : IDisposable
{
    bool BindComputePipeline(ComputePipeline computePipeline);

    bool BindGraphicsPipeline(IGraphicsPipeline graphicsPipeline);

    void BeginRenderToSwapchain(SwapchainRenderDescriptor swapchainRenderDescriptor);

    void BeginRenderToFramebuffer(FramebufferRenderDescriptor framebufferRenderDescriptor);

    void BlitFramebufferToSwapchain(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight);

    IVertexBuffer CreateVertexBuffer<TVertex>(
        Label label,
        uint size) where TVertex : unmanaged;

    IVertexBuffer CreateVertexBuffer<TVertex>(
        Label label,
        TVertex[] vertices) where TVertex : unmanaged;

    IIndexBuffer CreateIndexBuffer<TIndex>(
        Label label,
        uint size) where TIndex : unmanaged;

    IIndexBuffer CreateIndexBuffer<TIndex>(
        Label label,
        TIndex[] indices) where TIndex : unmanaged;

    IUniformBuffer CreateUniformBuffer<TUniformData>(
        Label label,
        TUniformData uniformData)
        where TUniformData : unmanaged;

    Result<ComputePipeline> CreateComputePipeline(ComputePipelineDescriptor computePipelineDescriptor);

    Result<IGraphicsPipeline> CreateGraphicsPipeline(GraphicsPipelineDescriptor graphicsPipelineDescriptor);

    ITexture CreateTexture(TextureCreateDescriptor textureCreateDescriptor);

    ITexture CreateTexture2D(int width, int height, Format format, Label? label = null);

    void EndRender();
}