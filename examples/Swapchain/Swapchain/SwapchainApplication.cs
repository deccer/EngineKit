using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace Swapchain;

internal sealed class SwapchainApplication : GraphicsApplication
{
    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly IMetrics _metrics;
    private readonly IImageLoader _imageLoader;
    private readonly IMeshLoader _meshLoader;
    private readonly ICamera _camera;

    private ITexture? _skullBaseColorTexture;
    private SwapchainRenderDescriptor _swapchainRenderDescriptor;

    private IGraphicsPipeline? _sceneGraphicsPipeline;

    private GpuConstants _gpuConstants;
    private IUniformBuffer? _gpuConstantsBuffer;
    private readonly IList<GpuObject> _gpuObjects;
    private IShaderStorageBuffer? _gpuObjectsBuffer;
    private readonly IList<GpuMaterial> _gpuMaterials;
    private IShaderStorageBuffer? _gpuMaterialBuffer;

    private IVertexBuffer? _skullVertexBuffer;
    private IIndexBuffer? _skullIndexBuffer;

    private ISampler? _linearMipmapNearestSampler;
    private ISampler? _linearMipmapLinear;

    public SwapchainApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        IMetrics metrics,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IUIRenderer uiRenderer,
        IImageLoader imageLoader,
        IMeshLoader meshLoader,
        ICamera camera)
        : base(logger, windowSettings, contextSettings, applicationContext, metrics, inputProvider, graphicsContext, uiRenderer)
    {
        _logger = logger;
        _applicationContext = applicationContext;
        _metrics = metrics;
        _imageLoader = imageLoader;
        _meshLoader = meshLoader;
        _camera = camera;

        _gpuConstants = new GpuConstants();
        _gpuObjects = new List<GpuObject>();
        _gpuMaterials = new List<GpuMaterial>();
        _camera.Sensitivity = 0.25f;
    }

    protected override bool Load()
    {
        if (!base.Load())
        {
            _logger.Error("{Category}: Unable to load", "App");
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

        _gpuConstantsBuffer = GraphicsContext.CreateUniformBuffer("Camera", _gpuConstants);

        _gpuObjects.Add(new GpuObject
        {
            World = Matrix4.CreateTranslation(-5, 0, 0)
        });
        _gpuObjects.Add(new GpuObject
        {
            World = Matrix4.CreateTranslation(0, 0, 0)
        });
        _gpuObjects.Add(new GpuObject
        {
            World = Matrix4.CreateTranslation(+5, 0, 0)
        });

        _gpuObjectsBuffer = GraphicsContext.CreateShaderStorageBuffer("Objects", _gpuObjects.ToArray());

        _gpuMaterials.Add(new GpuMaterial
        {
            FlagsInt = new Vector4i(0, 0, 0, 0),
            FlagsFloat = new Vector4(0.5f, 0.0f, 0.0f, 0.0f),
            BaseColor = new Vector4(0.1f, 0.2f, 0.3f, 1.0f),
        });
        _gpuMaterials.Add(new GpuMaterial
        {
            FlagsInt = new Vector4i(0, 0, 0, 0),
            FlagsFloat = new Vector4(0.5f, 0.0f, 0.0f, 0.0f),
            BaseColor = new Vector4(0.2f, 0.3f, 0.4f, 1.0f),
        });
        _gpuMaterials.Add(new GpuMaterial
        {
            FlagsInt = new Vector4i(0, 0, 0, 0),
            FlagsFloat = new Vector4(0.5f, 0.0f, 0.0f, 0.0f),
            BaseColor = new Vector4(0.3f, 0.4f, 0.5f, 1.0f),
        });

        _gpuMaterialBuffer = GraphicsContext
            .CreateShaderStorageBuffer<GpuMaterial>("Materials", _gpuMaterials.ToArray());

        return true;
    }

    protected override void Render()
    {
        _gpuConstants.ViewProjection = _camera.ViewMatrix * _camera.ProjectionMatrix;
        _gpuConstantsBuffer!.Update(_gpuConstants, 0);

        GraphicsContext.BeginRenderToSwapchain(_swapchainRenderDescriptor);
        GraphicsContext.BindGraphicsPipeline(_sceneGraphicsPipeline!);
        _sceneGraphicsPipeline!.BindVertexBuffer(_skullVertexBuffer!, 0, 0);
        _sceneGraphicsPipeline.BindIndexBuffer(_skullIndexBuffer!);

        _sceneGraphicsPipeline.BindUniformBuffer(_gpuConstantsBuffer, 0);
        _sceneGraphicsPipeline.BindShaderStorageBuffer(_gpuObjectsBuffer!, 1);
        _sceneGraphicsPipeline.BindShaderStorageBuffer(_gpuMaterialBuffer!, 2);
        _sceneGraphicsPipeline.BindSampledTexture(_linearMipmapLinear!, _skullBaseColorTexture!, 0);

        _sceneGraphicsPipeline.DrawElementsInstanced(_skullIndexBuffer!.Count, 0, _gpuObjects.Count);
        GraphicsContext.EndRender();

        RenderUi();
    }

    protected override void Unload()
    {
        _linearMipmapNearestSampler?.Dispose();
        _linearMipmapLinear?.Dispose();
        _skullBaseColorTexture?.Dispose();
        _skullVertexBuffer?.Dispose();
        _skullIndexBuffer?.Dispose();
        _sceneGraphicsPipeline?.Dispose();
        _gpuConstantsBuffer?.Dispose();
        _gpuMaterialBuffer?.Dispose();
        _gpuObjectsBuffer?.Dispose();

        base.Unload();
    }

    protected override void Update()
    {
        base.Update();

        if (IsMousePressed(Glfw.MouseButton.ButtonRight))
        {
            _camera.ProcessMouseMovement();
        }
        _camera.ProcessKeyboard(Vector3.Zero, _metrics.DeltaTime);

        if (IsKeyPressed(Glfw.Key.KeyEscape))
        {
            Close();
        }

        if (IsMousePressed(Glfw.MouseButton.ButtonLeft))
        {
            _logger.Debug("Left Mouse Button Pressed");
        }

        if (IsKeyPressed(Glfw.Key.KeyF5))
        {
            HideCursor();
        }

        if (IsKeyPressed(Glfw.Key.KeyF6))
        {
            ShowCursor();
        }
    }

    private void RenderUi()
    {
        UIRenderer.BeginLayout();
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

                if (ImGui.BeginMenu($"{_metrics.DeltaTime:F2}"))
                {
                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
                ImGui.EndMainMenuBar();
            }

            if (ImGui.Begin("Debug"))
            {
                var sensitivity = _camera.Sensitivity;
                ImGui.SliderFloat("Camera Sensitivity", ref sensitivity, 0.01f, 2.0f);
                _camera.Sensitivity = sensitivity;

                ImGui.End();
            }
        }
        UIRenderer.ShowDemoWindow();
        UIRenderer.EndLayout();
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
        var graphicsPipelineResult = GraphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/Scene.vs.glsl", "Shaders/Scene.fs.glsl")
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 3, 0)
                .AddAttribute(0, DataType.Float, 3, 12)
                .AddAttribute(0, DataType.Float, 2, 24)
                .AddAttribute(0, DataType.Float, 4, 32)
                .Build(nameof(VertexPositionNormalUvTangent)))
            .WithTopology(PrimitiveTopology.Triangles)
            .WithFaceWinding(FaceWinding.CounterClockwise)
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
        var skullMeshDates = _meshLoader.LoadModel(Path.Combine(baseDirectory, "Data/Props/Skull/SM_Skull_Optimized_point2.gltf")).ToArray();
        _skullVertexBuffer = GraphicsContext.CreateVertexBuffer("SkullVertices", skullMeshDates, VertexType.PositionNormalUvTangent);
        _skullIndexBuffer = GraphicsContext.CreateIndexBuffer("SkullIndices", skullMeshDates);

        //var pbrScene = _meshLoader.LoadModel(Path.Combine(baseDirectory, "Data/Scenes/Pbr_Reference/scene.gltf")).ToArray();

        return true;
    }

    private bool LoadResources()
    {
        _skullBaseColorTexture = LoadTexture("Data/Props/Skull/TD_Checker_Base_Color.png");
        if (_skullBaseColorTexture == null)
        {
            return false;
        }

        _linearMipmapNearestSampler = GraphicsContext.CreateSamplerBuilder()
            .WithMagnificationFilter()
            .WithMinificationFilter(Filter.Linear, Filter.Nearest)
            .WithAddressMode(AddressMode.Repeat)
            .Build("LinearMipmapNearest");

        _linearMipmapLinear = GraphicsContext.CreateSamplerBuilder()
            .Build("LinearMipmapLinear");

        return true;
    }

    private ITexture? LoadTexture(string filePath)
    {
        if (_imageLoader.LoadImage<Rgba32>(filePath) is not Image<Rgba32> image)
        {
            return null;
        }

        var textureCreateDescriptor = new TextureCreateDescriptor
        {
            Format = Format.R8G8B8A8UNorm,
            Label = Path.GetFileNameWithoutExtension(filePath),
            ArrayLayers = 0,
            ImageType = ImageType.Texture2D,
            MipLevels = 1 + (uint)MathF.Ceiling(MathF.Log2(MathF.Max(image.Width, image.Height))),
            SampleCount = SampleCount.OneSample,
            Size = new Vector3i(image.Width, image.Height, 1)
        };
        var texture = GraphicsContext.CreateTexture(textureCreateDescriptor);
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
            if (textureCreateDescriptor.MipLevels > 1)
            {
                texture.GenerateMipmaps();
            }
            image.Dispose();
        }

        return texture;
    }
}