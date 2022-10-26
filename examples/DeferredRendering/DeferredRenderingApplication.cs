using System.Runtime.InteropServices;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Mathematics;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using ImGuiNET;
using Microsoft.Extensions.Options;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = EngineKit.Mathematics.Color;
using Point = EngineKit.Mathematics.Point;

namespace HelloWindow;

public struct GpuGlobalUniforms
{
    public Matrix ViewProjection;
    public Matrix InverseViewProjection;
    public Matrix Projection;
    public Matrix CameraPosition;
}

public struct GpuShadingUniforms
{
    public Matrix SunViewProjection;
    public Vector4 SunDirection;
    public Vector4 SunStrength;
}

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct GpuRsmUniforms
{
    public Matrix SunViewProjection;
    public Matrix SunInverseViewProjection;
    public Point TargetDimension;
    public float RMax;
    public uint CurrentPass;
    public uint Samples;
}

internal sealed class DeferredRenderingApplication : Application
{
    private const int ShadowmapSize = 1024;

    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly IGraphicsContext _graphicsContext;
    private readonly IUIRenderer _uiRenderer;
    private readonly IImageLoader _imageLoader;

    private ITexture? _blueNoiseTexture;
    private SwapchainRenderDescriptor _swapchainRenderDescriptor;
    private ITexture _gBufferColorAttachment;
    private ITexture _gBufferNormalAttachment;
    private ITexture _gBufferDepthAttachment;
    private FramebufferRenderDescriptor _gBufferFramebufferRenderDescriptor;

    private ITexture _rsmFluxAttachment;
    private ITexture _rsmNormalAttachment;
    private ITexture _rsmDepthAttachment;
    private FramebufferRenderDescriptor _rsmFramebufferRenderDescriptor;

    private ITexture _indirectLightingTexture1;
    private ITexture _indirectLightingTexture2;

    private IGraphicsPipeline _sceneGraphicsPipeline;
    private IGraphicsPipeline _rsmGraphicsPipeline;
    private IGraphicsPipeline _shadingGraphicsPipeline;
    private IComputePipeline? _rsmIndirectComputePipeline;
    private IComputePipeline? _rsmIndirectDitheredFilteredComputePipeline;
    private IGraphicsPipeline _debugTextureGraphicsPipeline;

    private IVertexBuffer _sceneVertexBuffer;
    private IIndexBuffer _sceneIndexBuffer;

    private Sampler _nearestSampler;
    private Sampler _rsmSampler;

    public DeferredRenderingApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        IMetrics metrics,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IUIRenderer uiRenderer,
        IImageLoader imageLoader)
        : base(logger, windowSettings, contextSettings, applicationContext, metrics, inputProvider)
    {
        _logger = logger;
        _applicationContext = applicationContext;
        _graphicsContext = graphicsContext;
        _uiRenderer = uiRenderer;
        _imageLoader = imageLoader;
    }

    protected override bool Load()
    {
        if (!base.Load())
        {
            _logger.Error("{Category}: Unable to load", "App");
            return false;
        }

        if (!_uiRenderer.Load(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y))
        {
            return false;
        }

        _blueNoiseTexture = LoadTexture("Data/Default/T_BlueNoise32.png");
        if (_blueNoiseTexture == null)
        {
            return false;
        }

        _swapchainRenderDescriptor = new SwapchainRenderDescriptorBuilder()
            .WithViewport(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y)
            .Build();

        _gBufferColorAttachment = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y,
            Format.R8G8B8A8UNorm, "GBuffer-Color");
        ;
        _gBufferNormalAttachment = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.R16G16B16SNorm, "GBuffer-Normals");
        _gBufferDepthAttachment = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.D32UNorm, "GBuffer-Depth");

        _gBufferFramebufferRenderDescriptor = new FramebufferRenderDescriptorBuilder()
            .WithColorAttachment(_gBufferColorAttachment, true, new Vector4(0.1f, 0.3f, 0.5f, 0.0f))
            .WithColorAttachment(_gBufferNormalAttachment, false, Vector4.Zero)
            .WithDepthAttachment(_gBufferDepthAttachment, true, 1.0f)
            .Build("GBuffer");

        _rsmFluxAttachment = _graphicsContext.CreateTexture2D(ShadowmapSize, ShadowmapSize, Format.R11G11B10Float, "RSM-Flux");
        _rsmNormalAttachment =
            _graphicsContext.CreateTexture2D(ShadowmapSize, ShadowmapSize, Format.R16G16B16SNorm, "RSM-Normals");
        _rsmDepthAttachment =
            _graphicsContext.CreateTexture2D(ShadowmapSize, ShadowmapSize, Format.D16UNorm, "RSM-Depth");
        _rsmFramebufferRenderDescriptor = new FramebufferRenderDescriptorBuilder()
            .WithColorAttachment(_rsmFluxAttachment, false, Vector4.Zero)
            .WithColorAttachment(_rsmNormalAttachment, false, Vector4.Zero)
            .WithDepthAttachment(_rsmDepthAttachment, true, 1.0f)
            .Build("RSM");

        _indirectLightingTexture1 = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.R16G16B16A16Float);
        _indirectLightingTexture2 = _graphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.R16G16B16A16Float);

        var nearestSamplerDescriptor = new SamplerDescriptor
        {
            MagFilter = Filter.Nearest,
            MinFilter = Filter.Nearest,
            AddressModeU = AddressMode.Repeat,
            AddressModeV = AddressMode.Repeat
        };
        _nearestSampler = Sampler.Create(nearestSamplerDescriptor);

        var rsmSamplerDescriptor = new SamplerDescriptor
        {
            MagFilter = Filter.Linear,
            MinFilter = Filter.Linear,
            AddressModeU = AddressMode.Repeat,
            AddressModeV = AddressMode.Repeat,
            CompareOperation = CompareOperation.Less,
            IsCompareEnabled = true
        };
        _rsmSampler = Sampler.Create(rsmSamplerDescriptor);

        _sceneGraphicsPipeline = CreateSceneGraphicsPipeline();
        _rsmGraphicsPipeline = CreateSceneShadowGraphicsPipeline();
        _shadingGraphicsPipeline = CreateShadingGraphicsPipeline();
        _debugTextureGraphicsPipeline = CreateDebugTextureGraphicsPipeline();
        _rsmIndirectComputePipeline = CreateRsmIndirectComputePipeline();
        _rsmIndirectDitheredFilteredComputePipeline = CreateRsmIndirectDitheredFilteredComputePipeline();

        GL.ClearColor(0.3f, 0.2f, 0.4f, 1.0f);

        return true;
    }

    protected override void Render()
    {
        GL.Clear(GL.ClearBufferMask.ColorBufferBit | GL.ClearBufferMask.DepthBufferBit);

        _uiRenderer.BeginLayout();
        ImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode);
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Quit"))
                    {
                        Close();
                    }
                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            ImGui.EndMainMenuBar();
        }
        _uiRenderer.ShowDemoWindow();
        _uiRenderer.EndLayout();
    }

    protected override void FramebufferResized()
    {
        base.FramebufferResized();
        _uiRenderer.WindowResized(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y);
    }

    protected override void Unload()
    {
        _nearestSampler.Dispose();
        _rsmSampler.Dispose();
        _gBufferColorAttachment.Dispose();
        _gBufferNormalAttachment.Dispose();
        _gBufferDepthAttachment.Dispose();
        _rsmFluxAttachment.Dispose();
        _rsmNormalAttachment.Dispose();
        _rsmDepthAttachment.Dispose();
        _indirectLightingTexture1.Dispose();
        _indirectLightingTexture2.Dispose();
        _blueNoiseTexture?.Dispose();

        _sceneGraphicsPipeline.Dispose();
        _rsmGraphicsPipeline.Dispose();
        _shadingGraphicsPipeline.Dispose();
        _rsmIndirectComputePipeline?.Dispose();
        _rsmIndirectDitheredFilteredComputePipeline?.Dispose();
        _debugTextureGraphicsPipeline.Dispose();

        _graphicsContext.Dispose();
        base.Unload();
    }

    protected override void Update()
    {
        _uiRenderer?.Update(1.0f / 60.0f);
        if (IsKeyPressed(Glfw.Key.KeyEscape))
        {
            Close();
        }

        if (IsMousePressed(Glfw.MouseButton.ButtonLeft))
        {
            _logger.Debug("Left Mouse Button Pressed");
        }
    }

    private ITexture? LoadTexture(string filePath)
    {
        var image = _imageLoader.LoadImage<Rgba32>("Data/Default/T_BlueNoise32.png") as Image<Rgba32>;
        if (image == null)
        {
            return null;
        }

        var textureCreateDescriptor = new TextureCreateDescriptor
        {
            Format = Format.R8G8B8A8UNorm,
            Label = "T_BlueNoise",
            ArrayLayers = 0,
            ImageType = ImageType.Texture2D,
            MipLevels = 1,
            SampleCount = SampleCount.OneSample,
            Size = new Int3(image.Width, image.Height, 1)
        };
        var texture = _graphicsContext.CreateTexture(textureCreateDescriptor);
        var textureUpdateDescriptor = new TextureUpdateDescriptor
        {
            Level = 0,
            Offset = Int3.Zero,
            Size = textureCreateDescriptor.Size,
            UploadDimension = UploadDimension.Two,
            UploadFormat = UploadFormat.RedGreenBlueAlpha,
            UploadType = UploadType.UnsignedByte
        };

        if (image.DangerousTryGetSinglePixelMemory(out var pixelData))
        {
            texture.Update(textureUpdateDescriptor, pixelData.Pin());
            image.Dispose();
        }

        return texture;
    }

    private IGraphicsPipeline CreateSceneGraphicsPipeline()
    {
        var graphicsPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithShaders("Shaders/SceneDeferredPbr.vs.glsl", "Shaders/SceneDeferredPbr.fs.glsl")
            .WithVertexInput(CreateSceneVertexInputBindingDescriptor())
            .EnableDepthTest()
            .EnableDepthWrite()
            .Build("ScenePipeline");

        if (graphicsPipelineResult.IsFailure)
        {
            _logger.Error("App: Unable to build graphics pipeline. {Details}", graphicsPipelineResult.Error);
            Close();
        }

        return graphicsPipelineResult.Value;
    }

    private IGraphicsPipeline CreateSceneShadowGraphicsPipeline()
    {
        var graphicsPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithShaders("Shaders/SceneDeferredPbr.vs.glsl", "Shaders/RsmScenePbr.fs.glsl")
            .WithVertexInput(CreateSceneVertexInputBindingDescriptor())
            .EnableDepthBias(3.0f, 5.0f)
            .EnableDepthTest()
            .EnableDepthWrite()
            .Build("ShadowPipeline");

        if (graphicsPipelineResult.IsFailure)
        {
            _logger.Error("App: Unable to build graphics pipeline. {Details}", graphicsPipelineResult.Error);
            Close();
        }

        return graphicsPipelineResult.Value;
    }

    private IGraphicsPipeline CreateShadingGraphicsPipeline()
    {
        var graphicsPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithShaders("Shaders/FST.vs.glsl", "Shaders/ShadeDeferredPbr.fs.glsl")
            .DisableCulling()
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 3, 0)
                .AddAttribute(0, DataType.Float, 2, 12)
                .Build("Shading"))
            .Build("ShadingPipeline");

        if (graphicsPipelineResult.IsFailure)
        {
            _logger.Error("App: Unable to build graphics pipeline. {Details}", graphicsPipelineResult.Error);
            Close();
        }

        return graphicsPipelineResult.Value;
    }
    private IGraphicsPipeline CreateDebugTextureGraphicsPipeline()
    {
        var graphicsPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithShaders("Shaders/FST.vs.glsl", "Shaders/Texture.fs.glsl")
            .DisableCulling()
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 3, 0)
                .AddAttribute(0, DataType.Float, 2, 12)
                .Build("PositionNormalUv"))
            .Build("DebugTexturePipeline");

        if (graphicsPipelineResult.IsFailure)
        {
            _logger.Error("App: Unable to build graphics pipeline. {Details}", graphicsPipelineResult.Error);
            Close();
        }

        return graphicsPipelineResult.Value;
    }

    private IComputePipeline CreateRsmIndirectComputePipeline()
    {
        var computePipelineResult = _graphicsContext.CreateComputePipelineBuilder()
            .WithShader("Shaders/RsmIndirect.cs.glsl")
            .Build("RsmIndirectPipeline");

        if (computePipelineResult.IsFailure)
        {
            _logger.Error("App: Unable to build compute pipeline. {Details}", computePipelineResult.Error);
            Close();
        }

        return computePipelineResult.Value;
    }

    private IComputePipeline CreateRsmIndirectDitheredFilteredComputePipeline()
    {
        var computePipelineResult = _graphicsContext.CreateComputePipelineBuilder()
            .WithShader("Shaders/RsmIndirectDitheredFiltered.cs.glsl")
            .Build("RsmIndirectDitheredFilteredPipeline");

        if (computePipelineResult.IsFailure)
        {
            _logger.Error("App: Unable to build compute pipeline. {Details}", computePipelineResult.Error);
            Close();
        }

        return computePipelineResult.Value;
    }

    private static VertexInputDescriptor CreateSceneVertexInputBindingDescriptor()
    {
        return new VertexInputDescriptorBuilder()
            .AddAttribute(0, DataType.Float, 3, 0)
            .AddAttribute(0, DataType.Short, 2, 12)
            .AddAttribute(0, DataType.Float, 2, 20)
            .Build("Scene");
    }
}