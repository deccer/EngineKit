﻿using System;

namespace EngineKit.Graphics;

public interface IGraphicsContext : IDisposable
{
    bool BindComputePipeline(IComputePipeline computePipeline);

    bool BindGraphicsPipeline(IGraphicsPipeline graphicsPipeline);

    void BeginRenderToSwapchain(SwapchainRenderDescriptor swapchainRenderDescriptor);

    void BeginRenderToFramebuffer(FramebufferRenderDescriptor framebufferRenderDescriptor);

    void BlitFramebufferToSwapchain(
        int sourceWidth,
        int sourceHeight,
        int targetWidth,
        int targetHeight);

    IIndexBuffer CreateIndexBuffer<TIndex>(Label label)
        where TIndex : unmanaged;

    IIndexBuffer CreateIndexBuffer(
        Label label,
        MeshData[] meshDates);

    IIndirectBuffer CreateIndirectBuffer(Label label);

    IShaderStorageBuffer CreateShaderStorageBuffer<TShaderStorageData>(Label label)
        where TShaderStorageData : unmanaged;

    IUniformBuffer CreateUniformBuffer<TUniformData>(Label label)
        where TUniformData: unmanaged;

    IVertexBuffer CreateVertexBuffer<TVertex>(Label label)
        where TVertex : unmanaged;

    IVertexBuffer CreateVertexBuffer(
        Label label,
        MeshData[] meshDates,
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

    ITexture? CreateTextureFromFile(string filePath, bool generateMipmaps = true);

    ITexture? CreateTextureCubeFromFile(string[] filePaths);

    void EndRender();

    void InsertMemoryBarrier(BarrierMask mask);
}