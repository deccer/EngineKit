using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Native.Glfw;
using ImGuiNET;
using Microsoft.Extensions.Options;
using OpenTK.Mathematics;
using Serilog;

namespace ForwardRenderer;

public struct ModelMesh
{
    public string MeshName;

    public int VertexOffset;

    public int VertexCount;

    public int IndexOffset;

    public int IndexCount;

    public Matrix4 WorldMatrix;

    public ulong TextureHandle;
}

internal sealed class ForwardRendererApplication : GraphicsApplication
{
    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly IMetrics _metrics;
    private readonly IImageLoader _imageLoader;
    private readonly IMeshLoader _meshLoader;
    private readonly ICamera _camera;
    private readonly IList<ModelMeshInstance> _modelMeshInstances;
    private readonly IList<GpuMaterial> _gpuMaterials;

    private ITexture? _skullBaseColorTexture;
    private SwapchainRenderDescriptor _swapchainRenderDescriptor;

    private IGraphicsPipeline? _sceneGraphicsPipeline;

    private GpuConstants _gpuConstants;
    private IUniformBuffer? _gpuConstantsBuffer;
    private IShaderStorageBuffer? _gpuModelMeshInstanceBuffer;
    private IShaderStorageBuffer? _gpuMaterialBuffer;

    private IVertexBuffer? _skullVertexBuffer;
    private IIndexBuffer? _skullIndexBuffer;

    private ISampler? _linearMipmapNearestSampler;
    private ISampler? _linearMipmapLinear;

    private readonly IList<ModelMesh> _modelMeshes;
    private IList<GpuModelMeshInstance> _gpuModelMeshInstances;
    private readonly List<GpuIndirectElementData> _gpuIndirectElements;
    private IIndirectBuffer _gpuIndirectElementDataBuffer;

    public ForwardRendererApplication(
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
        _gpuMaterials = new List<GpuMaterial>();
        _camera.Sensitivity = 0.25f;

        _modelMeshes = new List<ModelMesh>();
        _modelMeshInstances = new List<ModelMeshInstance>();
        _gpuModelMeshInstances = new List<GpuModelMeshInstance>();
        _gpuIndirectElements = new List<GpuIndirectElementData>();
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

        _gpuConstantsBuffer = GraphicsContext.CreateUniformBuffer<GpuConstants>("Camera");
        _gpuConstantsBuffer.AllocateStorage(_gpuConstants, StorageAllocationFlags.Dynamic);

        _modelMeshInstances.Add(new ModelMeshInstance
        {
            ModelMesh = _modelMeshes.First(),
            World = Matrix4.CreateTranslation(-5, 0, 0)
        });
        _modelMeshInstances.Add(new ModelMeshInstance
        {
            ModelMesh = _modelMeshes.First(),
            World = Matrix4.CreateTranslation(0, 0, 0)
        });
        _modelMeshInstances.Add(new ModelMeshInstance
        {
            ModelMesh = _modelMeshes.First(),
            World = Matrix4.CreateTranslation(+5, 0, 0)
        });

        _gpuModelMeshInstanceBuffer = GraphicsContext.CreateShaderStorageBuffer<GpuModelMeshInstance>("ModelMeshInstances");
        _gpuModelMeshInstanceBuffer.AllocateStorage(3 * Marshal.SizeOf<GpuModelMeshInstance>(), StorageAllocationFlags.Dynamic);
        _gpuIndirectElementDataBuffer = GraphicsContext.CreateIndirectBuffer("MeshIndirectDrawElement");
        _gpuIndirectElementDataBuffer.AllocateStorage(3 * Marshal.SizeOf<GpuIndirectElementData>(), StorageAllocationFlags.Dynamic);

        _gpuMaterials.Add(new GpuMaterial
        {
            BaseColor = new Vector4(1.1f, 0.2f, 0.3f, 1.0f),
            BaseColorTextureHandle = _skullBaseColorTexture.TextureHandle
        });
        _gpuMaterials.Add(new GpuMaterial
        {
            BaseColor = new Vector4(0.2f, 0.3f, 1.5f, 1.0f),
            BaseColorTextureHandle = _skullBaseColorTexture.TextureHandle
        });
        _gpuMaterials.Add(new GpuMaterial
        {
            BaseColor = new Vector4(0.3f, 1.4f, 0.5f, 1.0f),
            BaseColorTextureHandle = _skullBaseColorTexture.TextureHandle
        });

        _gpuMaterialBuffer = GraphicsContext.CreateShaderStorageBuffer<GpuMaterial>("Materials");
        _gpuMaterialBuffer.AllocateStorage(_gpuMaterials.ToArray(), StorageAllocationFlags.Dynamic);

        return true;
    }

    protected override void Render()
    {
        _gpuModelMeshInstances = _modelMeshInstances.Select(mm => new GpuModelMeshInstance { World = mm.World }).ToList();
        _gpuModelMeshInstanceBuffer.Update(_gpuModelMeshInstances.ToArray(), 0);

        _gpuIndirectElements.Clear();
        foreach (var modelMeshInstance in _modelMeshInstances)
        {
            var gpuIndirectElement = new GpuIndirectElementData
            {
                IndexCount = (uint)modelMeshInstance.ModelMesh.IndexCount,
                BaseInstance = 0,
                BaseVertex = modelMeshInstance.ModelMesh.VertexOffset,
                FirstIndex = (uint)modelMeshInstance.ModelMesh.IndexOffset,
                InstanceCount = 1
            };
            _gpuIndirectElements.Add(gpuIndirectElement);
        }

        _gpuIndirectElementDataBuffer.Update(_gpuIndirectElements.ToArray(), 0);

        _gpuConstants.ViewProjection = _camera.ViewMatrix * _camera.ProjectionMatrix;
        _gpuConstantsBuffer!.Update(_gpuConstants, 0);

        GraphicsContext.BeginRenderToSwapchain(_swapchainRenderDescriptor);
        GraphicsContext.BindGraphicsPipeline(_sceneGraphicsPipeline!);
        _sceneGraphicsPipeline!.BindVertexBuffer(_skullVertexBuffer!, 0, 0);
        _sceneGraphicsPipeline.BindIndexBuffer(_skullIndexBuffer!);

        _sceneGraphicsPipeline.BindUniformBuffer(_gpuConstantsBuffer, 0);
        _sceneGraphicsPipeline.BindShaderStorageBuffer(_gpuModelMeshInstanceBuffer!, 1);
        _sceneGraphicsPipeline.BindShaderStorageBuffer(_gpuMaterialBuffer!, 2);
        _sceneGraphicsPipeline.BindSampledTexture(_linearMipmapLinear!, _skullBaseColorTexture!, 0);

        _sceneGraphicsPipeline.MultiDrawElementsIndirect(_gpuIndirectElementDataBuffer, _gpuIndirectElements.Count);

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
        _gpuModelMeshInstanceBuffer?.Dispose();

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
        var meshDates = _meshLoader.LoadModel(Path.Combine(baseDirectory, "Data/Props/Skull/SM_Skull_Optimized_point2.gltf")).ToArray();

        var indexOffset = 0;
        var vertexOffset = 0;
        foreach (var meshData in meshDates)
        {
            meshData.IndexOffset = indexOffset;
            meshData.VertexOffset = vertexOffset;

            var obj = new ModelMesh
            {
                IndexCount = meshData.IndexCount,
                IndexOffset = meshData.IndexOffset,
                VertexCount = meshData.VertexCount,
                VertexOffset = meshData.VertexOffset,
                MeshName = meshData.MeshName
            };
            _modelMeshes.Add(obj);

            indexOffset += meshData.IndexCount;
            vertexOffset += meshData.VertexCount;
        }

        _skullVertexBuffer = GraphicsContext.CreateVertexBuffer("SkullVertices", meshDates, VertexType.PositionNormalUvTangent);
        _skullIndexBuffer = GraphicsContext.CreateIndexBuffer("SkullIndices", meshDates);

        return true;
    }

    private bool LoadResources()
    {
        _skullBaseColorTexture = GraphicsContext.CreateTextureFromFile("Data/Props/Skull/TD_Checker_Base_Color.png", true);
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
}