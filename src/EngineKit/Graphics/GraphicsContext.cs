using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EngineKit.Core;
using EngineKit.Extensions;
using EngineKit.Graphics.Assets;
using EngineKit.Graphics.RHI;
using EngineKit.Graphics.Shaders;
using EngineKit.Mathematics;
using EngineKit.Native.Ktx;
using EngineKit.Native.OpenGL;
//using Ktx2Sharp;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EngineKit.Graphics;

internal sealed class GraphicsContext : IGraphicsContext
{
    private readonly ILogger _logger;
    private readonly IShaderProgramFactory _shaderProgramFactory;
    private readonly IFramebufferCache _framebufferCache;
    private readonly ICapabilities _capabilities;
    private readonly IImageLoader _imageLoader;
    private readonly Dictionary<IPipeline, GraphicsPipelineDescriptor> _graphicsPipelineCache;
    private readonly Dictionary<IPipeline, ComputePipelineDescriptor> _computePipelineCache;
    private readonly Dictionary<int, IInputLayout> _inputLayoutCache;
    private readonly FramebufferDescriptorBuilder _framebufferDescriptorBuilder;
    private readonly SwapchainDescriptorBuilder _swapchainDescriptorBuilder;
    private uint? _currentFramebuffer;
    private bool _srgbWasDisabled;
    private Viewport _currentFramebufferViewport;
    private Viewport _currentSwapchainViewport;
    private bool _isGraphicsPipelineDebugGroupActive;
    private bool _isComputePipelineDebugGroupActive;

    public GraphicsContext(
        ILogger logger,
        IShaderProgramFactory shaderProgramFactory,
        IFramebufferCache framebufferCache,
        ICapabilities capabilities,
        IImageLoader imageLoader,
        IApplicationContext applicationContext)
    {
        _logger = logger.ForContext<GraphicsContext>();
        _shaderProgramFactory = shaderProgramFactory;
        _framebufferCache = framebufferCache;
        _capabilities = capabilities;
        _imageLoader = imageLoader;
        _graphicsPipelineCache = new Dictionary<IPipeline, GraphicsPipelineDescriptor>(16);
        _computePipelineCache = new Dictionary<IPipeline, ComputePipelineDescriptor>(16);
        _inputLayoutCache = new Dictionary<int, IInputLayout>(16);
        _framebufferDescriptorBuilder = new FramebufferDescriptorBuilder();
        _swapchainDescriptorBuilder = new SwapchainDescriptorBuilder(applicationContext);
    }

    public void Dispose()
    {
        _framebufferCache.Dispose();
    }

    public void CopyTexture(
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
        BlitFramebufferFilter interpolationFilter)
    {
        var sourceFramebufferDescriptor = CreateSingleFramebufferDescriptorFromTexture(sourceTexture);
        var sourceFramebuffer = _framebufferCache.GetOrCreateFramebuffer(sourceFramebufferDescriptor);
        var targetFramebufferDescriptor = CreateSingleFramebufferDescriptorFromTexture(targetTexture);
        var targetFramebuffer = _framebufferCache.GetOrCreateFramebuffer(targetFramebufferDescriptor);

        GL.BlitNamedFramebuffer(
            sourceFramebuffer,
            targetFramebuffer,
            sourceOffsetX,
            sourceOffsetY,
            sourceWidth,
            sourceHeight,
            targetOffsetX,
            targetOffsetY,
            targetWidth,
            targetHeight,
            framebufferBit.ToGL(),
            interpolationFilter.ToGL());

        RemoveFramebuffer(sourceFramebufferDescriptor);
        RemoveFramebuffer(targetFramebufferDescriptor);
    }

    public void UseViewport(Viewport viewport)
    {
        GL.Viewport(viewport);
    }

    public IMeshPool CreateMeshPool(
        Label label,
        uint maxVertexCount,
        uint maxIndexCount)
    {
        return new MeshPool(label, this, maxVertexCount, maxIndexCount);
    }

    public IMaterialPool CreateMaterialPool(
        Label label,
        uint maxMaterialCount,
        ISamplerLibrary samplerLibrary)
    {
        return new MaterialPool(_logger, label, _capabilities, this, samplerLibrary, maxMaterialCount);
    }

    public IGraphicsPipelineBuilder CreateGraphicsPipelineBuilder()
    {
        return new GraphicsPipelineBuilder(_inputLayoutCache, _graphicsPipelineCache, _shaderProgramFactory);
    }

    public IComputePipelineBuilder CreateComputePipelineBuilder()
    {
        return new ComputePipelineBuilder(_computePipelineCache, _shaderProgramFactory);
    }

    public IBuffer CreateUntypedBuffer(
        Label label,
        nuint sizeInBytes,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
    {
        return new Buffer(label, sizeInBytes, bufferStorageFlags);
    }

    public IBuffer CreateUntypedBuffer(
        Label label,
        nuint sizeInBytes,
        nint dataPtr,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
    {
        return new Buffer(label, sizeInBytes, dataPtr, bufferStorageFlags);
    }

    public IBuffer CreateTypedBuffer<TElement>(
        Label label,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
        where TElement : unmanaged
    {
        return new TypedBuffer<TElement>(label, bufferStorageFlags);
    }

    public IBuffer CreateTypedBuffer<TElement>(
        Label label,
        TElement element,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
        where TElement : unmanaged
    {
        return new TypedBuffer<TElement>(label, element, bufferStorageFlags);
    }

    public IBuffer CreateTypedBuffer<TElement>(
        Label label,
        TElement[] elements,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
        where TElement : unmanaged
    {
        return new TypedBuffer<TElement>(label, elements, bufferStorageFlags);
    }

    public IBuffer CreateTypedBuffer<TElement>(
        Label label,
        uint elementCount,
        BufferStorageFlags bufferStorageFlags = BufferStorageFlags.None)
        where TElement : unmanaged
    {
        return new TypedBuffer<TElement>(label, elementCount, bufferStorageFlags);
    }

    public IBuffer CreateVertexBuffer(
        Label label,
        MeshPrimitive[] meshPrimitives)
    {
        var vertexCount = meshPrimitives.Sum(mp => mp.Positions.Count);
        var bufferData = new GpuVertexPositionNormalUvTangent[vertexCount];

        var bufferDataIndex = 0;
        foreach (var meshPrimitive in meshPrimitives)
        {
            if (meshPrimitive.RealTangents.Count == 0)
            {
                meshPrimitive.CalculateTangents();
            }

            for (var i = 0; i < meshPrimitive.Positions.Count; ++i)
            {
                bufferData[bufferDataIndex++] = new GpuVertexPositionNormalUvTangent(
                    meshPrimitive.Positions[i],
                    meshPrimitive.Normals[i],
                    meshPrimitive.Uvs[i],
                    meshPrimitive.RealTangents[i]);
            }
        }

        return new TypedBuffer<GpuVertexPositionNormalUvTangent>(label, in bufferData);
    }

    public IBuffer CreateIndexBuffer(Label label, MeshPrimitive[] meshPrimitives)
    {
        var indices = meshPrimitives
            .SelectMany(meshPrimitive => meshPrimitive.Indices)
            .ToArray();
        return new TypedBuffer<uint>(label, in indices);
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

    public ITexture CreateTexture2D(
        Int2 extent,
        Format format,
        Label? label)
    {
        return CreateTexture2D(extent.X, extent.Y, format, label);
    }

    public ITexture CreateTexture2D(
        int width,
        int height,
        Format format,
        Label? label)
    {
        var textureCreateDescriptor = new TextureCreateDescriptor
        {
            Format = format,
            Size = new Int3(width, height, 1),
            Label = string.IsNullOrEmpty(label) ? $"Texture-{width}x{height}-{format}" : $"Texture-{width}x{height}-{format}-{label}",
            ArrayLayers = 0,
            MipLevels = 1,
            TextureType = TextureType.Texture2D,
            TextureSampleCount = TextureSampleCount.OneSample
        };
        return CreateTexture(textureCreateDescriptor);
    }

    public ITexture? CreateTextureFromFile(
        string filePath,
        Format format,
        bool generateMipmaps,
        bool flipVertical,
        bool flipHorizontal)
    {
        if (!File.Exists(filePath))
        {
           _logger.Error("{Category}: Unable to load image from file '{FilePath}'", "App", filePath);
            return null;
        }

        using var image = _imageLoader.LoadImageFromFile<Rgba32>(filePath, flipVertical, flipHorizontal) as Image<Rgba32>;
        if (image == null)
        {
            _logger.Error("{Category}: Unable to load image from file '{FilePath}'", "App", filePath);
            return null;
        }

        return CreateTextureFromImage(image, Path.GetFileNameWithoutExtension(filePath), format, generateMipmaps);
    }

    public ITexture? CreateTextureFromMemory(ImageInformation imageInformation,
        Format format,
        Label label,
        bool generateMipmaps,
        bool flipVertical,
        bool flipHorizontal)
    {
        if (!imageInformation.ImageData.HasValue)
        {
            _logger.Error("{Category}: Unable to load image from memory. Provided pixelBytes are empty", "App");
            return null;
        }

        if (imageInformation.MimeType == "image/ktx2")
        {
            unsafe
            {
                var ktxTexture = Ktx.LoadFromMemory(imageInformation.ImageData.Value);
                if (ktxTexture == null)
                {
                    _logger.Error("{Category}: Unable to load image from memory", "App");
                    return null;
                }

                if (ktxTexture->CompressionScheme != Ktx.SuperCompressionScheme.None ||
                    Ktx.NeedsTranscoding(ktxTexture))
                {
                    var transcodeResult = Ktx.Transcode(ktxTexture, Ktx.TranscodeFormat.Bc7Rgba, Ktx.TranscodeFlagBits.HighQuality);
                    if (transcodeResult != Ktx.KtxErrorCode.KtxSuccess)
                    {
                        _logger.Error("{Category}: Unable to load image from memory. Transcoding was not successful ({TranscodeResult})", "App", transcodeResult);
                        Ktx.Destroy(ktxTexture);
                        return null;
                    }
                }

                var texture = CreateTextureFromKtxTexture(ktxTexture, label);

                Ktx.Destroy(ktxTexture);

                return texture;
            }
        }

        using var image = _imageLoader.LoadImageFromMemory<Rgba32>(
            imageInformation.ImageData.Value.Span,
            flipVertical,
            flipHorizontal) as Image<Rgba32>;
        if (image == null)
        {
            _logger.Error("{Category}: Unable to load image from memory", "App");
            return null;
        }

        return CreateTextureFromImage(image, label, format, generateMipmaps);
    }

    public ITexture? CreateTextureCubeFromFiles(
        Label label,
        string[] filePaths,
        bool flipVertical = true,
        bool flipHorizontal = false)
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
            using var image = _imageLoader.LoadImageFromFile<Rgba32>(filePath, flipVertical, flipHorizontal) as Image<Rgba32>;
            if (image == null)
            {
                _logger.Error("{Category}: Unable to load skybox texture part {FilePath}", "App", filePath);
                continue;
            }
            if (slice == 0)
            {
                var format = Format.R16G16B16A16Float;
                var skyboxTextureCreateDescriptor = new TextureCreateDescriptor
                {
                    TextureType = TextureType.TextureCube,
                    Format = format,
                    Label = string.IsNullOrEmpty(label)
                        ? $"Texture-{image.Width}x{image.Height}-{format}"
                        : $"Texture-{image.Width}x{image.Height}-{format}-{label}",
                    Size = new Int3(image.Width, image.Height, 1),
                    MipLevels = 10,
                    TextureSampleCount = TextureSampleCount.OneSample
                };
                textureCube = CreateTexture(skyboxTextureCreateDescriptor);
            }

            var skyboxTextureUpdateDescriptor = new TextureUpdateDescriptor
            {
                Offset = new Int3(0, 0, slice++),
                Size = new Int3(image.Width, image.Height, 1),
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

    public IFramebufferDescriptorBuilder GetFramebufferDescriptorBuilder()
    {
        return _framebufferDescriptorBuilder.Reset();
    }

    public ISwapchainDescriptorBuilder GetSwapchainDescriptorBuilder()
    {
        return _swapchainDescriptorBuilder.Reset();
    }

    public FramebufferDescriptor CreateSingleFramebufferDescriptorFromTexture(ITexture texture)
    {
        var framebufferDescriptor = new FramebufferDescriptor();
        if (texture.TextureCreateDescriptor.Format.IsColorFormat())
        {
            framebufferDescriptor.ColorAttachments = new[]
            {
                new FramebufferRenderAttachment(texture, ClearValue.Zero, false)
            };
        }
        else if (texture.TextureCreateDescriptor.Format.IsDepthFormat())
        {
            framebufferDescriptor.DepthAttachment = new FramebufferRenderAttachment(texture, ClearValue.Zero, false);
        }
        else if (texture.TextureCreateDescriptor.Format.IsStencilFormat())
        {
            framebufferDescriptor.StencilAttachment = new FramebufferRenderAttachment(texture, ClearValue.Zero, false);
        }

        framebufferDescriptor.Label = $"Framebuffer-Single-{texture.TextureCreateDescriptor.Size.X}x{texture.TextureCreateDescriptor.Size.Y}x{texture.TextureCreateDescriptor.Format}-{GetHashCode()}";

        return framebufferDescriptor;
    }

    public bool BindComputePipeline(IComputePipeline computePipeline)
    {
        if (!_computePipelineCache.TryGetValue(computePipeline, out var computePipelineDescriptor))
        {
            return false;
        }

        if (_isComputePipelineDebugGroupActive)
        {
            GL.PopDebugGroup();
            _isComputePipelineDebugGroupActive = false;
        }

        if (!string.IsNullOrEmpty(computePipelineDescriptor.PipelineProgramLabel))
        {
            GL.PushDebugGroup(computePipelineDescriptor.PipelineProgramLabel);
            _isComputePipelineDebugGroupActive = true;
        }

        if (computePipelineDescriptor.ClearResourceBindings)
        {
            ClearResourceBindings();
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

        if (graphicsPipelineDescriptor.ClearResourceBindings)
        {
            ClearResourceBindings();
        }
        graphicsPipeline.Bind();

        GL.EnableWhen(
            GL.EnableType.PrimitiveRestart,
            graphicsPipelineDescriptor.InputAssembly.IsPrimitiveRestartEnabled);

        var rasterizationDescriptor = graphicsPipelineDescriptor.RasterizationDescriptor;
        GL.ClipControl(GL.ClipControlOrigin.LowerLeft, rasterizationDescriptor.ClipControlDepth.ToGL());
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

        if (Math.Abs(rasterizationDescriptor.LineWidth - 1.0f) > float.Epsilon)
        {
            GL.LineWidth(rasterizationDescriptor.LineWidth);
        }

        if (Math.Abs(rasterizationDescriptor.PointSize - 1.0f) > float.Epsilon)
        {
            GL.PointSize(rasterizationDescriptor.PointSize);
        }

        var depthStencilDescriptor = graphicsPipelineDescriptor.DepthStencilDescriptor;
        GL.EnableWhen(GL.EnableType.DepthTest, depthStencilDescriptor.IsDepthTestEnabled);
        GL.DepthMask(depthStencilDescriptor.IsDepthWriteEnabled);
        GL.DepthFunc(depthStencilDescriptor.DepthCompareFunction.ToGL());
        GL.EnableWhen(GL.EnableType.DepthClamp, rasterizationDescriptor.IsDepthClampEnabled);
        if (rasterizationDescriptor.IsDepthBiasEnabled)
        {
            GL.PolygonOffset(
                rasterizationDescriptor.DepthBiasSlopeFactor,
                rasterizationDescriptor.DepthBiasConstantFactor);
        }

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
                colorBlendAttachment.SourceColorBlend.ToGL(),
                colorBlendAttachment.DestinationColorBlend.ToGL(),
                colorBlendAttachment.SourceAlphaBlend.ToGL(),
                colorBlendAttachment.DestinationAlphaBlend.ToGL());
            GL.BlendEquationSeparatei(
                i,
                colorBlendAttachment.ColorBlendFunction.ToGL(),
                colorBlendAttachment.AlphaBlendFunction.ToGL());
            GL.ColorMaski(
                i,
                (colorBlendAttachment.ColorWriteMask & ColorMask.Red) != ColorMask.None,
                (colorBlendAttachment.ColorWriteMask & ColorMask.Green) != ColorMask.None,
                (colorBlendAttachment.ColorWriteMask & ColorMask.Blue) != ColorMask.None,
                (colorBlendAttachment.ColorWriteMask & ColorMask.Alpha) != ColorMask.None);
        }

        return true;
    }

    public void BeginRenderPass(SwapchainDescriptor swapchainDescriptor)
    {
        if (_isComputePipelineDebugGroupActive)
        {
            GL.PopDebugGroup();
            _isComputePipelineDebugGroupActive = false;
        }

        if (!string.IsNullOrEmpty(swapchainDescriptor.Label))
        {
            GL.PushDebugGroup(swapchainDescriptor.Label);
            _isGraphicsPipelineDebugGroupActive = true;
        }

        GL.BindFramebuffer(GL.FramebufferTarget.Framebuffer, 0);
        GL.FramebufferBit framebufferBit = 0;
        if (swapchainDescriptor.ClearColor)
        {
            GL.ClearColor(
                swapchainDescriptor.ClearColorValue.ColorFloat[0],
                swapchainDescriptor.ClearColorValue.ColorFloat[1],
                swapchainDescriptor.ClearColorValue.ColorFloat[2],
                swapchainDescriptor.ClearColorValue.ColorFloat[3]);
            framebufferBit |= GL.FramebufferBit.ColorBufferBit;
        }

        if (swapchainDescriptor.ClearDepth)
        {
            GL.ClearDepth(swapchainDescriptor.ClearDepthValue);
            framebufferBit |= GL.FramebufferBit.DepthBufferBit;
        }

        if (swapchainDescriptor.ClearStencil)
        {
            GL.ClearStencil(swapchainDescriptor.ClearStencilValue);
            framebufferBit |= GL.FramebufferBit.StencilBufferBit;
        }

        if (framebufferBit != 0)
        {
            EnableAllMaskStates();
            GL.Clear(framebufferBit);
        }

        if (swapchainDescriptor.ScissorRect.HasValue)
        {
            GL.Scissor(swapchainDescriptor.ScissorRect.Value);
        }

        if (!swapchainDescriptor.EnableSrgb)
        {
            GL.Disable(GL.EnableType.FramebufferSrgb);
            _srgbWasDisabled = true;
        }

        if (_currentSwapchainViewport != swapchainDescriptor.Viewport)
        {
            _currentSwapchainViewport = swapchainDescriptor.Viewport;
            GL.Viewport(_currentSwapchainViewport);
        }
    }

    public void BeginRenderPass(FramebufferDescriptor framebufferDescriptor)
    {
        if (_isComputePipelineDebugGroupActive)
        {
            GL.PopDebugGroup();
            _isComputePipelineDebugGroupActive = false;
        }

        if (!string.IsNullOrEmpty(framebufferDescriptor.Label))
        {
            GL.PushDebugGroup(framebufferDescriptor.Label);
            _isGraphicsPipelineDebugGroupActive = true;
        }

        _currentFramebuffer = _framebufferCache.GetOrCreateFramebuffer(framebufferDescriptor);
        GL.BindFramebuffer(GL.FramebufferTarget.Framebuffer, _currentFramebuffer.Value);
        if (!framebufferDescriptor.HasSrgbEnabledAttachment())
        {
            GL.Disable(GL.EnableType.FramebufferSrgb);
        }

        EnableAllMaskStates();

        for (var i = 0; i < framebufferDescriptor.ColorAttachments.Length; i++)
        {
            var colorAttachment = framebufferDescriptor.ColorAttachments[i];
            if (colorAttachment.Clear)
            {
                var format = colorAttachment.Texture.TextureCreateDescriptor.Format;
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

        if (framebufferDescriptor.DepthAttachment.HasValue &&
            framebufferDescriptor.DepthAttachment.Value.Clear &&
            framebufferDescriptor.StencilAttachment.HasValue &&
            framebufferDescriptor.StencilAttachment.Value.Clear)
        {
            GL.ClearNamedFramebuffer(
                _currentFramebuffer.Value,
                GL.Buffer.DepthStencil,
                0,
                framebufferDescriptor.DepthAttachment.Value.ClearValue.DepthStencil.Depth,
                framebufferDescriptor.StencilAttachment.Value.ClearValue.DepthStencil.Stencil);
        }
        else if (framebufferDescriptor.DepthAttachment.HasValue &&
                 framebufferDescriptor.DepthAttachment.Value.Clear)
        {
            GL.ClearNamedFramebuffer(
                _currentFramebuffer.Value,
                GL.Buffer.Depth,
                0,
                framebufferDescriptor.DepthAttachment.Value.ClearValue.DepthStencil.Depth);
        }
        else if (framebufferDescriptor.StencilAttachment.HasValue &&
                 framebufferDescriptor.StencilAttachment.Value.Clear)
        {
            GL.ClearNamedFramebuffer(
                _currentFramebuffer.Value,
                GL.Buffer.Stencil,
                0,
                framebufferDescriptor.StencilAttachment.Value.ClearValue.DepthStencil.Depth);
        }

        if (_currentFramebufferViewport != framebufferDescriptor.Viewport)
        {
            _currentFramebufferViewport = framebufferDescriptor.Viewport;
            GL.Viewport(_currentFramebufferViewport);

            if (MathF.Abs(_currentFramebufferViewport.MinDepth - framebufferDescriptor.Viewport.MinDepth) > 0.0001f ||
                MathF.Abs(_currentFramebufferViewport.MaxDepth - framebufferDescriptor.Viewport.MaxDepth) > 0.0001f)
            {
                GL.DepthRange(
                    framebufferDescriptor.Viewport.MinDepth,
                    framebufferDescriptor.Viewport.MaxDepth);
            }
        }

        if (!framebufferDescriptor.HasSrgbEnabledAttachment())
        {
            GL.Disable(GL.EnableType.FramebufferSrgb);
            _srgbWasDisabled = true;
        }
    }

    public void EndRenderPass()
    {
        if (_srgbWasDisabled)
        {
            GL.Enable(GL.EnableType.FramebufferSrgb);
        }

        if (_isComputePipelineDebugGroupActive)
        {
            GL.PopDebugGroup();
            _isComputePipelineDebugGroupActive = false;
        }

        if (_isGraphicsPipelineDebugGroupActive)
        {
            GL.PopDebugGroup();
            _isGraphicsPipelineDebugGroupActive = false;
        }
    }

    public void BlitFramebuffer(
        FramebufferDescriptor sourceFramebufferDescriptor,
        FramebufferDescriptor targetFramebufferDescriptor,
        int sourceWidth,
        int sourceHeight,
        int targetWidth,
        int targetHeight)
    {
        var sourceFramebuffer = _framebufferCache.GetOrCreateFramebuffer(sourceFramebufferDescriptor);
        var targetFramebuffer = _framebufferCache.GetOrCreateFramebuffer(targetFramebufferDescriptor);

        GL.BlitNamedFramebuffer(
            sourceFramebuffer,
            targetFramebuffer,
            0,
            0,
            sourceWidth,
            sourceHeight,
            0,
            0,
            targetWidth,
            targetHeight,
            GL.FramebufferBit.ColorBufferBit,
            GL.BlitFramebufferFilter.Nearest);
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
            GL.FramebufferBit.ColorBufferBit,
            GL.BlitFramebufferFilter.Nearest);
    }

    public void InsertMemoryBarrier(BarrierMask mask)
    {
        GL.MemoryBarrier(mask.ToGL());
    }

    public uint CreateFramebuffer(FramebufferDescriptor framebufferDescriptor)
    {
        return _framebufferCache.GetOrCreateFramebuffer(framebufferDescriptor);
    }

    public void RemoveFramebuffer(FramebufferDescriptor framebufferDescriptor)
    {
        _framebufferCache.RemoveFramebuffer(framebufferDescriptor);
    }

    public void Finish()
    {
        GL.Finish();
    }

    private static void EnableAllMaskStates()
    {
        GL.ColorMask(true, true, true, true);
        GL.DepthMask(true);
        GL.StencilMask(true);
    }

    public void ClearResourceBindings()
    {
        for (var i = 0u; i < _capabilities.MaxImageUnits; i++)
        {
            GL.BindImageTexture(i, 0, 0, true, 0, GL.MemoryAccess.ReadWrite, GL.SizedInternalFormat.Rgba32f);
        }

        for (var i = 0u; i < _capabilities.MaxShaderStorageBlocks; i++)
        {
            GL.BindBufferRange(GL.BufferTarget.ShaderStorageBuffer, i, 0, 0, 0);
        }

        for (var i = 0u; i < _capabilities.MaxUniformBlocks; i++)
        {
            GL.BindBufferRange(GL.BufferTarget.UniformBuffer, i, 0, 0, 0);
        }

        for (var i = 0u; i < _capabilities.MaxCombinedTextureImageUnits; i++)
        {
            GL.BindTextureUnit(i, 0);
            GL.BindSampler(i, 0);
        }
    }

    private unsafe ITexture CreateTextureFromKtxTexture(Ktx.KtxTexture* ktxTexture, Label label)
    {
        var textureCreateDescriptor = new TextureCreateDescriptor
        {
            TextureType = TextureType.Texture2D,
            Format = ktxTexture->VulkanFormat.ToFormat(),
            Label = label,
            Size = new Int3((int)ktxTexture->BaseWidth, (int)ktxTexture->BaseHeight, (int)ktxTexture->BaseDepth),
            MipLevels = ktxTexture->NumLevels,
            TextureSampleCount = TextureSampleCount.OneSample
        };

        var texture = CreateTexture(textureCreateDescriptor);
        for (var mipLevel = 0; mipLevel < ktxTexture->NumLevels; mipLevel++)
        {
            var mipMapWidth = (int)MathF.Max(ktxTexture->BaseWidth >> mipLevel, 1u);
            var mipMapHeight = (int)MathF.Max(ktxTexture->BaseHeight >> mipLevel, 1u);

            var imageSize = Ktx.GetImageSize(ktxTexture, (uint)mipLevel);

            var textureUpdateDescriptor = new TextureUpdateDescriptor
            {
                Offset = new Int3(0, 0, (int)imageSize),
                Size = new Int3(mipMapWidth, mipMapHeight, 1),
                UploadDimension = UploadDimension.Two,
                UploadFormat = UploadFormat.RedGreenBlueAlpha,
                UploadType = UploadType.UnsignedByte,
                Level = mipLevel,
            };

            var imageOffset = Ktx.GetImageOffset(ktxTexture, (uint)mipLevel, 0, 0);

            texture.Update(textureUpdateDescriptor, new MemoryHandle((void*)(new IntPtr(ktxTexture->Data) + new IntPtr(imageOffset))));
        }

        return texture;
    }

    private ITexture CreateTextureFromImage(Image<Rgba32> image, Label label, Format format, bool generateMipmaps = true)
    {
        var calculatedMipLevels = (uint)(1 + MathF.Floor(MathF.Log2(MathF.Min(image.Width, image.Height))));
        var textureCreateDescriptor = new TextureCreateDescriptor
        {
            TextureType = TextureType.Texture2D,
            Format = format,
            Label = label,
            Size = new Int3(image.Width, image.Height, 1),
            MipLevels = generateMipmaps ? calculatedMipLevels : 1,
            TextureSampleCount = TextureSampleCount.OneSample
        };

        var texture = CreateTexture(textureCreateDescriptor);
        var textureUpdateDescriptor = new TextureUpdateDescriptor
        {
            Offset = Int3.Zero,
            Size = new Int3(image.Width, image.Height, 1),
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
}
