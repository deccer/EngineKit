using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using ImGuiNET;
using Microsoft.Extensions.Options;
using OpenTK.Mathematics;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Swapchain;

public struct GpuGlobalUniforms
{
    public Matrix4 ViewProjection;
    public Matrix4 InverseViewProjection;
    public Matrix4 Projection;
    public Vector4 CameraPosition;
}

internal sealed class SwapchainApplication : Application
{
    private const int ShadowmapSize = 1024;

    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly IGraphicsContext _graphicsContext;
    private readonly IUIRenderer _uiRenderer;
    private readonly IImageLoader _imageLoader;

    private ITexture? _blueNoiseTexture;
    private SwapchainRenderDescriptor _swapchainRenderDescriptor;

    private IGraphicsPipeline _sceneGraphicsPipeline;

    private GpuConstants _gpuConstants;
    private IList<GpuObject> _gpuObjects;

    private IVertexBuffer _sceneVertexBuffer;
    private IIndexBuffer _sceneIndexBuffer;

    private ISampler _nearestSampler;
    private ISampler _rsmSampler;

    public SwapchainApplication(
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

        _gpuConstants = new GpuConstants();
        _gpuObjects = new List<GpuObject>();
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

        if (!LoadResources())
        {
            return false;
        }

        if (!LoadRenderDescriptors())
        {
            return false;
        }

        if (!LoadPipelines())
        {
            return false;
        }

        return true;
    }

    protected override void Render()
    {
        _graphicsContext.BindGraphicsPipeline(_sceneGraphicsPipeline);
        _graphicsContext.BeginRenderToSwapchain(_swapchainRenderDescriptor);
        _graphicsContext.EndRender();

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
        _nearestSampler?.Dispose();
        _blueNoiseTexture?.Dispose();

        _sceneGraphicsPipeline.Dispose();

        _graphicsContext.Dispose();
        base.Unload();
    }

    protected override void Update()
    {
        _uiRenderer.Update(1.0f / 60.0f);
        if (IsKeyPressed(Glfw.Key.KeyEscape))
        {
            Close();
        }

        if (IsMousePressed(Glfw.MouseButton.ButtonLeft))
        {
            _logger.Debug("Left Mouse Button Pressed");
        }
    }

    private bool LoadRenderDescriptors()
    {
        //TODO(deccer) hide SwapchainDescriptor in Application/also make sure to resize when window resize
        _swapchainRenderDescriptor = new SwapchainRenderDescriptorBuilder()
            .WithViewport(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y)
            .Build();

        return true;
    }

    private bool LoadPipelines()
    {
        var graphicsPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/Scene.vs.glsl", "Shaders/Scene.fs.glsl")
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 3, 0)
                .AddAttribute(0, DataType.Float, 3, 12)
                .AddAttribute(0, DataType.Float, 2, 24)
                .Build("PositionNormalUv"))
            .EnableDepthTest()
            .EnableDepthWrite()
            .Build("ScenePipeline");

        if (graphicsPipelineResult.IsFailure)
        {
            _logger.Error("App: Unable to build graphics pipeline {PipelineName}. {Details}",
                graphicsPipelineResult.Error, "ScenePipeline");
            return false;
        }

        _sceneGraphicsPipeline = graphicsPipelineResult.Value;

        return true;
    }

    private bool LoadResources()
    {
        _blueNoiseTexture = LoadTexture("Data/Default/T_BlueNoise32.png");
        if (_blueNoiseTexture == null)
        {
            return false;
        }

        return true;
    }

    private ITexture? LoadTexture(string filePath)
    {
        if (_imageLoader.LoadImage<Rgba32>("Data/Default/T_BlueNoise32.png") is not Image<Rgba32> image)
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
            Size = new Vector3i(image.Width, image.Height, 1)
        };
        var texture = _graphicsContext.CreateTexture(textureCreateDescriptor);
        var textureUpdateDescriptor = new TextureUpdateDescriptor
        {
            Level = 0,
            Offset = Vector3i.Zero,
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

    private static VertexInputDescriptor CreatePositionNormalUvVertexInput()
    {
        return new VertexInputDescriptorBuilder()
            .AddAttribute(0, DataType.Float, 3, 0)
            .AddAttribute(0, DataType.Float, 3, 12)
            .AddAttribute(0, DataType.Float, 2, 24)
            .Build("PositionNormalUv");
    }
}