using System;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Native.Glfw;
using ImGuiNET;
using Microsoft.Extensions.Options;
using OpenTK.Mathematics;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ComputeConvolution;

internal sealed class ComputeConvolutionApplication : Application
{
    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly IMetrics _metrics;
    private readonly IGraphicsContext _graphicsContext;
    private readonly IUIRenderer _uiRenderer;

    private ITexture? _skyboxTexture;
    private ITexture? _skyboxConvolvedTexture;
    private ISampler? _skyboxSampler;
    private SwapchainRenderDescriptor _swapchainRenderDescriptor;

    private IGraphicsPipeline? _sceneGraphicsPipeline;

    public ComputeConvolutionApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        IMetrics metrics,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IUIRenderer uiRenderer)
        : base(logger, windowSettings, contextSettings, applicationContext, metrics, inputProvider)
    {
        _logger = logger;
        _applicationContext = applicationContext;
        _metrics = metrics;
        _graphicsContext = graphicsContext;
        _uiRenderer = uiRenderer;
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

        if (!LoadMeshes())
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
        if (_metrics.FrameCounter == 0)
        {
            if (!ConvolveSkybox())
            {
                return;
            }
        }
        _graphicsContext.BeginRenderToSwapchain(_swapchainRenderDescriptor);
        _graphicsContext.BindGraphicsPipeline(_sceneGraphicsPipeline!);

        _sceneGraphicsPipeline!.BindSampledTexture(_skyboxSampler!, _skyboxTexture.Id!, 0);

        _sceneGraphicsPipeline.DrawArrays(3, 0);
        _graphicsContext.EndRender();

        RenderUi();
    }

    protected override void FramebufferResized()
    {
        base.FramebufferResized();
        _uiRenderer.WindowResized(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y);
    }

    protected override void Unload()
    {
        _skyboxSampler?.Dispose();
        _skyboxTexture?.Dispose();
        _skyboxConvolvedTexture?.Dispose();
        _sceneGraphicsPipeline?.Dispose();

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
    }

    private void RenderUi()
    {
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

    private bool LoadRenderDescriptors()
    {
        //TODO(deccer) hide SwapchainDescriptor in Application/also make sure to resize when window resize
        _swapchainRenderDescriptor = new SwapchainRenderDescriptorBuilder()
            .ClearColor(Color4.DimGray)
            .ClearDepth()
            .WithViewport(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y)
            .Build();

        return true;
    }

    private bool LoadPipelines()
    {
        var graphicsPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/FST.vs.glsl", "Shaders/Texture.fs.glsl")
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 3, 0)
                .AddAttribute(0, DataType.Float, 2, 12)
                .Build(nameof(VertexPositionUv)))
            .WithTopology(PrimitiveTopology.Triangles)
            .WithFaceWinding(FaceWinding.Clockwise)
            .EnableCulling(CullMode.Back)
            .EnableDepthTest()
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

    private bool LoadMeshes()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        return true;
    }

    private bool LoadResources()
    {
        if (!LoadSkybox("Miramar"))
        {
            return false;
        }

        var skyboxTextureCreateDescriptor = new TextureCreateDescriptor
        {
            ImageType = ImageType.TextureCube,
            Format = Format.R16G16B16A16Float,
            Label = "ConvolvedSkybox",
            Size = new Vector3i(1024, 1024, 1),
            MipLevels = (uint)(1 + MathF.Ceiling(MathF.Log2(1024))),
            SampleCount = SampleCount.OneSample
        };
        _skyboxConvolvedTexture =
            _graphicsContext.CreateTexture(skyboxTextureCreateDescriptor);

        _skyboxSampler = _graphicsContext.CreateSamplerBuilder()
            .WithMagnificationFilter()
            .WithMinificationFilter(Filter.Linear, Filter.Nearest)
            .WithAddressMode(AddressMode.Repeat)
            .Build("LinearMipmapNearest");

        return true;
    }

    private bool ConvolveSkybox()
    {
        var convolutionComputePipelineResult = _graphicsContext.CreateComputePipelineBuilder()
            .WithShaderFromFile("Shaders/ConvolveSkybox.cs.glsl")
            .Build("ComputeConvolution");
        if (convolutionComputePipelineResult.IsFailure)
        {
            _logger.Error("{Category}: Unable to compile compute pipeline. {Details}", "App",
                convolutionComputePipelineResult.Error);
            return false;
        }

        const int xSize = 16;
        const int ySize = 16;
        const int xGroups = (1024 + xSize - 1) / xSize;
        const int yGroups = (1024 + ySize - 1) / ySize;

        using var convolutionComputePipeline = convolutionComputePipelineResult.Value;
        _graphicsContext.BindComputePipeline(convolutionComputePipeline);

        convolutionComputePipeline.BindImage(
            _skyboxTexture,
            0,
            0,
            MemoryAccess.ReadOnly,
            _skyboxTexture!.Format);
        convolutionComputePipeline.BindImage(
            _skyboxConvolvedTexture,
            1,
            0,
            MemoryAccess.WriteOnly,
            _skyboxConvolvedTexture.Format);
        convolutionComputePipeline.Dispatch(xGroups, yGroups, 6);
        _graphicsContext.InsertMemoryBarrier(BarrierMask.ShaderImageAccess);

        _skyboxConvolvedTexture!.GenerateMipmaps();

        return true;
    }

    private bool LoadSkybox(string skyboxName)
    {
        var skyboxTextureCreateDescriptor = new TextureCreateDescriptor
        {
            ImageType = ImageType.TextureCube,
            Format = Format.R8G8B8A8UNorm,
            Label = $"Skybox_{skyboxName}",
            Size = new Vector3i(1024, 1024, 1),
            MipLevels = 1,
            SampleCount = SampleCount.OneSample
        };
        _skyboxTexture = _graphicsContext.CreateTexture(skyboxTextureCreateDescriptor);

        var skyboxFileNames = new[]
        {
            $"Data/Sky/TC_{skyboxName}_Xp.png",
            $"Data/Sky/TC_{skyboxName}_Xn.png",
            $"Data/Sky/TC_{skyboxName}_Yp.png",
            $"Data/Sky/TC_{skyboxName}_Yn.png",
            $"Data/Sky/TC_{skyboxName}_Zp.png",
            $"Data/Sky/TC_{skyboxName}_Zn.png"
        };
        var slice = 0;
        foreach (var skyboxFileName in skyboxFileNames)
        {
            using var image = Image.Load<Rgba32>(skyboxFileName);

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
                _skyboxTexture.Update(skyboxTextureUpdateDescriptor, imageMemory.Pin());
            }
        }

        return true;
    }
}