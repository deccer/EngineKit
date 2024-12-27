using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using EngineKit;
using EngineKit.Core;
using EngineKit.Core.Messages;
using EngineKit.Graphics;
using EngineKit.Graphics.Assets;
using EngineKit.Graphics.RHI;
using EngineKit.Mathematics;
using EngineKit.UI;
using ImGuiNET;
using Serilog;

namespace ForwardRendering;

internal class ForwardRenderer : IRenderer
{
    private readonly ILogger _logger;
    private readonly IGraphicsContext _graphicsContext;
    private readonly IApplicationContext _applicationContext;
    private readonly IMessageBus _messageBus;
    private readonly ICapabilities _capabilities;
    private readonly IMetrics _metrics;
    private readonly ICamera _camera;
    private readonly IMeshLoader _meshLoader;
    private readonly IUIRenderer _uiRenderer;

    private IGraphicsPipeline? _sceneGraphicsPipeline;

    private GpuConstants _gpuConstants;
    private IBuffer? _gpuConstantsBuffer;
    private IBuffer? _gpuModelMeshInstanceBuffer;
    private IBuffer? _gpuMaterialBuffer;

    private IBuffer? _skullVertexBuffer;
    private IBuffer? _skullIndexBuffer;

    private ISampler? _linearMipmapNearestSampler;
    private ISampler? _linearMipmapLinear;

    private readonly List<ModelMesh> _modelMeshes;
    private List<GpuModelMeshInstance> _gpuModelMeshInstances;
    private readonly List<GpuDrawElementIndirectCommand> _gpuIndirectElements;
    private IBuffer? _gpuIndirectElementDataBuffer;

    private readonly List<ModelMeshInstance> _modelMeshInstances;
    private readonly List<GpuMaterial> _gpuMaterials;

    private ITexture? _skullBaseColorTexture;

    private SwapchainDescriptor _swapchainDescriptor;

    public ForwardRenderer(ILogger logger,
        IGraphicsContext graphicsContext,
        IApplicationContext applicationContext,
        IMessageBus messageBus,
        ICapabilities capabilities,
        IMetrics metrics,
        ICamera camera,
        IMeshLoader meshLoader,
        IUIRenderer uiRenderer)
    {
        _logger = logger.ForContext<ForwardRenderer>();
        _graphicsContext = graphicsContext;
        _applicationContext = applicationContext;
        _messageBus = messageBus;
        _capabilities = capabilities;
        _metrics = metrics;
        _camera = camera;
        _meshLoader = meshLoader;
        _uiRenderer = uiRenderer;

        _gpuConstants = new GpuConstants();
        _gpuMaterials = new List<GpuMaterial>();
        _modelMeshes = new List<ModelMesh>();
        _modelMeshInstances = new List<ModelMeshInstance>();
        _gpuModelMeshInstances = new List<GpuModelMeshInstance>();
        _gpuIndirectElements = new List<GpuDrawElementIndirectCommand>();
    }

    public void Dispose()
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
    }

    public bool Load()
    {
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

        _modelMeshInstances.Add(new ModelMeshInstance
        {
            ModelMesh = _modelMeshes.First(),
            World = Matrix4x4.CreateTranslation(-5, 0, 0)
        });
        _modelMeshInstances.Add(new ModelMeshInstance
        {
            ModelMesh = _modelMeshes.First(),
            World = Matrix4x4.CreateTranslation(0, 0, 0)
        });
        _modelMeshInstances.Add(new ModelMeshInstance
        {
            ModelMesh = _modelMeshes.First(),
            World = Matrix4x4.CreateTranslation(+5, 0, 0)
        });
        _gpuMaterials.Add(new GpuMaterial
        {
            BaseColor = new Vector4(1.1f, 0.2f, 0.3f, 1.0f),
            BaseColorTextureHandle = _skullBaseColorTexture!.TextureHandle
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

        _gpuConstantsBuffer = _graphicsContext.CreateTypedBuffer<GpuConstants>("Camera", 1, BufferStorageFlags.DynamicStorage);
        _gpuMaterialBuffer = _graphicsContext.CreateTypedBuffer<GpuMaterial>("Materials", (uint)_gpuMaterials.Count, BufferStorageFlags.DynamicStorage);
        _gpuMaterialBuffer.UpdateElements(_gpuMaterials.ToArray(), 0);
        
        _gpuModelMeshInstanceBuffer =
            _graphicsContext.CreateTypedBuffer<GpuModelMeshInstance>("ModelMeshInstances", 3, BufferStorageFlags.DynamicStorage);
        _gpuIndirectElementDataBuffer =
            _graphicsContext.CreateTypedBuffer<GpuDrawElementIndirectCommand>("MeshIndirectDrawElement", 3,
                                                                              BufferStorageFlags.DynamicStorage);
        return true;
    }

    public void Render(float deltaTime,
        float elapsedTime)
    {
        _gpuModelMeshInstances = _modelMeshInstances.Select((mm,
            index) =>
        {
            var rotationMatrix = index switch
            {
                0 => Matrix4x4.CreateRotationX(1.0f * elapsedTime),
                1 => Matrix4x4.CreateRotationZ(0.5f * elapsedTime),
                2 => Matrix4x4.CreateRotationY(2.0f * elapsedTime),
                _ => Matrix4x4.Identity
            };

            return new GpuModelMeshInstance
            {
                World = rotationMatrix * mm.World
            };
        }).ToList();
        _gpuModelMeshInstanceBuffer!.UpdateElements(_gpuModelMeshInstances.ToArray(), 0);

        _gpuIndirectElements.Clear();
        foreach (var modelMeshInstance in _modelMeshInstances)
        {
            var gpuIndirectElement = new GpuDrawElementIndirectCommand
            {
                IndexCount = (uint)modelMeshInstance.ModelMesh.IndexCount,
                BaseInstance = 0,
                BaseVertex = (uint)modelMeshInstance.ModelMesh.VertexOffset,
                FirstIndex = (uint)modelMeshInstance.ModelMesh.IndexOffset,
                InstanceCount = 1
            };
            _gpuIndirectElements.Add(gpuIndirectElement);
        }

        _gpuIndirectElementDataBuffer!.UpdateElements(_gpuIndirectElements.ToArray(), 0);

        _gpuConstants.ViewProjection = _camera.ViewMatrix * _camera.ProjectionMatrix;
        _gpuConstantsBuffer!.UpdateElement(_gpuConstants, 0);

        _graphicsContext.BeginRenderPass(_swapchainDescriptor);
        _graphicsContext.BindGraphicsPipeline(_sceneGraphicsPipeline!);
        _sceneGraphicsPipeline!.BindAsVertexBuffer(_skullVertexBuffer!, 0, GpuVertexPositionNormalUvTangent.Stride, 0);
        _sceneGraphicsPipeline.BindAsIndexBuffer(_skullIndexBuffer!);

        _sceneGraphicsPipeline.BindAsUniformBuffer(_gpuConstantsBuffer, 0, Offset.Zero, SizeInBytes.Whole);
        _sceneGraphicsPipeline.BindAsShaderStorageBuffer(_gpuModelMeshInstanceBuffer!, 1, Offset.Zero, SizeInBytes.Whole);
        _sceneGraphicsPipeline.BindAsShaderStorageBuffer(_gpuMaterialBuffer!, 2, Offset.Zero, SizeInBytes.Whole);
        _sceneGraphicsPipeline.BindSampledTexture(_linearMipmapLinear!, _skullBaseColorTexture!, 0);

        _sceneGraphicsPipeline.MultiDrawElementsIndirect(_gpuIndirectElementDataBuffer, (uint)_gpuIndirectElements.Count);

        _graphicsContext.EndRenderPass();
    }

    public void RenderUi(float deltaTime,
        float elapsedTime)
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Quit"))
                    {
                        _messageBus.PublishWait(new CloseWindowMessage());
                    }

                    ImGui.EndMenu();
                }

                var isNvidia = _capabilities.SupportsNvx;
                if (isNvidia)
                {
                    ImGui.SetCursorPos(new Vector2(ImGui.GetWindowViewport().Size.X - 416, 0));
                    ImGui.TextUnformatted($"video memory: {_capabilities.GetCurrentAvailableGpuMemoryInMebiBytes()} MiB");
                    ImGui.SameLine();
                }
                else
                {
                    ImGui.SetCursorPos(new Vector2(ImGui.GetWindowViewport().Size.X - 256, 0));
                }

                ImGui.TextUnformatted($"avg frame time: {_metrics.AverageFrameTime:F2} ms");
                ImGui.SameLine();
                ImGui.Button(MaterialDesignIcons.WindowMinimize);
                ImGui.SameLine();
                if (ImGui.Button(_applicationContext.IsWindowMaximized
                        ? MaterialDesignIcons.WindowRestore
                        : MaterialDesignIcons.WindowMaximize))
                {
                    if (_applicationContext.IsWindowMaximized)
                    {
                        _messageBus.PublishWait(new RestoreWindowMessage());
                    }
                    else
                    {
                        _messageBus.PublishWait(new MaximizeWindowMessage());
                    }
                }

                ImGui.SameLine();
                if (ImGui.Button(MaterialDesignIcons.WindowClose))
                {
                    _messageBus.PublishWait(new CloseWindowMessage());
                }

                ImGui.EndMainMenuBar();
            }

            if (ImGui.Begin("Debug"))
            {
                var sensitivity = _camera.Sensitivity;
                ImGui.SliderFloat("Camera Sensitivity", ref sensitivity, 0.01f, 1.0f);
                _camera.Sensitivity = sensitivity;

                ImGui.End();
            }
        }

        _uiRenderer.ShowDemoWindow();
    }

    public void WindowFramebufferResized()
    {
        _swapchainDescriptor = _graphicsContext.GetSwapchainDescriptorBuilder()
            .ClearColor(MathHelper.GammaToLinear(Colors.DarkSlateBlue))
            .ClearDepth(1.0f)
            .WithFramebufferSizeAsViewport()
            .Build("Swapchain");
    }

    private bool LoadRenderDescriptors()
    {
        _swapchainDescriptor = _graphicsContext.GetSwapchainDescriptorBuilder()
            .ClearColor(MathHelper.GammaToLinear(Colors.DarkSlateBlue))
            .ClearDepth(1.0f)
            .WithFramebufferSizeAsViewport()
            .Build("Swapchain");

        return true;
    }

    private bool LoadPipelines()
    {
        var graphicsPipelineResult = _graphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/Scene.vs.glsl", "Shaders/Scene.fs.glsl")
            .WithVertexAttributesFromDescriptor(new VertexInputDescriptorBuilder()
                .AddAttribute(0, Format.R32G32B32Float, 0)
                .AddAttribute(0, Format.R32G32B32Float, 12)
                .AddAttribute(0, Format.R32G32Float, 24)
                .AddAttribute(0, Format.R32G32B32A32Float, 32)
                .Build(nameof(GpuVertexPositionNormalUvTangent)))
            .WithTopology(PrimitiveTopology.Triangles)
            .WithFaceWinding(FaceWinding.CounterClockwise)
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

    private bool LoadMeshes()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var meshDates = _meshLoader
            .LoadMeshPrimitivesFromFile("Data/Props/Skull/SM_Skull_Optimized_point2.gltf")
            .ToArray();

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

        _skullVertexBuffer = _graphicsContext.CreateVertexBuffer("SkullVertices", meshDates);
        _skullIndexBuffer = _graphicsContext.CreateIndexBuffer("SkullIndices", meshDates);

        return true;
    }

    private bool LoadResources()
    {
        _skullBaseColorTexture = _graphicsContext.CreateTextureFromFile(
            "Data/Props/Skull/TD_Checker_Base_Color.png",
            Format.R8G8B8A8Srgb,
            true,
            false,
            false);
        if (_skullBaseColorTexture == null)
        {
            return false;
        }

        _linearMipmapNearestSampler = _graphicsContext.CreateSamplerBuilder()
            .WithInterpolationFilter(TextureInterpolationFilter.Linear)
            .WithMipmapFilter(TextureMipmapFilter.LinearMipmapNearest)
            .WithAddressMode(TextureAddressMode.Repeat)
            .Build("LinearMipmapNearest");

        _linearMipmapLinear = _graphicsContext.CreateSamplerBuilder()
            .WithInterpolationFilter(TextureInterpolationFilter.Linear)
            .WithMipmapFilter(TextureMipmapFilter.LinearMipmapLinear)
            .Build("LinearMipmapLinear");

        return true;
    }
}
