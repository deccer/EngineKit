using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using EngineKit.Extensions;
using EngineKit.Native.OpenGL;
using OpenTK.Mathematics;

namespace EngineKit.Graphics;

internal sealed class GraphicsContext : IGraphicsContext, IInternalGraphicsContext
{
    private readonly IFramebufferFactory _framebufferFactory;
    private readonly IDictionary<IPipeline, GraphicsPipelineDescriptor> _graphicsPipelineCache;
    private readonly IDictionary<IPipeline, ComputePipelineDescriptor> _computePipelineCache;
    private readonly IDictionary<int, IInputLayout> _inputLayoutCache;
    private uint? _currentFramebuffer;

    private PrimitiveTopology _currentPrimitiveTopology;

    public GraphicsContext(IFramebufferFactory framebufferFactory)
    {
        _framebufferFactory = framebufferFactory;
        _graphicsPipelineCache = new Dictionary<IPipeline, GraphicsPipelineDescriptor>(16);
        _computePipelineCache = new Dictionary<IPipeline, ComputePipelineDescriptor>(16);
        _inputLayoutCache = new Dictionary<int, IInputLayout>(16);
    }

    public void Dispose()
    {
        _framebufferFactory.Dispose();
    }

    public Result<IComputePipeline> CreateComputePipeline(ComputePipelineDescriptor computePipelineDescriptor)
    {
        var computePipeline = new ComputePipeline(computePipelineDescriptor);
        var compileShaderResult = computePipeline.LinkPrograms();
        if (compileShaderResult.IsFailure)
        {
            return Result.Failure<IComputePipeline>(compileShaderResult.Error);
        }

        _computePipelineCache[computePipeline] = computePipelineDescriptor;

        return Result.Success<IComputePipeline>(computePipeline);
    }

    public IGraphicsPipelineBuilder CreateGraphicsPipelineBuilder()
    {
        return new GraphicsPipelineBuilder(this);
    }

    public IComputePipelineBuilder CreateComputePipelineBuilder()
    {
        return new ComputePipelineBuilder(this);
    }

    public Result<IGraphicsPipeline> CreateGraphicsPipeline(GraphicsPipelineDescriptor graphicsPipelineDescriptor)
    {
        var vertexInputHashCode = graphicsPipelineDescriptor.VertexInput.VertexBindingDescriptors.GetHashCode();
        if (!_inputLayoutCache.TryGetValue(vertexInputHashCode, out var inputLayout))
        {
            inputLayout = new InputLayout(graphicsPipelineDescriptor.VertexInput);
            _inputLayoutCache.Add(vertexInputHashCode, inputLayout);
        }

        var graphicsPipeline = new GraphicsPipeline(graphicsPipelineDescriptor);

        /*
         * TODO(deccer) this is not cool, it needs to be set in a way that i dont have to use an internal, but also
         * dont want to expose InputLayout to the consuming side
         */
        graphicsPipeline.CurrentInputLayout = inputLayout;

        var compileShaderResult = graphicsPipeline.LinkPrograms();
        if (compileShaderResult.IsFailure)
        {
            return Result.Failure<IGraphicsPipeline>(compileShaderResult.Error);
        }

        _graphicsPipelineCache[graphicsPipeline] = graphicsPipelineDescriptor;

        return Result.Success<IGraphicsPipeline>(graphicsPipeline);
    }

    public IVertexBuffer CreateVertexBuffer<TVertex>(
        Label label,
        uint size) where TVertex: unmanaged
    {
        return new VertexBuffer<TVertex>(label, size);
    }

    public IVertexBuffer CreateVertexBuffer<TVertex>(
        Label label,
        TVertex[] vertices) where TVertex : unmanaged
    {
        return new VertexBuffer<TVertex>(label, vertices);
    }

    public IVertexBuffer CreateVertexBuffer(MeshData[] meshDates, VertexType targetVertexType)
    {
        var bufferData = new List<VertexPositionNormalUvTangent>(1_024_000);
        foreach (var meshData in meshDates)
        {
            if (!meshData.RealTangents.Any())
            {
                meshData.CalculateTangents();
            }

            for (var i = 0; i < meshData.Positions.Count; ++i)
            {
                bufferData.Add(new VertexPositionNormalUvTangent(
                    meshData.Positions[i],
                    meshData.Normals[i],
                    meshData.Uvs[i],
                    meshData.RealTangents[i]));
            }
        }

        return new VertexBuffer<VertexPositionNormalUvTangent>("Vertices", bufferData.ToArray());
    }

    public IIndexBuffer CreateIndexBuffer(MeshData[] meshDates)
    {
        var indices = meshDates
            .SelectMany(meshData => meshData.Indices)
            .ToArray();
        return new IndexBuffer<uint>("Indices", indices);
    }

    public IIndexBuffer CreateIndexBuffer<TIndex>(
        Label label,
        uint size) where TIndex : unmanaged
    {
        return new IndexBuffer<TIndex>(label, size);
    }

    public IIndexBuffer CreateIndexBuffer<TIndex>(
        Label label,
        TIndex[] indices) where TIndex : unmanaged
    {
        return new IndexBuffer<TIndex>(label, indices);
    }

    public IUniformBuffer CreateUniformBuffer<TUniformData>(
        Label label,
        TUniformData uniformData)
        where TUniformData : unmanaged
    {
        return new UniformBuffer<TUniformData>(label, uniformData);
    }

    public IShaderStorageBuffer CreateShaderStorageBuffer<TShaderStorageData>(
        Label label,
        TShaderStorageData[] shaderStorageData)
        where TShaderStorageData : unmanaged
    {
        return new ShaderStorageBuffer<TShaderStorageData>(label, shaderStorageData);
    }

    public IShaderStorageBuffer CreateShaderStorageBuffer<TShaderStorageData>(
        Label label,
        uint size)
        where TShaderStorageData : unmanaged
    {
        return new ShaderStorageBuffer<TShaderStorageData>(label, size);
    }

    public IIndirectBuffer CreateIndirectBuffer(Label label,
        GpuIndirectElementData[] indirectElementData)
    {
        return new IndirectBuffer(label, indirectElementData);
    }

    public ITexture CreateTexture(TextureCreateDescriptor textureCreateDescriptor)
    {
        return new Texture(textureCreateDescriptor);
    }

    public ITexture CreateTexture2D(int width, int height, Format format, Label? label)
    {
        var textureCreateDescriptor = new TextureCreateDescriptor
        {
            Format = format,
            Size = new Vector3i(width, height, 1),
            Label = string.IsNullOrEmpty(label) ? $"T_{width}x{height}_{format}" : $"T_{width}x{height}_{format}_{label}",
            ArrayLayers = 0,
            MipLevels = 1,
            ImageType = ImageType.Texture2D,
            SampleCount = SampleCount.OneSample
        };
        return CreateTexture(textureCreateDescriptor);
    }

    public bool BindComputePipeline(IComputePipeline computePipeline)
    {
        if (_computePipelineCache.TryGetValue(computePipeline, out var computePipelineDescriptor))
        {
            return false;
        }

        computePipeline.Bind();

        return true;
    }

    public bool BindGraphicsPipeline(IGraphicsPipeline graphicsPipeline)
    {
        if (!_graphicsPipelineCache.TryGetValue(graphicsPipeline, out var graphicsPipelineDescriptor))
        {
            return false;
        }

        graphicsPipeline.Bind();

        GL.EnableWhen(
            GL.EnableType.PrimitiveRestart,
            graphicsPipelineDescriptor.InputAssembly.IsPrimitiveRestartEnabled);

        _currentPrimitiveTopology = graphicsPipelineDescriptor.InputAssembly.PrimitiveTopology;

        var rasterizationDescriptor = graphicsPipelineDescriptor.RasterizationDescriptor;
        GL.EnableWhen(GL.EnableType.DepthClamp, rasterizationDescriptor.IsDepthClampEnabled);
        GL.PolygonMode(GL.PolygonModeType.FrontAndBack, rasterizationDescriptor.FillMode.ToGL());
        GL.EnableWhen(GL.EnableType.CullFace, rasterizationDescriptor.IsCullingEnabled);
        if (rasterizationDescriptor.IsCullingEnabled)
        {
            GL.CullFace(rasterizationDescriptor.CullMode.ToGL());
        }

        GL.FrontFace(rasterizationDescriptor.FaceWinding.ToGL());

        GL.EnableWhen(GL.EnableType.PolygonOffsetFill, rasterizationDescriptor.IsDepthBiasEnabled);
        GL.EnableWhen(GL.EnableType.PolygonOffsetLine, rasterizationDescriptor.IsDepthBiasEnabled);
        GL.EnableWhen(GL.EnableType.PolygonOffsetPoint, rasterizationDescriptor.IsDepthBiasEnabled);
        if (rasterizationDescriptor.IsDepthBiasEnabled)
        {
            GL.PolygonOffset(
                rasterizationDescriptor.DepthBiasSlopeFactor,
                rasterizationDescriptor.DepthBiasConstantFactor);
        }

        if (Math.Abs(rasterizationDescriptor.LineWidth - 1.0f) < 0.01f)
        {
            GL.LineWidth(rasterizationDescriptor.LineWidth);
        }

        if (Math.Abs(rasterizationDescriptor.PointSize - 1.0f) < 0.01f)
        {
            GL.PointSize(rasterizationDescriptor.PointSize);
        }

        var depthStencilDescriptor = graphicsPipelineDescriptor.DepthStencilDescriptor;
        GL.EnableWhen(GL.EnableType.DepthTest, depthStencilDescriptor.IsDepthTestEnabled);
        GL.DepthMask(depthStencilDescriptor.IsDepthWriteEnabled);
        GL.DepthFunc(depthStencilDescriptor.DepthCompareOperation.ToGL());

        var colorBlendDescriptor = graphicsPipelineDescriptor.ColorBlendDescriptor;
        GL.EnableWhen(GL.EnableType.Blend, colorBlendDescriptor.ColorBlendAttachmentDescriptors.Any(blendAttachmentDescriptor => blendAttachmentDescriptor.IsBlendEnabled));
        GL.BlendColor(
            colorBlendDescriptor.BlendConstants[0],
            colorBlendDescriptor.BlendConstants[1],
            colorBlendDescriptor.BlendConstants[2],
            colorBlendDescriptor.BlendConstants[3]);

        for (uint i = 0; i < colorBlendDescriptor.ColorBlendAttachmentDescriptors.Length; i++)
        {
            var colorBlendAttachment = colorBlendDescriptor.ColorBlendAttachmentDescriptors[i];

            GL.BlendFuncSeparatei(
                i,
                colorBlendAttachment.SourceColorBlendFactor.ToGL(),
                colorBlendAttachment.DestinationColorBlendFactor.ToGL(),
                colorBlendAttachment.SourceAlphaBlendFactor.ToGL(),
                colorBlendAttachment.DestinationAlphaBlendFactor.ToGL());
            GL.BlendEquationSeparatei(
                i,
                colorBlendAttachment.ColorBlendOperation.ToGL(),
                colorBlendAttachment.AlphaBlendOperation.ToGL());
            GL.ColorMaski(
                i,
                (colorBlendAttachment.ColorWriteMask & ColorMask.Red) != ColorMask.None,
                (colorBlendAttachment.ColorWriteMask & ColorMask.Green) != ColorMask.None,
                (colorBlendAttachment.ColorWriteMask & ColorMask.Blue) != ColorMask.None,
                (colorBlendAttachment.ColorWriteMask & ColorMask.Alpha) != ColorMask.None);
        }

        return true;
    }

    public void BeginRenderToSwapchain(SwapchainRenderDescriptor swapchainRenderDescriptor)
    {
        GL.BindFramebuffer(GL.FramebufferTarget.Framebuffer, 0);
        GL.ClearBufferMask clearBufferMask = 0;
        if (swapchainRenderDescriptor.ClearColor)
        {
            GL.ClearColor(
                swapchainRenderDescriptor.ClearColorValue.ColorFloat[0],
                swapchainRenderDescriptor.ClearColorValue.ColorFloat[1],
                swapchainRenderDescriptor.ClearColorValue.ColorFloat[2],
                swapchainRenderDescriptor.ClearColorValue.ColorFloat[3]);
            clearBufferMask |= GL.ClearBufferMask.ColorBufferBit;
        }

        if (swapchainRenderDescriptor.ClearDepth)
        {
            GL.ClearDepth(swapchainRenderDescriptor.ClearDepthValue);
            clearBufferMask |= GL.ClearBufferMask.DepthBufferBit;
        }

        if (swapchainRenderDescriptor.ClearStencil)
        {
            GL.ClearStencil(swapchainRenderDescriptor.ClearStencilValue);
            clearBufferMask |= GL.ClearBufferMask.StencilBufferBit;
        }

        if (clearBufferMask != 0)
        {
            EnableAllMaskStates();
            GL.Clear(clearBufferMask);
        }

        if (swapchainRenderDescriptor.ScissorRect.HasValue)
        {
            var v = swapchainRenderDescriptor.ScissorRect.Value;
            var x = new EngineKit.Mathematics.Viewport(v.X, v.Y, v.Z, v.W);
            GL.Scissor(x);
        }

        var vv = swapchainRenderDescriptor.Viewport;
        var xx = new EngineKit.Mathematics.Viewport(vv.X, vv.Y, vv.Z, vv.W);
        GL.Viewport(xx);
    }

    public void BeginRenderToFramebuffer(FramebufferRenderDescriptor framebufferRenderDescriptor)
    {
        _currentFramebuffer = _framebufferFactory.GetOrCreateFramebuffer(framebufferRenderDescriptor);
        GL.BindFramebuffer(GL.FramebufferTarget.Framebuffer, _currentFramebuffer.Value);
        EnableAllMaskStates();

        for (var i = 0; i < framebufferRenderDescriptor.ColorAttachments.Length; i++)
        {
            var colorAttachment = framebufferRenderDescriptor.ColorAttachments[i];
            if (colorAttachment.Clear)
            {
                var format = colorAttachment.Texture.Format;
                var formatBaseType = format.ToFormatBaseType();
                switch (formatBaseType)
                {
                    case FormatBaseType.Float:
                        GL.ClearNamedFramebuffer(
                            _currentFramebuffer.Value,
                            GL.Buffer.Color,
                            i,
                            colorAttachment.ClearValue.Color.ColorFloat[0]);
                        break;
                    case FormatBaseType.SignedInteger:
                        GL.ClearNamedFramebuffer(
                            _currentFramebuffer.Value,
                            GL.Buffer.Color,
                            i,
                            colorAttachment.ClearValue.Color.ColorSignedInteger[0]);
                        break;
                    case FormatBaseType.UnsignedInteger:
                        GL.ClearNamedFramebuffer(
                            _currentFramebuffer.Value,
                            GL.Buffer.Color,
                            i,
                            colorAttachment.ClearValue.Color.ColorUnsignedInteger[0]);
                        break;
                }
            }
        }

        if (framebufferRenderDescriptor.DepthAttachment.HasValue &&
            framebufferRenderDescriptor.DepthAttachment.Value.Clear)
        {
            GL.ClearNamedFramebuffer(
                _currentFramebuffer.Value,
                GL.Buffer.Depth,
                0,
                framebufferRenderDescriptor.DepthAttachment.Value.ClearValue.DepthStencil.Depth);
        }

        // TODO(deccer) clear stencil
        // TODO(deccer) depth & stencil

        var vv = framebufferRenderDescriptor.Viewport;
        var xx = new EngineKit.Mathematics.Viewport(vv.X, vv.Y, vv.Z, vv.W);
        GL.Viewport(xx);
        /*
        GL.DepthRange(
            framebufferRenderDescriptor.Viewport.MinDepth,
            framebufferRenderDescriptor.Viewport.MaxDepth);
            */
    }

    public void EndRender()
    {
    }

    public void BlitFramebufferToSwapchain(
        int sourceWidth,
        int sourceHeight,
        int targetWidth,
        int targetHeight)
    {
        if (!_currentFramebuffer.HasValue)
        {
            return;
        }

        GL.BlitNamedFramebuffer(
            _currentFramebuffer.Value,
            0,
            0,
            0,
            sourceWidth,
            sourceHeight,
            0,
            0,
            targetWidth,
            targetHeight,
            GL.ClearBufferMask.ColorBufferBit,
            GL.BlitFramebufferFilter.Nearest);
    }

    private static void EnableAllMaskStates()
    {
        GL.ColorMask(true, true, true, true);
        GL.DepthMask(true);
        GL.StencilMask(true);
    }
}