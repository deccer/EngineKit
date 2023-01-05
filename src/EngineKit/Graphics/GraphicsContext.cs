using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using EngineKit.Extensions;
using EngineKit.Native.OpenGL;
using OpenTK.Mathematics;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EngineKit.Graphics;

internal sealed class GraphicsContext : IGraphicsContext, IInternalGraphicsContext
{
    private readonly ILogger _logger;
    private readonly IFramebufferCache _framebufferCache;
    private readonly IDictionary<IPipeline, GraphicsPipelineDescriptor> _graphicsPipelineCache;
    private readonly IDictionary<IPipeline, ComputePipelineDescriptor> _computePipelineCache;
    private readonly IDictionary<int, IInputLayout> _inputLayoutCache;
    private readonly IList<string> _extensions;
    private uint? _currentFramebuffer;

    public GraphicsContext(ILogger logger, IFramebufferCache framebufferCache)
    {
        _logger = logger.ForContext<GraphicsContext>();
        _framebufferCache = framebufferCache;
        _graphicsPipelineCache = new Dictionary<IPipeline, GraphicsPipelineDescriptor>(16);
        _computePipelineCache = new Dictionary<IPipeline, ComputePipelineDescriptor>(16);
        _inputLayoutCache = new Dictionary<int, IInputLayout>(16);
        _extensions = new List<string>(512);
    }

    public void Dispose()
    {
        _framebufferCache.Dispose();
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

    public IIndexBuffer CreateIndexBuffer(Label label, MeshData[] meshDates)
    {
        var indices = meshDates
            .SelectMany(meshData => meshData.Indices)
            .ToArray();
        var indexBuffer = new IndexBuffer<uint>(label);
        indexBuffer.AllocateStorage(indices, StorageAllocationFlags.None);
        return indexBuffer;
    }

    public IIndexBuffer CreateIndexBuffer<TIndex>(Label label)
        where TIndex : unmanaged
    {
        return new IndexBuffer<TIndex>(label);
    }

    public IIndirectBuffer CreateIndirectBuffer(Label label)
    {
        return new IndirectBuffer(label);
    }

    public IShaderStorageBuffer CreateShaderStorageBuffer<TShaderStorageData>(Label label)
        where TShaderStorageData : unmanaged
    {
        return new ShaderStorageBuffer<TShaderStorageData>(label);
    }

    public IUniformBuffer CreateUniformBuffer<TUniformData>(Label label)
        where TUniformData: unmanaged
    {
        return new UniformBuffer<TUniformData>(label);
    }

    public IVertexBuffer CreateVertexBuffer<TVertex>(Label label)
        where TVertex : unmanaged
    {
        return new VertexBuffer<TVertex>(label);
    }

    public IVertexBuffer CreateVertexBuffer(
        Label label,
        MeshData[] meshDates,
        VertexType targetVertexType)
    {
        //TODO(deccer) return vertexbuffers depending on targetVertexType
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

        var vertexBuffer = new VertexBuffer<VertexPositionNormalUvTangent>(label);
        vertexBuffer.AllocateStorage(bufferData.ToArray(), StorageAllocationFlags.None);
        return vertexBuffer;
    }

    public ISampler CreateSampler(SamplerDescriptor samplerDescriptor)
    {
        return new Sampler(samplerDescriptor);
    }

    public SamplerBuilder CreateSamplerBuilder()
    {
        return new SamplerBuilder(this);
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

    public ITexture? CreateTextureFromFile(string filePath, bool generateMipmaps = true)
    {
        if (!File.Exists(filePath))
        {
           _logger.Error("{Category}: Unable to load image from file '{FilePath}'", "App", filePath);
            return null;
        }
        using var image = Image.Load<Rgba32>(filePath);

        var calculatedMipLevels = (uint)(1 + MathF.Ceiling(MathF.Log2(MathF.Min(image.Width, image.Height))));
        var textureCreateDescriptor = new TextureCreateDescriptor
        {
            ImageType = ImageType.Texture2D,
            Format = Format.R8G8B8A8UNorm,
            Label = Path.GetFileNameWithoutExtension(filePath),
            Size = new Vector3i(image.Width, image.Height, 1),
            MipLevels = generateMipmaps ? calculatedMipLevels : 1,
            SampleCount = SampleCount.OneSample
        };

        var texture = CreateTexture(textureCreateDescriptor);
        var textureUpdateDescriptor = new TextureUpdateDescriptor
        {
            Offset = Vector3i.Zero,
            Size = new Vector3i(image.Width, image.Height, 1),
            UploadDimension = UploadDimension.Two,
            UploadFormat = UploadFormat.RedGreenBlueAlpha,
            UploadType = UploadType.UnsignedByte,
            Level = 0,
        };

        if (image.DangerousTryGetSinglePixelMemory(out var imageMemory))
        {
            texture.Update(textureUpdateDescriptor, imageMemory.Pin());
            if (generateMipmaps)
            {
                texture.GenerateMipmaps();
            }
        }

        return texture;
    }

    public ITexture? CreateTextureCubeFromFile(string[] filePaths)
    {
        if (filePaths.Length != 6)
        {
            _logger.Error("{Category}: Unable to load skybox texture, 6 file paths must be provided", "App");
            return null;
        }

        var slice = 0;
        ITexture? textureCube = null;
        foreach (var filePath in filePaths)
        {
            using var image = Image.Load<Rgba32>(filePath);

            if (slice == 0)
            {
                var skyboxTextureCreateDescriptor = new TextureCreateDescriptor
                {
                    ImageType = ImageType.TextureCube,
                    Format = Format.R8G8B8A8UNorm,
                    Label = "Skybox",
                    Size = new Vector3i(image.Width, image.Height, 1),
                    MipLevels = 10,
                    SampleCount = SampleCount.OneSample
                };
                textureCube = CreateTexture(skyboxTextureCreateDescriptor);
            }

            var skyboxTextureUpdateDescriptor = new TextureUpdateDescriptor
            {
                Offset = new Vector3i(0, 0, slice++),
                Size = new Vector3i(image.Width, image.Height, 1),
                UploadDimension = UploadDimension.Three,
                UploadFormat = UploadFormat.RedGreenBlueAlpha,
                UploadType = UploadType.UnsignedByte,
                Level = 0,
            };

            if (image.DangerousTryGetSinglePixelMemory(out var imageMemory))
            {
                textureCube!.Update(skyboxTextureUpdateDescriptor, imageMemory.Pin());
            }
        }

        textureCube?.GenerateMipmaps();

        return textureCube;
    }

    public bool BindComputePipeline(IComputePipeline computePipeline)
    {
        if (!_computePipelineCache.TryGetValue(computePipeline, out var computePipelineDescriptor))
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
            GL.Scissor(swapchainRenderDescriptor.ScissorRect.Value);
        }

        GL.Viewport(swapchainRenderDescriptor.Viewport);
    }

    public void BeginRenderToFramebuffer(FramebufferRenderDescriptor framebufferRenderDescriptor)
    {
        _currentFramebuffer = _framebufferCache.GetOrCreateFramebuffer(framebufferRenderDescriptor);
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
        GL.Viewport(framebufferRenderDescriptor.Viewport);
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

    public void InsertMemoryBarrier(BarrierMask mask)
    {
        GL.MemoryBarrier(mask.ToGL());
    }

    public void RemoveFramebuffer(FramebufferRenderDescriptor framebufferRenderDescriptor)
    {
        _framebufferCache.RemoveFramebuffer(framebufferRenderDescriptor);
    }

    public bool IsExtensionSupported(string extensionName)
    {
        return _extensions.Contains(extensionName);
    }

    public void LoadExtensions()
    {
        var extensionCount = GL.GetInteger((uint)GL.GetName.NumExtensions);
        for (var i = 0u; i < extensionCount; i++)
        {
            _extensions.Add(GL.GetString(GL.StringName.Extensions, i));
        }
    }
}