using System;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Native.Glfw;
using ImGuiNET;
using Microsoft.Extensions.Options;
using EngineKit.Mathematics;
using Serilog;
using SixLabors.ImageSharp.PixelFormats;

namespace ComputeConvolution;

internal sealed class ComputeConvolutionApplication : GraphicsApplication
{
    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly IMetrics _metrics;

    private ITexture? _skyboxTexture;
    private ITexture? _skyboxConvolvedTexture;
    private ISampler? _skyboxSampler;
    private SwapchainDescriptor _swapchainDescriptor;

    private IGraphicsPipeline? _sceneGraphicsPipeline;

    public ComputeConvolutionApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        ICapabilities capabilities,
        IMetrics metrics,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IUIRenderer uiRenderer)
        : base(
            logger,
            windowSettings,
            contextSettings,
            applicationContext,
            capabilities,
            metrics,
            inputProvider,
            graphicsContext,
            uiRenderer)
    {
        _logger = logger;
        _applicationContext = applicationContext;
        _metrics = metrics;
    }
    
    protected override bool Initialize()
    {
        if (!base.Initialize())
        {
            return false;
        }
        
        SetWindowIcon("enginekit-icon.png");

        return true;
    }

    protected override bool Load()
    {
        if (!base.Load())
        {
            _logger.Error("{Category}: Unable to load", "App");
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

    protected override void Render(float deltaTime, float elapsedMilliseconds)
    {
        if (_metrics.FrameCounter == 0)
        {
            if (!ConvolveSkybox())
            {
                return;
            }
        }
        GraphicsContext.BeginRenderPass(_swapchainDescriptor);
        GraphicsContext.BindGraphicsPipeline(_sceneGraphicsPipeline!);

        _sceneGraphicsPipeline!.BindSampledTexture(_skyboxSampler!, _skyboxTexture!.Id, 0);

        _sceneGraphicsPipeline.DrawArrays(3, 0);
        GraphicsContext.EndRenderPass();

        RenderUi();
    }

    protected override void Unload()
    {
        _skyboxSampler?.Dispose();
        _skyboxTexture?.Dispose();
        _skyboxConvolvedTexture?.Dispose();
        _sceneGraphicsPipeline?.Dispose();

        base.Unload();
    }

    protected override void Update(float deltaTime, float elapsedMilliseconds)
    {
        base.Update(deltaTime, elapsedMilliseconds);
        if (IsKeyPressed(Glfw.Key.KeyEscape))
        {
            Close();
        }
    }

    private void RenderUi()
    {
        UIRenderer.BeginLayout();
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
        UIRenderer.ShowDemoWindow();
        UIRenderer.EndLayout();
    }

    private bool LoadRenderDescriptors()
    {
        //TODO(deccer) hide SwapchainDescriptor in Application/also make sure to resize when window resize
        _swapchainDescriptor = new SwapchainDescriptorBuilder()
            .ClearColor(Colors.DimGray)
            .ClearDepth()
            .WithViewport(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y)
            .Build("Swapchain");

        return true;
    }

    private bool LoadPipelines()
    {
        var graphicsPipelineResult = GraphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/FST.vs.glsl", "Shaders/Texture.fs.glsl")
            .WithVertexAttributesFromDescriptor(new VertexInputDescriptorBuilder()
                .AddAttribute(0, Format.R32G32B32Float, 0)
                .AddAttribute(0, Format.R32G32Float, 12)
                .Build(nameof(VertexPositionUv)))
            .WithTopology(PrimitiveTopology.Triangles)
            .WithFaceWinding(FaceWinding.Clockwise)
            .WithCullingEnabled(CullMode.Back)
            .WithDepthTestEnabled(CompareFunction.Less)
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
        if (!LoadSkybox("Miramar"))
        {
            return false;
        }

        var skyboxTextureCreateDescriptor = new TextureCreateDescriptor
        {
            ImageType = ImageType.TextureCube,
            Format = Format.R16G16B16A16Float,
            Label = "ConvolvedSkybox",
            Size = new Int3(_skyboxTexture!.TextureCreateDescriptor.Size.X, _skyboxTexture.TextureCreateDescriptor.Size.Y, 1),
            MipLevels = (uint)(1 + MathF.Floor(MathF.Log2(MathF.Min(_skyboxTexture.TextureCreateDescriptor.Size.X, _skyboxTexture.TextureCreateDescriptor.Size.Y)))),
            TextureSampleCount = TextureSampleCount.OneSample
        };
        _skyboxConvolvedTexture = GraphicsContext.CreateTexture(skyboxTextureCreateDescriptor);

        _skyboxSampler = GraphicsContext.CreateSamplerBuilder()
            .WithInterpolationFilter(TextureInterpolationFilter.Linear)
            .WithMipmapFilter(TextureMipmapFilter.LinearMipmapNearest)
            .WithAddressMode(TextureAddressMode.Repeat)
            .Build("LinearMipmapNearest");

        return true;
    }

    private bool ConvolveSkybox()
    {
        var convolutionComputePipelineResult = GraphicsContext.CreateComputePipelineBuilder()
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
        GraphicsContext.BindComputePipeline(convolutionComputePipeline);

        convolutionComputePipeline.BindImage(
            _skyboxTexture!,
            0,
            0,
            MemoryAccess.ReadOnly,
            _skyboxTexture!.TextureCreateDescriptor.Format);
        convolutionComputePipeline.BindImage(
            _skyboxConvolvedTexture!,
            1,
            0,
            MemoryAccess.WriteOnly,
            _skyboxConvolvedTexture!.TextureCreateDescriptor.Format);
        convolutionComputePipeline.Dispatch(xGroups, yGroups, 6);
        GraphicsContext.InsertMemoryBarrier(BarrierMask.ShaderImageAccess);

        _skyboxConvolvedTexture!.GenerateMipmaps();

        return true;
    }

    private bool LoadSkybox(string skyboxName)
    {
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
            using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(skyboxFileName);

            if (slice == 0)
            {
                var skyboxTextureCreateDescriptor = new TextureCreateDescriptor
                {
                    ImageType = ImageType.TextureCube,
                    Format = Format.R8G8B8A8UNorm,
                    Label = $"Skybox_{skyboxName}",
                    Size = new Int3(image.Width, image.Height, 1),
                    MipLevels = 1,
                    TextureSampleCount = TextureSampleCount.OneSample
                };
                _skyboxTexture = GraphicsContext.CreateTexture(skyboxTextureCreateDescriptor);
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
                _skyboxTexture!.Update(skyboxTextureUpdateDescriptor, imageMemory.Pin());
            }
        }

        return true;
    }
}