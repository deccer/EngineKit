using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Mathematics;
using EngineKit.Native.Glfw;
using EngineKit.UI;
using ImGuiNET;
using Microsoft.Extensions.Options;
using Serilog;

namespace ForwardRendering;

internal sealed class ForwardRendererApplication : GraphicsApplication
{
    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly ICapabilities _capabilities;
    private readonly IMetrics _metrics;
    private readonly IMeshLoader _meshLoader;
    private readonly ICamera _camera;
    private readonly List<ModelMeshInstance> _modelMeshInstances;
    private readonly List<GpuMaterial> _gpuMaterials;

    private ITexture? _skullBaseColorTexture;
    private SwapchainDescriptor _swapchainDescriptor;

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
    private readonly List<DrawElementIndirectCommand> _gpuIndirectElements;
    private IBuffer? _gpuIndirectElementDataBuffer;

    public ForwardRendererApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        ICapabilities capabilities,
        IMetrics metrics,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IUIRenderer uiRenderer,
        IMeshLoader meshLoader,
        ICamera camera)
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
        _capabilities = capabilities;
        _applicationContext.ShowResizeInLog = true;
        _metrics = metrics;
        _meshLoader = meshLoader;
        _camera = camera;

        _gpuConstants = new GpuConstants();
        _gpuMaterials = new List<GpuMaterial>();
        _camera.Sensitivity = 0.125f;

        _modelMeshes = new List<ModelMesh>();
        _modelMeshInstances = new List<ModelMeshInstance>();
        _gpuModelMeshInstances = new List<GpuModelMeshInstance>();
        _gpuIndirectElements = new List<DrawElementIndirectCommand>();
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

        _gpuConstantsBuffer = GraphicsContext.CreateTypedBuffer<GpuConstants>("Camera", 1, BufferStorageFlags.DynamicStorage);

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

        _gpuModelMeshInstanceBuffer = GraphicsContext.CreateTypedBuffer<GpuModelMeshInstance>("ModelMeshInstances", 3, BufferStorageFlags.DynamicStorage);
        _gpuIndirectElementDataBuffer = GraphicsContext.CreateTypedBuffer<DrawElementIndirectCommand>("MeshIndirectDrawElement", 3, BufferStorageFlags.DynamicStorage);

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

        _gpuMaterialBuffer = GraphicsContext.CreateTypedBuffer<GpuMaterial>("Materials", (uint)_gpuMaterials.Count, BufferStorageFlags.DynamicStorage);
        _gpuMaterialBuffer.UpdateElements(_gpuMaterials.ToArray(), 0);

        return true;
    }

    protected override void Render(float deltaTime, float elapsedMilliseconds)
    {
        _gpuModelMeshInstances = _modelMeshInstances.Select((mm, index) =>
        {
            var rotationMatrix = index switch 
            {
                0 => Matrix4x4.CreateRotationX(0.0010f * elapsedMilliseconds),
                1 => Matrix4x4.CreateRotationZ(0.0005f * elapsedMilliseconds),
                2 => Matrix4x4.CreateRotationY(0.0020f * elapsedMilliseconds),                
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
            var gpuIndirectElement = new DrawElementIndirectCommand
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

        GraphicsContext.BeginRenderPass(_swapchainDescriptor);
        GraphicsContext.BindGraphicsPipeline(_sceneGraphicsPipeline!);
        _sceneGraphicsPipeline!.BindAsVertexBuffer(_skullVertexBuffer!, 0, VertexPositionNormalUvTangent.Stride, 0);
        _sceneGraphicsPipeline.BindAsIndexBuffer(_skullIndexBuffer!);

        _sceneGraphicsPipeline.BindAsUniformBuffer(_gpuConstantsBuffer, 0, Offset.Zero, SizeInBytes.Whole);
        _sceneGraphicsPipeline.BindAsShaderStorageBuffer(_gpuModelMeshInstanceBuffer!, 1, Offset.Zero, SizeInBytes.Whole);
        _sceneGraphicsPipeline.BindAsShaderStorageBuffer(_gpuMaterialBuffer!, 2, Offset.Zero, SizeInBytes.Whole);
        _sceneGraphicsPipeline.BindSampledTexture(_linearMipmapLinear!, _skullBaseColorTexture!, 0);

        _sceneGraphicsPipeline.MultiDrawElementsIndirect(_gpuIndirectElementDataBuffer, (uint)_gpuIndirectElements.Count);

        GraphicsContext.EndRenderPass();

        RenderUi();
    }

    protected override void FramebufferResized()
    {
        base.FramebufferResized();
        _swapchainDescriptor = new SwapchainDescriptorBuilder()
            .ClearColor(MathHelper.GammaToLinear(Colors.DarkSlateBlue))
            .ClearDepth()
            .WithViewport(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y)
            .Build("Swapchain");
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

    protected override void HandleDebugger(out bool breakOnError)
    {
        breakOnError = true;
    }

    protected override void Update(float deltaTime, float elapsedMilliseconds)
    {
        base.Update(deltaTime, elapsedMilliseconds);

        if (IsMousePressed(Glfw.MouseButton.ButtonRight))
        {
            _camera.ProcessMouseMovement();
        }
        _camera.ProcessKeyboard(Vector3.Zero, deltaTime);

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
                if (ImGui.Button(_applicationContext.IsWindowMaximized ? MaterialDesignIcons.WindowRestore : MaterialDesignIcons.WindowMaximize))
                {
                    if (_applicationContext.IsWindowMaximized)
                    {
                        RestoreWindow();
                    }
                    else
                    {
                        MaximizeWindow();
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button(MaterialDesignIcons.WindowClose))
                {
                    Close();
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
        UIRenderer.ShowDemoWindow();
        UIRenderer.EndLayout();
    }

    private bool LoadRenderDescriptors()
    {
        _swapchainDescriptor = new SwapchainDescriptorBuilder()
            .ClearColor(MathHelper.GammaToLinear(Colors.DarkSlateBlue))
            .ClearDepth()
            .WithViewport(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y)
            .Build("Swapchain");

        return true;
    }

    private bool LoadPipelines()
    {
        var graphicsPipelineResult = GraphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/Scene.vs.glsl", "Shaders/Scene.fs.glsl")
            .WithVertexAttributesFromDescriptor(new VertexInputDescriptorBuilder()
                .AddAttribute(0, Format.R32G32B32Float, 0)
                .AddAttribute(0, Format.R32G32B32Float, 12)
                .AddAttribute(0, Format.R32G32Float, 24)
                .AddAttribute(0, Format.R32G32B32A32Float, 32)
                .Build(nameof(VertexPositionNormalUvTangent)))
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
        var meshDates = _meshLoader.LoadMeshPrimitivesFromFile(Path.Combine(baseDirectory, "Data/Props/Skull/SM_Skull_Optimized_point2.gltf")).ToArray();

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

        _skullVertexBuffer = GraphicsContext.CreateVertexBuffer("SkullVertices", meshDates);
        _skullIndexBuffer = GraphicsContext.CreateIndexBuffer("SkullIndices", meshDates);

        return true;
    }

    private bool LoadResources()
    {
        _skullBaseColorTexture = GraphicsContext.CreateTextureFromFile(
            "Data/Props/Skull/TD_Checker_Base_Color.png",
            Format.R8G8B8A8Srgb,
            true,
            false,
            false);
        if (_skullBaseColorTexture == null)
        {
            return false;
        }

        _linearMipmapNearestSampler = GraphicsContext.CreateSamplerBuilder()
            .WithInterpolationFilter(TextureInterpolationFilter.Linear)
            .WithMipmapFilter(TextureMipmapFilter.LinearMipmapNearest)
            .WithAddressMode(TextureAddressMode.Repeat)
            .Build("LinearMipmapNearest");

        _linearMipmapLinear = GraphicsContext.CreateSamplerBuilder()
            .WithInterpolationFilter(TextureInterpolationFilter.Linear)
            .WithMipmapFilter(TextureMipmapFilter.LinearMipmapLinear)
            .Build("LinearMipmapLinear");

        return true;
    }
}