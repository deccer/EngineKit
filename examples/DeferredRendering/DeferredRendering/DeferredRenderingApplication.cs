using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using EngineKit.Mathematics;
using EngineKit.UI;
using ImGuiNET;
using Microsoft.Extensions.Options;
using Serilog;

namespace DeferredRendering;

internal sealed class DeferredRenderingApplication : GraphicsApplication
{
    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly ICapabilities _capabilities;
    private readonly IMetrics _metrics;
    private readonly ICamera _camera;
    private readonly IMeshLoader _meshLoader;
    private readonly IMaterialLibrary _materialLibrary;

    private Model? _deccerCubesModel;

    private IBuffer? _gpuVertexBuffer;
    private IBuffer? _gpuIndexBuffer;

    private SwapchainDescriptor _swapchainDescriptor;

    private ISampler? _pointSampler;

    private IGraphicsPipeline? _gBufferGraphicsPipeline;
    private IGraphicsPipeline? _gBufferVertexPullingGraphicsPipeline;
    private FramebufferDescriptor _gBufferFramebufferDescriptor;
    private ITexture? _gBufferBaseColorTexture;
    private ITexture? _gBufferNormalTexture;
    private ITexture? _gBufferDepthTexture;

    private IGraphicsPipeline? _finalGraphicsPipeline;
    private FramebufferDescriptor _finalFramebufferDescriptor;
    private ITexture? _finalTexture;

    private GpuCameraConstants _gpuCameraConstants;
    private IBuffer? _gpuCameraConstantsBuffer;

    private readonly List<GpuModelMeshInstance> _gpuModelMeshInstances;
    private IBuffer? _gpuModelMeshInstanceBuffer;
    private readonly List<DrawCommand> _drawCommands;

    private readonly List<GpuMaterial> _gpuMaterials;
    private readonly List<string> _gpuMaterialsInUse;
    private IBuffer? _gpuMaterialBuffer;

    private readonly Dictionary<string, ITexture> _textures;
    private bool _useVertexPulling;

    public DeferredRenderingApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        ICapabilities capabilities,
        IMetrics metrics,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IUIRenderer uiRenderer,
        ICamera camera,
        IMeshLoader meshLoader,
        IMaterialLibrary materialLibrary,
        IMessageBus messageBus)
        : base(
            logger,
            windowSettings,
            contextSettings,
            applicationContext,
            capabilities,
            metrics,
            inputProvider,
            graphicsContext,
            uiRenderer,
            messageBus)
    {
        _logger = logger;
        _applicationContext = applicationContext;
        _capabilities = capabilities;
        _metrics = metrics;
        _camera = camera;
        _meshLoader = meshLoader;
        _materialLibrary = materialLibrary;

        _drawCommands = new List<DrawCommand>(256);
        _gpuModelMeshInstances = new List<GpuModelMeshInstance>(256);
        _gpuMaterials = new List<GpuMaterial>(256);
        _gpuMaterialsInUse = new List<string>(256);
        _textures = new Dictionary<string, ITexture>(16);
    }

    protected override void FramebufferResized()
    {
        DestroyResolutionDependentResources();
        CreateResolutionDependentResources();
        base.FramebufferResized();
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
            return false;
        }

        CreateSamplers();
        CreateResolutionDependentResources();
        CreateSwapchainDescriptor();

        if (!CreatePipelines())
        {
            return false;
        }

        LoadModels();
        PrepareScene();

        _gpuModelMeshInstanceBuffer = GraphicsContext.CreateTypedBuffer<GpuModelMeshInstance>("ModelMeshInstances", 256, BufferStorageFlags.DynamicStorage);

        _gpuCameraConstants.ViewProjection = _camera.ViewMatrix * _camera.ProjectionMatrix;
        _gpuCameraConstantsBuffer = GraphicsContext.CreateTypedBuffer<GpuCameraConstants>("CameraConstants", 1, BufferStorageFlags.DynamicStorage);

        _camera.ProcessMouseMovement();
        
        return true;
    }

    protected override void Render(float deltaTime, float elapsedSeconds)
    {
        _gpuCameraConstants.ViewProjection = _camera.ViewMatrix * _camera.ProjectionMatrix;
        _gpuCameraConstantsBuffer!.UpdateElement(_gpuCameraConstants, Offset.Zero);
        _gpuModelMeshInstanceBuffer!.UpdateElements(_gpuModelMeshInstances.ToArray(), 0);

        GraphicsContext.BeginRenderPass(_gBufferFramebufferDescriptor);
        if (_useVertexPulling)
        {
            GraphicsContext.BindGraphicsPipeline(_gBufferVertexPullingGraphicsPipeline!);
            _gBufferVertexPullingGraphicsPipeline!.BindAsUniformBuffer(_gpuCameraConstantsBuffer, 0, Offset.Zero, SizeInBytes.Whole);
            _gBufferVertexPullingGraphicsPipeline.BindAsShaderStorageBuffer(_gpuModelMeshInstanceBuffer, 1, Offset.Zero, SizeInBytes.Whole);
            _gBufferVertexPullingGraphicsPipeline.BindAsShaderStorageBuffer(_gpuMaterialBuffer!, 2, Offset.Zero, SizeInBytes.Whole);
            _gBufferVertexPullingGraphicsPipeline.BindAsShaderStorageBuffer(_gpuVertexBuffer!, 3, Offset.Zero, SizeInBytes.Whole);
            _gBufferVertexPullingGraphicsPipeline.BindAsIndexBuffer(_gpuIndexBuffer!);

            for (var i = 0; i < _drawCommands.Count; i++)
            {
                var drawCommand = _drawCommands[i];
                _gBufferVertexPullingGraphicsPipeline.DrawElementsInstancedBaseVertexBaseInstance(
                    drawCommand.IndexCount,
                    drawCommand.IndexOffset,
                    1,
                    drawCommand.VertexOffset,
                    i);
            }
        }
        else
        {
            GraphicsContext.BindGraphicsPipeline(_gBufferGraphicsPipeline!);
            _gBufferGraphicsPipeline!.BindAsUniformBuffer(_gpuCameraConstantsBuffer, 0, Offset.Zero, SizeInBytes.Whole);
            _gBufferGraphicsPipeline.BindAsShaderStorageBuffer(_gpuModelMeshInstanceBuffer, 1, Offset.Zero, SizeInBytes.Whole);
            _gBufferGraphicsPipeline.BindAsShaderStorageBuffer(_gpuMaterialBuffer!, 2, Offset.Zero, SizeInBytes.Whole);
            _gBufferGraphicsPipeline.BindAsVertexBuffer(_gpuVertexBuffer!, 0, VertexPositionNormalUvTangent.Stride, Offset.Zero);
            _gBufferGraphicsPipeline.BindAsIndexBuffer(_gpuIndexBuffer!);

            for (var i = 0; i < _drawCommands.Count; i++)
            {
                var drawCommand = _drawCommands[i];
                _gBufferGraphicsPipeline.DrawElementsInstancedBaseVertexBaseInstance(
                    drawCommand.IndexCount,
                    drawCommand.IndexOffset,
                    1,
                    drawCommand.VertexOffset,
                    i);
            }
        }
        GraphicsContext.EndRenderPass();

        GraphicsContext.BeginRenderPass(_finalFramebufferDescriptor);
        GraphicsContext.BindGraphicsPipeline(_finalGraphicsPipeline!);
        _finalGraphicsPipeline!.BindSampledTexture(_pointSampler!, _gBufferBaseColorTexture!, 0);
        _finalGraphicsPipeline.DrawArrays(3, 0);
        GraphicsContext.EndRenderPass();

        GraphicsContext.BeginRenderPass(_swapchainDescriptor);
        GraphicsContext.BlitFramebufferToSwapchain(
            _applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y,
            _applicationContext.FramebufferSize.X,
            _applicationContext.FramebufferSize.Y);

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

                ImGui.Checkbox("Use Vertex Pulling", ref _useVertexPulling);

                ImGui.Image((nint)_gBufferBaseColorTexture!.Id, new Vector2(320, 180), new Vector2(0, 1), new Vector2(1, 0));
                ImGui.Image((nint)_gBufferNormalTexture!.Id, new Vector2(320, 180), new Vector2(0, 1), new Vector2(1, 0));
                ImGui.Image((nint)_gBufferDepthTexture!.Id, new Vector2(320, 180), new Vector2(0, 1), new Vector2(1, 0));

                ImGui.End();
            }
        }
        UIRenderer.EndLayout();
        GraphicsContext.EndRenderPass();
        GL.Finish();
    }

    protected override void Unload()
    {
        DestroyResolutionDependentResources();

        _pointSampler?.Dispose();

        base.Unload();
    }

    protected override void Update(float deltaTime, float elapsedSeconds)
    {
        base.Update(deltaTime, elapsedSeconds);

        if (IsMousePressed(Glfw.MouseButton.ButtonRight))
        {
            _camera.ProcessMouseMovement();
        }

        if (IsKeyPressed(Glfw.Key.KeyEscape))
        {
            Close();
        }

        var movement = Vector3.Zero;
        var speedFactor = 10.0f;
        if (IsKeyPressed(Glfw.Key.KeyW))
        {
            movement += _camera.Direction;
        }
        if (IsKeyPressed(Glfw.Key.KeyS))
        {
            movement -= _camera.Direction;
        }
        if (IsKeyPressed(Glfw.Key.KeyA))
        {
            movement += -_camera.Right;
        }
        if (IsKeyPressed(Glfw.Key.KeyD))
        {
            movement += _camera.Right;
        }

        movement = Vector3.Normalize(movement);
        if (IsKeyPressed(Glfw.Key.KeyLeftShift))
        {
            movement *= speedFactor;
        }
        if (movement.Length() > 0.0f)
        {
            _camera.ProcessKeyboard();
        }
    }

    protected override void WindowResized()
    {
        CreateSwapchainDescriptor();
    }

    private void PrepareScene()
    {
        var meshPrimitives = new List<MeshPrimitive>();

        _drawCommands.Add(new DrawCommand { Name = "Cube", WorldMatrix = Matrix4x4.CreateTranslation(-4, 0, 0) });
        _drawCommands.Add(new DrawCommand { Name = "Cube.003", WorldMatrix = Matrix4x4.CreateTranslation(4, 0, 0) });
        _drawCommands.Add(new DrawCommand { Name = "Cube.004", WorldMatrix = Matrix4x4.CreateTranslation(0, 4, 0) });
        _drawCommands.Add(new DrawCommand { Name = "Cube.001", WorldMatrix = Matrix4x4.CreateTranslation(0, 0, 0) });

        foreach (var drawCommand in _drawCommands)
        {
            var modelMesh = _deccerCubesModel!.ModelMeshes.FirstOrDefault(m => m.Name == drawCommand.Name);

            if (!_gpuMaterialsInUse.Contains(modelMesh!.MeshData.MaterialName!))
            {
                var material = _materialLibrary.GetMaterialByName(modelMesh.MeshData.MaterialName);
                if (!_textures.TryGetValue(material.BaseColorImage!.Name, out var texture))
                {
                    texture = material.BaseColorImage.ImageData.HasValue
                        ? GraphicsContext.CreateTextureFromMemory(material.BaseColorImage,
                            Format.R8G8B8A8Srgb,
                            material.BaseColorImage.Name, true, true, false)
                        : GraphicsContext.CreateTextureFromFile(material.BaseColorImage.FileName!, Format.R8G8B8A8Srgb, true, true, false);
                    if (texture != null)
                    {
                        texture.MakeResident();
                        _textures.Add(material.BaseColorImage.Name, texture);
                    }
                }

                _gpuMaterials.Add(new GpuMaterial
                {
                    BaseColor = new Color4(0.1f, 0.1f, 0.1f, 1.0f),
                    BaseColorTexture = texture!.TextureHandle
                });
                _gpuMaterialsInUse.Add(modelMesh.MeshData.MaterialName!);
            }

            var materialIndex = _gpuMaterialsInUse.IndexOf(modelMesh.MeshData.MaterialName!);
            _gpuModelMeshInstances.Add(new GpuModelMeshInstance
            {
                WorldMatrix = drawCommand.WorldMatrix,
                MaterialId = new Int4(materialIndex, 0, 0, 0)
            });

            if (!meshPrimitives.Contains(modelMesh.MeshData))
            {
                meshPrimitives.Add(modelMesh.MeshData);
            }
        }

        var indexOffset = 0;
        var vertexOffset = 0;
        foreach (var meshPrimitive in meshPrimitives)
        {
            meshPrimitive.VertexOffset = vertexOffset;
            meshPrimitive.IndexOffset = indexOffset;
            vertexOffset += meshPrimitive.VertexCount;
            indexOffset += meshPrimitive.IndexCount;
        }

        foreach (var drawCommand in _drawCommands)
        {
            var meshPrimitive = meshPrimitives.FirstOrDefault(m => m.MeshName == drawCommand.Name);
            drawCommand.IndexCount = meshPrimitive!.IndexCount;
            drawCommand.IndexOffset = meshPrimitive.IndexOffset;
            drawCommand.VertexOffset = meshPrimitive.VertexOffset;
        }

        _gpuMaterialBuffer = GraphicsContext.CreateTypedBuffer<GpuMaterial>("SceneMaterials", (uint)_gpuMaterials.Count, BufferStorageFlags.DynamicStorage);
        _gpuMaterialBuffer.UpdateElements(_gpuMaterials.ToArray(), 0);

        var meshDatasAsArray = meshPrimitives.ToArray();

        _gpuVertexBuffer = GraphicsContext.CreateVertexBuffer("SceneVertices", meshDatasAsArray);
        _gpuIndexBuffer = GraphicsContext.CreateIndexBuffer("SceneIndices", meshDatasAsArray);
    }

    private void DestroyResolutionDependentResources()
    {
        _gBufferBaseColorTexture?.Dispose();
        _gBufferNormalTexture?.Dispose();
        _gBufferDepthTexture?.Dispose();
        GraphicsContext.RemoveFramebuffer(_gBufferFramebufferDescriptor);

        _finalTexture?.Dispose();
        GraphicsContext.RemoveFramebuffer(_finalFramebufferDescriptor);
    }

    private void CreateSamplers()
    {
        _pointSampler = new SamplerBuilder(GraphicsContext)
            .WithInterpolationFilter(TextureInterpolationFilter.Nearest)
            .WithMipmapFilter(TextureMipmapFilter.Nearest)
            .WithAddressMode(TextureAddressMode.ClampToEdge)
            .Build("PointSampler");
    }

    private void CreateResolutionDependentResources()
    {
        _gBufferBaseColorTexture = GraphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.R8G8B8A8UNorm, "BaseColor");
        _gBufferNormalTexture = GraphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.R16G16B16Float, "Normals");
        _gBufferDepthTexture = GraphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.D32UNorm, "Depth");

        _gBufferFramebufferDescriptor = GraphicsContext.GetFramebufferDescriptorBuilder()
            .WithColorAttachment(_gBufferBaseColorTexture, true, Colors.DarkSlateBlue)
            .WithColorAttachment(_gBufferNormalTexture, true, Vector4.Zero)
            .WithDepthAttachment(_gBufferDepthTexture, true)
            .WithViewport(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y)
            .Build("GBuffer");

        _finalTexture = GraphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.R8G8B8A8UNorm, "Final");
        _finalFramebufferDescriptor = GraphicsContext.GetFramebufferDescriptorBuilder()
            .WithColorAttachment(_finalTexture, true, new Vector4(0.2f, 0.2f, 0.2f, 1.0f))
            .WithViewport(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y)
            .Build("Final");
    }

    private void CreateSwapchainDescriptor()
    {
        _swapchainDescriptor = GraphicsContext.GetSwapchainDescriptorBuilder()
            .ClearColor(MathHelper.GammaToLinear(Colors.DarkSlateBlue))
            .WithFramebufferSizeAsViewport()
            .Build("Swapchain");
    }

    private bool CreatePipelines()
    {
        var gBufferGraphicsPipelineResult = GraphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/Scene.vs.glsl", "Shaders/Scene.fs.glsl")
            .WithVertexAttributesFromDescriptor(new VertexInputDescriptorBuilder()
                .AddAttribute(0, Format.R32G32B32Float, 0)
                .AddAttribute(0, Format.R32G32B32Float, 12)
                .AddAttribute(0, Format.R32G32Float, 24)
                .AddAttribute(0, Format.R32G32B32A32Float, 32)
                .Build("Scene"))
            .WithCullingEnabled(CullMode.Back)
            .Build("GBuffer-Pipeline");
        if (gBufferGraphicsPipelineResult.IsFailure)
        {
            _logger.Error(gBufferGraphicsPipelineResult.Error);
            return false;
        }

        _gBufferGraphicsPipeline = gBufferGraphicsPipelineResult.Value;
        
        var gBufferVertexPullingGraphicsPipelineResult = GraphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/SceneVertexPulling.vs.glsl", "Shaders/Scene.fs.glsl")
            .WithCullingEnabled(CullMode.Back)
            .Build("GBuffer-Pipeline");
        if (gBufferVertexPullingGraphicsPipelineResult.IsFailure)
        {
            _logger.Error(gBufferVertexPullingGraphicsPipelineResult.Error);
            return false;
        }

        _gBufferVertexPullingGraphicsPipeline = gBufferVertexPullingGraphicsPipelineResult.Value;

        var finalGraphicsPipelineResult = GraphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/FST.vs.glsl", "Shaders/Texture.fs.glsl")
            .WithVertexAttributesFromDescriptor(new VertexInputDescriptorBuilder()
                .AddAttribute(0, Format.R32G32B32Float, 0)
                .AddAttribute(0, Format.R32G32Float, 12)
                .Build("FST"))
            .Build("Final-Pipeline");
        if (finalGraphicsPipelineResult.IsFailure)
        {
            _logger.Error(finalGraphicsPipelineResult.Error);
            return false;
        }

        _finalGraphicsPipeline = finalGraphicsPipelineResult.Value;

        return true;
    }

    private Model LoadModel(string modelName, string modelFilePath)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var modelMeshes = _meshLoader
            .LoadMeshPrimitivesFromFile(Path.Combine(baseDirectory, modelFilePath))
            .Select(meshData => new ModelMesh(meshData.MeshName, meshData))
            .ToArray();

        return new Model(modelName, modelMeshes);
    }

    private void LoadModels()
    {
        _deccerCubesModel = LoadModel("Deccer-Cubes", "Data/Default/SM_Deccer_Cubes_Textured.gltf");
    }
}