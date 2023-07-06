using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using ImGuiNET;
using Microsoft.Extensions.Options;
using EngineKit.Mathematics;
using Serilog;
using Num = System.Numerics;

namespace DeferredRendering;

public class DrawCommand
{
    public string Name { get; set; }

    public Matrix WorldMatrix { get; set; }

    public int IndexCount;

    public int IndexOffset;

    public int VertexOffset;
}

internal sealed class DeferredRenderingApplication : GraphicsApplication
{
    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly IMetrics _metrics;
    private readonly ICamera _camera;
    private readonly IMeshLoader _meshLoader;
    private readonly IMaterialLibrary _materialLibrary;

    private Model _deccerCubesModel;

    private IVertexBuffer? _gpuVertexBuffer;
    private IIndexBuffer? _gpuIndexBuffer;

    private SwapchainDescriptor _swapchainDescriptor;

    private ISampler? _pointSampler;

    private IGraphicsPipeline? _gBufferGraphicsPipeline;
    private FramebufferDescriptor _gBufferFramebufferDescriptor;
    private ITexture? _gBufferBaseColorTexture;
    private ITexture? _gBufferNormalTexture;
    private ITexture? _gBufferDepthTexture;

    private IGraphicsPipeline? _finalGraphicsPipeline;
    private FramebufferDescriptor _finalFramebufferDescriptor;
    private ITexture? _finalTexture;

    private GpuCameraConstants _gpuCameraConstants;
    private IUniformBuffer? _gpuCameraConstantsBuffer;

    private IList<GpuModelMeshInstance> _gpuModelMeshInstances;
    private IShaderStorageBuffer? _gpuModelMeshInstanceBuffer;
    private IList<DrawCommand> _drawCommands;

    private IList<GpuMaterial> _gpuMaterials;
    private IList<string> _gpuMaterialsInUse;
    private IShaderStorageBuffer _gpuMaterialBuffer;

    private IDictionary<string, ITexture> _textures;

    public DeferredRenderingApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        IMetrics metrics,
        ILimits limits,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IUIRenderer uiRenderer,
        ICamera camera,
        IMeshLoader meshLoader,
        IMaterialLibrary materialLibrary)
        : base(logger, windowSettings, contextSettings, applicationContext, metrics, limits, inputProvider, graphicsContext,
            uiRenderer)
    {
        _logger = logger;
        _applicationContext = applicationContext;
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

        _gpuModelMeshInstanceBuffer = GraphicsContext.CreateShaderStorageBuffer<GpuModelMeshInstance>("ModelMeshInstances");
        _gpuModelMeshInstanceBuffer.AllocateStorage(Marshal.SizeOf<GpuModelMeshInstance>() * 256, StorageAllocationFlags.Dynamic);

        _gpuCameraConstants.ViewProjection = _camera.ViewMatrix * _camera.ProjectionMatrix;
        _gpuCameraConstantsBuffer = GraphicsContext.CreateUniformBuffer<GpuCameraConstants>("CameraConstants");
        _gpuCameraConstantsBuffer.AllocateStorage(_gpuCameraConstants, StorageAllocationFlags.Dynamic);

        return true;
    }

    protected override void Render(float deltaTime)
    {
        GL.PushDebugGroup("Geometry-Pass");
        _gpuCameraConstants.ViewProjection = _camera.ViewMatrix * _camera.ProjectionMatrix;
        _gpuCameraConstantsBuffer.Update(_gpuCameraConstants, 0);
        _gpuModelMeshInstanceBuffer.Update(_gpuModelMeshInstances.ToArray(), 0);

        GraphicsContext.BeginRenderToFramebuffer(_gBufferFramebufferDescriptor);
        GraphicsContext.BindGraphicsPipeline(_gBufferGraphicsPipeline);
        _gBufferGraphicsPipeline.BindUniformBuffer(_gpuCameraConstantsBuffer, 0);
        _gBufferGraphicsPipeline.BindShaderStorageBuffer(_gpuModelMeshInstanceBuffer, 1);
        _gBufferGraphicsPipeline.BindShaderStorageBuffer(_gpuMaterialBuffer, 2);
        _gBufferGraphicsPipeline.BindVertexBuffer(_gpuVertexBuffer, 0, 0);
        _gBufferGraphicsPipeline.BindIndexBuffer(_gpuIndexBuffer);

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

        GraphicsContext.EndRender();
        GL.PopDebugGroup();

        GL.PushDebugGroup("Resolve-Pass");
        GraphicsContext.BeginRenderToFramebuffer(_finalFramebufferDescriptor);
        GraphicsContext.BindGraphicsPipeline(_finalGraphicsPipeline);
        _finalGraphicsPipeline.BindSampledTexture(_pointSampler, _gBufferBaseColorTexture, 0);
        _finalGraphicsPipeline.DrawArrays(3, 0);
        GraphicsContext.EndRender();
        GL.PopDebugGroup();

        GL.PushDebugGroup("UI-Pass");
        GraphicsContext.BeginRenderToSwapchain(_swapchainDescriptor);
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

                ImGui.SetCursorPos(new Num.Vector2(ImGui.GetWindowViewport().Size.X - 64, 0));
                ImGui.TextUnformatted($"Fps: {_metrics.AverageFrameTime}");

                ImGui.EndMenuBar();
                ImGui.EndMainMenuBar();
            }

            if (ImGui.Begin("Debug"))
            {
                var sensitivity = _camera.Sensitivity;
                ImGui.SliderFloat("Camera Sensitivity", ref sensitivity, 0.01f, 1.0f);
                _camera.Sensitivity = sensitivity;

                ImGui.Image((nint)_gBufferBaseColorTexture.Id, new Num.Vector2(320, 180), new Num.Vector2(0, 1), new Num.Vector2(1, 0));
                ImGui.Image((nint)_gBufferNormalTexture.Id, new Num.Vector2(320, 180), new Num.Vector2(0, 1), new Num.Vector2(1, 0));
                ImGui.Image((nint)_gBufferDepthTexture.Id, new Num.Vector2(320, 180), new Num.Vector2(0, 1), new Num.Vector2(1, 0));

                ImGui.End();
            }
        }
        UIRenderer.EndLayout();
        GraphicsContext.EndRender();
        GL.PopDebugGroup();
        GL.Finish();
    }

    protected override void Unload()
    {
        DestroyResolutionDependentResources();

        _pointSampler?.Dispose();

        base.Unload();
    }

    protected override void Update(float deltaTime)
    {
        base.Update(deltaTime);

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
            _camera.ProcessKeyboard(movement, 1 / 60.0f);
        }
    }

    protected override void WindowResized()
    {
        CreateSwapchainDescriptor();
    }

    private void PrepareScene()
    {
        var meshPrimitives = new List<MeshPrimitive>();

        _drawCommands.Add(new DrawCommand { Name = "Cube", WorldMatrix = Matrix.Translation(-4, 0, 0) });
        _drawCommands.Add(new DrawCommand { Name = "Cube.003", WorldMatrix = Matrix.Translation(4, 0, 0) });
        _drawCommands.Add(new DrawCommand { Name = "Cube.004", WorldMatrix = Matrix.Translation(0, 4, 0) });
        _drawCommands.Add(new DrawCommand { Name = "Cube.001", WorldMatrix = Matrix.Translation(0, 0, 0) });

        foreach (var drawCommand in _drawCommands)
        {
            var modelMesh = _deccerCubesModel.ModelMeshes.FirstOrDefault(m => m.Name == drawCommand.Name);

            if (!_gpuMaterialsInUse.Contains(modelMesh.MeshData.MaterialName))
            {
                var material = _materialLibrary.GetMaterialByName(modelMesh.MeshData.MaterialName);
                if (!_textures.TryGetValue(material.BaseColorImage.Name, out var texture))
                {
                    texture = material.BaseColorImage.ImageData.HasValue
                        ? GraphicsContext.CreateTextureFromMemory(material.BaseColorImage,
                            Format.R8G8B8A8Srgb,
                            material.BaseColorImage.Name, true)
                        : GraphicsContext.CreateTextureFromFile(material.BaseColorImage.FileName, Format.R8G8B8A8Srgb, true);
                    if (texture != null)
                    {
                        texture.MakeResident();
                        _textures.Add(material.BaseColorImage.Name, texture);
                    }
                }

                _gpuMaterials.Add(new GpuMaterial
                {
                    BaseColor = new Color4(0.1f, 0.1f, 0.1f, 1.0f),
                    BaseColorTexture = texture.TextureHandle
                });
                _gpuMaterialsInUse.Add(modelMesh.MeshData.MaterialName);
            }

            var materialIndex = _gpuMaterialsInUse.IndexOf(modelMesh.MeshData.MaterialName);
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
            drawCommand.IndexCount = meshPrimitive.IndexCount;
            drawCommand.IndexOffset = meshPrimitive.IndexOffset;
            drawCommand.VertexOffset = meshPrimitive.VertexOffset;
        }

        _gpuMaterialBuffer = GraphicsContext.CreateShaderStorageBuffer<GpuMaterial>("SceneMaterials");
        _gpuMaterialBuffer.AllocateStorage(Marshal.SizeOf<GpuMaterial>() * _gpuMaterials.Count, StorageAllocationFlags.Dynamic);
        _gpuMaterialBuffer.Update(_gpuMaterials.ToArray(), 0);

        var meshDatasAsArray = meshPrimitives.ToArray();

        _gpuVertexBuffer =
            GraphicsContext.CreateVertexBuffer("SceneVertices", meshDatasAsArray, VertexType.PositionNormalUvTangent);
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
            _applicationContext.ScaledFramebufferSize.Y, Format.R16G16B16A16Float, "Normals");
        _gBufferDepthTexture = GraphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.D32UNorm, "Depth");

        _gBufferFramebufferDescriptor = new FramebufferDescriptorBuilder()
            .WithColorAttachment(_gBufferBaseColorTexture, true, Vector4.Zero)
            .WithColorAttachment(_gBufferNormalTexture, true, Vector4.One)
            .WithDepthAttachment(_gBufferDepthTexture, true)
            .WithViewport(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y)
            .Build("GBuffer");

        _finalTexture = GraphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.R8G8B8A8UNorm, "Final");
        _finalFramebufferDescriptor = new FramebufferDescriptorBuilder()
            .WithColorAttachment(_finalTexture, true, new Vector4(0.2f, 0.2f, 0.2f, 1.0f))
            .WithViewport(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y)
            .Build("Final");
    }

    private void CreateSwapchainDescriptor()
    {
        _swapchainDescriptor = new SwapchainDescriptorBuilder()
            .WithViewport(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y)
            .Build();
    }

    private bool CreatePipelines()
    {
        var gBufferGraphicsPipelineResult = GraphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/Scene.vs.glsl", "Shaders/Scene.fs.glsl")
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 3, 0)
                .AddAttribute(0, DataType.Float, 3, 12)
                .AddAttribute(0, DataType.Float, 2, 24)
                .AddAttribute(0, DataType.Float, 4, 32)
                .Build("Scene"))
            .EnableCulling(CullMode.Back)
            .Build("GBuffer-Pipeline");
        if (gBufferGraphicsPipelineResult.IsFailure)
        {
            _logger.Error(gBufferGraphicsPipelineResult.Error);
            return false;
        }

        _gBufferGraphicsPipeline = gBufferGraphicsPipelineResult.Value;

        var finalGraphicsPipelineResult = GraphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/FST.vs.glsl", "Shaders/Texture.fs.glsl")
            .WithVertexInput(new VertexInputDescriptorBuilder()
                .AddAttribute(0, DataType.Float, 3, 0)
                .AddAttribute(0, DataType.Float, 2, 12)
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