using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Mathematics;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using EngineKit.UI;
using ImGuiNET;
using Microsoft.Extensions.Options;
using Serilog;
using MeshPrimitive = EngineKit.Graphics.MeshPrimitive;
using TextureInterpolationFilter = EngineKit.Graphics.TextureInterpolationFilter;

namespace CullOnGpu;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct SceneObject
{
    public Matrix4x4 WorldMatrix;
    
    public Vector3 AabbMin;
    public float _padding1;
    
    public Vector3 AabbMax;
    public float _padding2;

    public int IndexCount;
    public int IndexOffset;
    public int VertexOffset;
    public uint MaterialId;
}

internal sealed class CullOnGpuApplication : GraphicsApplication
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

    private ITexture? _swapchainColorBuffer;
    private FramebufferDescriptor _swapchainDescriptor;

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
    private IBuffer? _gpuCameraConstantsBuffer;

    private readonly List<GpuModelMeshInstance> _gpuModelMeshInstances;
    private IBuffer? _gpuModelMeshInstanceBuffer;
    private readonly List<DrawCommand> _drawCommands;

    private readonly List<GpuMaterial> _gpuMaterials;
    private readonly List<string> _gpuMaterialsInUse;
    private IBuffer? _gpuMaterialBuffer;

    private readonly Dictionary<string, ITexture> _textures;

    private IComputePipeline? _cullComputePipeline;
    private IBuffer? _culledDrawElementCommandsBuffer;
    private IBuffer? _sceneObjectBuffer;
    private IBuffer? _culledDrawCountBuffer;
    private IBuffer? _cullFrustumBuffer;
    private readonly List<SceneObject> _sceneObjects;

    private IGraphicsPipeline? _debugLinesGraphicsPipeline;
    private IBuffer? _cameraFrustumDebugLineBuffer;
    private IBuffer? _debugOriginalAabbBuffer;
    private BoundingFrustum _frozenFrustum;
    private Vector4[]? _frozenFrustumPlanes;
    private bool _isCameraFrustumFrozen;

    private bool _cullOnGpu;

    public CullOnGpuApplication(
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
        IMaterialLibrary materialLibrary)
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
        _metrics = metrics;
        _camera = camera;
        _camera.Mode = CameraMode.PerspectiveInfinity;
        _meshLoader = meshLoader;
        _materialLibrary = materialLibrary;

        _drawCommands = new List<DrawCommand>(256);
        _gpuModelMeshInstances = new List<GpuModelMeshInstance>(256);
        _gpuMaterials = new List<GpuMaterial>(256);
        _gpuMaterialsInUse = new List<string>(256);
        _textures = new Dictionary<string, ITexture>(16);
        _sceneObjects = new List<SceneObject>(256);
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

        if (!CreatePipelines())
        {
            return false;
        }

        LoadModels();
        
        _gpuModelMeshInstanceBuffer = GraphicsContext.CreateTypedBuffer<GpuModelMeshInstance>("ModelMeshInstances", 8 * 1024, BufferStorageFlags.DynamicStorage);

        _gpuCameraConstants.ViewProjection = _camera.ViewMatrix * _camera.ProjectionMatrix;
        _gpuCameraConstantsBuffer = GraphicsContext.CreateTypedBuffer<GpuCameraConstants>("CameraConstants", 1, BufferStorageFlags.DynamicStorage);

        _culledDrawCountBuffer = GraphicsContext.CreateTypedBuffer<uint>("DrawCount", 1, BufferStorageFlags.DynamicStorage);
        _culledDrawCountBuffer.UpdateElement(0, 0);

        _culledDrawElementCommandsBuffer = GraphicsContext.CreateTypedBuffer<DrawElementIndirectCommand>("CullDrawElements", 8 * 1024, BufferStorageFlags.DynamicStorage);
        _sceneObjectBuffer = GraphicsContext.CreateTypedBuffer<SceneObject>("SceneObjects", 8 * 1024, BufferStorageFlags.DynamicStorage);
        _cullFrustumBuffer = GraphicsContext.CreateTypedBuffer<Vector4>("FrustumPlanes", 6, BufferStorageFlags.DynamicStorage);

        _debugOriginalAabbBuffer = GraphicsContext.CreateTypedBuffer<VertexPositionColor>("DebugAabbRaw", 24 * 8 * 1024, BufferStorageFlags.DynamicStorage);

        SetCameraFrustumDebugLines(_camera);
        
        PrepareScene();
       
        _camera.ProcessMouseMovement();
        _frozenFrustum = new BoundingFrustum(_camera.ViewMatrix * _camera.ProjectionMatrix);
        _frozenFrustumPlanes = MakeFrustumPlanes(_camera.ViewMatrix * _camera.ProjectionMatrix);
        
        return true;
    }

    protected override void HandleDebugger(out bool breakOnError)
    {
        breakOnError = true;
    }

    protected override void Render(float deltaTime, float elapsedMilliseconds)
    {
        _gpuCameraConstants.ViewProjection = _camera.ViewMatrix * _camera.ProjectionMatrix;
        if (_cullOnGpu)
        {
            RenderComputeCull(_camera);
        }
        else
        {
            var cameraFrustum = new BoundingFrustum(_gpuCameraConstants.ViewProjection);
            if (!_isCameraFrustumFrozen)
            {
                _frozenFrustum = new BoundingFrustum(_gpuCameraConstants.ViewProjection);
                SetCameraFrustumDebugLines(_camera);
            }
            var boundingFrustum = _isCameraFrustumFrozen
                ? _frozenFrustum
                : cameraFrustum;
            
            var drawCommands = _sceneObjects
                //.Where(so => boundingFrustum.Intersects(new BoundingBox(so.AabbMin, so.AabbMax)))
                .Select(so => new DrawElementIndirectCommand
                {
                    BaseInstance = 1,
                    BaseVertex = (uint)so.VertexOffset,
                    FirstIndex = (uint)so.IndexOffset,
                    IndexCount = (uint)so.IndexCount,
                    InstanceCount = 1
                }).ToArray();
            _culledDrawElementCommandsBuffer!.UpdateElements(drawCommands, 0);
            var drawCommandCount = drawCommands.Length;
            _culledDrawCountBuffer!.UpdateElement(drawCommandCount, 0);

            var instances = _sceneObjects
                .Where(so => boundingFrustum.Intersects(new BoundingBox(so.AabbMin, so.AabbMax)))
                .Select(so => new GpuModelMeshInstance
                {
                    WorldMatrix = so.WorldMatrix,
                    MaterialId = new Int4((int)so.MaterialId, 0, 0, 0)
                })
                .ToArray();
            _gpuModelMeshInstanceBuffer!.UpdateElements(instances, 0);
        }
        
        _gpuCameraConstantsBuffer!.UpdateElement(_gpuCameraConstants, Offset.Zero);

        GraphicsContext.BeginRenderPass(_gBufferFramebufferDescriptor);
        
        // actual render into G-Buffer
        GraphicsContext.BindGraphicsPipeline(_gBufferGraphicsPipeline!);
        _gBufferGraphicsPipeline!.BindAsUniformBuffer(_gpuCameraConstantsBuffer, 0, Offset.Zero, SizeInBytes.Whole);
        _gBufferGraphicsPipeline.BindAsShaderStorageBuffer(_gpuModelMeshInstanceBuffer!, 1, Offset.Zero, SizeInBytes.Whole);
        _gBufferGraphicsPipeline.BindAsShaderStorageBuffer(_gpuMaterialBuffer!, 2, Offset.Zero, SizeInBytes.Whole);
        _gBufferGraphicsPipeline.BindAsShaderStorageBuffer(_gpuVertexBuffer!, 3, Offset.Zero, SizeInBytes.Whole);
        _gBufferGraphicsPipeline.BindAsIndexBuffer(_gpuIndexBuffer!);
        _gBufferGraphicsPipeline.MultiDrawElementsIndirectCount(
            _culledDrawElementCommandsBuffer!,
            _culledDrawCountBuffer!,
            (uint)_sceneObjects.Count);

        // debug lines - render camera frustum
        GraphicsContext.BindGraphicsPipeline(_debugLinesGraphicsPipeline!);
        _debugLinesGraphicsPipeline!.BindAsVertexBuffer(_cameraFrustumDebugLineBuffer!, 0, VertexPositionColor.Stride, 0);
        _debugLinesGraphicsPipeline.BindAsUniformBuffer(_gpuCameraConstantsBuffer, 0, Offset.Zero, SizeInBytes.Whole);
        _debugLinesGraphicsPipeline.DrawArrays(24, 0);
        // debug lines - render cube aabbs
        _debugLinesGraphicsPipeline.BindAsVertexBuffer(_debugOriginalAabbBuffer!, 0, VertexPositionColor.Stride, 0);
        _debugLinesGraphicsPipeline.DrawArrays(24 * (uint)_drawCommands.Count, 0);
        GraphicsContext.EndRenderPass();

        GraphicsContext.BeginRenderPass(_finalFramebufferDescriptor);
        GraphicsContext.BindGraphicsPipeline(_finalGraphicsPipeline!);
        _finalGraphicsPipeline!.BindSampledTexture(_pointSampler!, _gBufferBaseColorTexture!, 0);
        _finalGraphicsPipeline.DrawArrays(3, 0);
        GraphicsContext.EndRenderPass();

        GraphicsContext.BeginRenderPass(_swapchainDescriptor);
        GraphicsContext.BlitFramebufferToSwapchain(
            _finalFramebufferDescriptor,
            _swapchainDescriptor,
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
                if (ImGui.Button(IsWindowMaximized ? MaterialDesignIcons.WindowRestore : MaterialDesignIcons.WindowMaximize))
                {
                    if (IsWindowMaximized)
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

                ImGui.Checkbox("Cull on GPU", ref _cullOnGpu);
                ImGui.Checkbox("Freeze Camera Frustum", ref _isCameraFrustumFrozen);
                if (ImGui.Button("Freeze Frustum"))
                {
                    _frozenFrustum = new BoundingFrustum(_camera.ViewMatrix * _camera.ProjectionMatrix);
                    _frozenFrustumPlanes = MakeFrustumPlanes(_camera.ViewMatrix * _camera.ProjectionMatrix);
                    SetCameraFrustumDebugLines(_camera);
                }

                ImGui.Image((nint)_gBufferBaseColorTexture!.Id, new Vector2(320, 180), new Vector2(0, 1), new Vector2(1, 0));
                ImGui.Image((nint)_gBufferNormalTexture!.Id, new Vector2(320, 180), new Vector2(0, 1), new Vector2(1, 0));
                ImGui.Image((nint)_gBufferDepthTexture!.Id, new Vector2(320, 180), new Vector2(0, 1), new Vector2(1, 0));

                ImGui.End();
            }
        }
        UIRenderer.EndLayout();
        GraphicsContext.EndRenderPass();
        GraphicsContext.BlitFramebufferToSwapchain(
            _applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y,
            _applicationContext.FramebufferSize.X,
            _applicationContext.FramebufferSize.Y);
        
        if (_applicationContext.IsLaunchedByNSightGraphicsOnLinux)
        {
            GL.Finish();
        }
    }

    protected override void Unload()
    {
        DestroyResolutionDependentResources();

        _pointSampler?.Dispose();

        base.Unload();
    }

    protected override void Update(float deltaTime, float elapsedMilliseconds)
    {
        base.Update(deltaTime, elapsedMilliseconds);

        if (IsMousePressed(Glfw.MouseButton.ButtonRight))
        {
            _camera.ProcessMouseMovement();
        }

        if (IsKeyPressed(Glfw.Key.KeyEscape))
        {
            Close();
        }

        var movement = Vector3.Zero;
        var speedFactor = 100.0f;
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

    private void SetCameraFrustumDebugLines(ICamera camera)
    {
        var cameraFrustum = new BoundingFrustum(camera.ViewMatrix * camera.ProjectionMatrix);

        var nearColor = Colors.Orange.ToVector3();
        var farColor = Colors.DarkOrange.ToVector3();
        var bbCorners = cameraFrustum.GetCorners();

        var nearBottomRight = bbCorners[0];
        var nearTopRight = bbCorners[1];
        var nearTopLeft = bbCorners[2];
        var nearBottomLeft = bbCorners[3];

        var farBottomRight = bbCorners[4];
        var farTopRight = bbCorners[5];
        var farTopLeft = bbCorners[6];
        var farBottomLeft = bbCorners[7];

        var vertices = new[]
        {
            new VertexPositionColor(nearBottomRight, nearColor),
            new VertexPositionColor(nearTopRight, nearColor),

            new VertexPositionColor(nearTopRight, nearColor),
            new VertexPositionColor(nearTopLeft, nearColor),

            new VertexPositionColor(nearTopLeft, nearColor),
            new VertexPositionColor(nearBottomLeft, nearColor),

            new VertexPositionColor(nearBottomLeft, nearColor),
            new VertexPositionColor(nearBottomRight, nearColor),



            new VertexPositionColor(nearBottomRight, nearColor),
            new VertexPositionColor(farBottomRight, farColor),

            new VertexPositionColor(nearTopRight, nearColor),
            new VertexPositionColor(farTopRight, farColor),

            new VertexPositionColor(nearTopLeft, nearColor),
            new VertexPositionColor(farTopLeft, farColor),

            new VertexPositionColor(nearBottomLeft, nearColor),
            new VertexPositionColor(farBottomLeft, farColor),



            new VertexPositionColor(farBottomRight, farColor),
            new VertexPositionColor(farTopRight, farColor),

            new VertexPositionColor(farTopRight, farColor),
            new VertexPositionColor(farTopLeft, farColor),

            new VertexPositionColor(farTopLeft, farColor),
            new VertexPositionColor(farBottomLeft, farColor),

            new VertexPositionColor(farBottomLeft, farColor),
            new VertexPositionColor(farBottomRight, farColor)
        };
        
        _cameraFrustumDebugLineBuffer?.Dispose();
        _cameraFrustumDebugLineBuffer = GraphicsContext.CreateTypedBuffer<VertexPositionColor>("DebugLines", (uint)vertices.Length, BufferStorageFlags.DynamicStorage);
        _cameraFrustumDebugLineBuffer.UpdateElements(vertices, 0);
    }

    private void RenderComputeCull(ICamera camera)
    {
        /*
        var cameraBoundingFrustum = _isCameraFrustumFrozen
            ? _frozenFrustum
            : new BoundingFrustum(_camera.ViewMatrix * _camera.ProjectionMatrix);

        var bottomPlane = new Vector4(cameraBoundingFrustum.Bottom.Normal, cameraBoundingFrustum.Bottom.D);
        var topPlane = new Vector4(cameraBoundingFrustum.Top.Normal, cameraBoundingFrustum.Top.D);
        var leftPlane = new Vector4(cameraBoundingFrustum.Left.Normal, cameraBoundingFrustum.Left.D);
        var rightPlane = new Vector4(cameraBoundingFrustum.Right.Normal, cameraBoundingFrustum.Right.D);
        var nearPlane = new Vector4(cameraBoundingFrustum.Near.Normal, cameraBoundingFrustum.Near.D);
        var farPlane = new Vector4(cameraBoundingFrustum.Far.Normal, cameraBoundingFrustum.Far.D);

        var planes = new[] { bottomPlane, topPlane, leftPlane, rightPlane, nearPlane, farPlane };
        */
        var planes = MakeFrustumPlanes(_camera.ViewMatrix * camera.ProjectionMatrix);

        GraphicsContext.BindComputePipeline(_cullComputePipeline!);
        _cullFrustumBuffer!.UpdateElements(_isCameraFrustumFrozen ? _frozenFrustumPlanes! : planes, 0);

        _culledDrawElementCommandsBuffer!.ClearWith(new BufferClearInfo{Offset = 0, Size = SizeInBytes.Whole, Value = 0u});
        _culledDrawCountBuffer!.ClearAll();

        _cullComputePipeline!.BindAsShaderStorageBuffer(_sceneObjectBuffer!, 0, Offset.Zero, SizeInBytes.Whole);
        _cullComputePipeline.BindAsShaderStorageBuffer(_culledDrawElementCommandsBuffer, 1, Offset.Zero, SizeInBytes.Whole);
        _cullComputePipeline.BindAsShaderStorageBuffer(_cullFrustumBuffer, 2, Offset.Zero, SizeInBytes.Whole);
        _cullComputePipeline.BindAsShaderStorageBuffer(_culledDrawCountBuffer, 3, Offset.Zero, SizeInBytes.Whole);
        _cullComputePipeline.BindAsShaderStorageBuffer(_gpuModelMeshInstanceBuffer!, 4, Offset.Zero, SizeInBytes.Whole);
        _cullComputePipeline.BindAsUniformBuffer(_gpuCameraConstantsBuffer!, 5, Offset.Zero, SizeInBytes.Whole);
        //_cullComputePipeline.Uniform(0, false, _camera.ViewMatrix * camera.ProjectionMatrix);

        GraphicsContext.InsertMemoryBarrier(BarrierMask.BufferUpdate);
        _cullComputePipeline.Dispatch(((uint)_sceneObjects.Count + 31) / 32, 1, 1);
        GraphicsContext.InsertMemoryBarrier(BarrierMask.ShaderStorage | BarrierMask.Command);
    }

    private static VertexPositionColor[] CreateAabbLinesFromBoundingBox(BoundingBox boundingBox)
    {
        var nearColor = Colors.LimeGreen.ToVector3();
        var farColor = Colors.ForestGreen.ToVector3();
        var bbCorners = boundingBox.GetCorners();

        var nearBottomRight = bbCorners[0];
        var nearTopRight = bbCorners[1];
        var nearTopLeft = bbCorners[2];
        var nearBottomLeft = bbCorners[3];

        var farBottomRight = bbCorners[4];
        var farTopRight = bbCorners[5];
        var farTopLeft = bbCorners[6];
        var farBottomLeft = bbCorners[7];

        var vertices = new[]
        {
            new VertexPositionColor(nearBottomRight, nearColor),
            new VertexPositionColor(nearTopRight, nearColor),

            new VertexPositionColor(nearTopRight, nearColor),
            new VertexPositionColor(nearTopLeft, nearColor),

            new VertexPositionColor(nearTopLeft, nearColor),
            new VertexPositionColor(nearBottomLeft, nearColor),

            new VertexPositionColor(nearBottomLeft, nearColor),
            new VertexPositionColor(nearBottomRight, nearColor),



            new VertexPositionColor(nearBottomRight, nearColor),
            new VertexPositionColor(farBottomRight, farColor),

            new VertexPositionColor(nearTopRight, nearColor),
            new VertexPositionColor(farTopRight, farColor),

            new VertexPositionColor(nearTopLeft, nearColor),
            new VertexPositionColor(farTopLeft, farColor),

            new VertexPositionColor(nearBottomLeft, nearColor),
            new VertexPositionColor(farBottomLeft, farColor),



            new VertexPositionColor(farBottomRight, farColor),
            new VertexPositionColor(farTopRight, farColor),

            new VertexPositionColor(farTopRight, farColor),
            new VertexPositionColor(farTopLeft, farColor),

            new VertexPositionColor(farTopLeft, farColor),
            new VertexPositionColor(farBottomLeft, farColor),

            new VertexPositionColor(farBottomLeft, farColor),
            new VertexPositionColor(farBottomRight, farColor)
        };
        
        return vertices;
    }

    private static Vector4[] MakeFrustumPlanes(Matrix4x4 viewProj)
    {
        var planes = new Vector4[6];
        for (var i = 0; i < 4; ++i)
        {
            planes[0][i] = viewProj[i, 3] + viewProj[i, 0];
        }
        for (var i = 0; i < 4; ++i)
        {
            planes[1][i] = viewProj[i, 3] - viewProj[i, 0];
        }
        for (var i = 0; i < 4; ++i)
        {
            planes[2][i] = viewProj[i, 3] + viewProj[i, 1];
        }
        for (var i = 0; i < 4; ++i)
        {
            planes[3][i] = viewProj[i, 3] - viewProj[i, 1];
        }
        for (var i = 0; i < 4; ++i)
        {
            planes[4][i] = viewProj[i, 3] + viewProj[i, 2];
        }
        for (var i = 0; i < 4; ++i)
        {
            planes[5][i] = viewProj[i, 3] - viewProj[i, 2];
        }
        for (var i = 0; i < planes.Length; i++)
        {
            planes[i] = Vector4.Divide(planes[i], new Vector3(planes[i].X, planes[i].Y, planes[i].Z).Length());
            planes[i].W = -planes[i].W;
        }
        return planes;
    }

    private void PrepareScene()
    {
        var meshPrimitives = new List<MeshPrimitive>();
        _sceneObjects.Clear();

        var cubes = new[]
        {
            "Cube",
            "Cube.003",
            "Cube.004",
            "Cube.001"
        };

        var range = Enumerable.Range(0, 1024);
        foreach (var i in range)
        {
            var position = new Vector3(-100.0f + 200 * Random.Shared.NextSingle(), -100.0f + 200 * Random.Shared.NextSingle(), -100.0f + 200 * Random.Shared.NextSingle());
            var randomCube = cubes[Random.Shared.Next(0, cubes.Length)];
            
            _drawCommands.Add(new DrawCommand
            {
                Name = randomCube,
                WorldMatrix = Matrix4x4.CreateTranslation(position)
            });
        }

        var debugOriginalAabbLines = new List<VertexPositionColor>(24 * _drawCommands.Count);
        foreach (var drawCommand in _drawCommands)
        {
            var modelMesh = _deccerCubesModel!.ModelMeshes.FirstOrDefault(m => m.Name == drawCommand.Name);
            var meshDataBoundingBox = modelMesh!.MeshData.BoundingBox;
            meshDataBoundingBox.Max = Vector3.Transform(meshDataBoundingBox.Max, drawCommand.WorldMatrix);
            meshDataBoundingBox.Min = Vector3.Transform(meshDataBoundingBox.Min, drawCommand.WorldMatrix);
            var lines = CreateAabbLinesFromBoundingBox(meshDataBoundingBox);
            debugOriginalAabbLines.AddRange(lines);

            if (!_gpuMaterialsInUse.Contains(modelMesh.MeshData.MaterialName!))
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
                        if (!_applicationContext.IsLaunchedByRenderDoc)
                        {
                            texture.MakeResident();
                        }
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

            drawCommand.MaterialIndex = materialIndex;
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
            
            var primitiveBoundingBox = meshPrimitive.BoundingBox;
            //*
            //primitiveBoundingBox.Max = Vector3.Transform(primitiveBoundingBox.Max, drawCommand.WorldMatrix);
            //primitiveBoundingBox.Min = Vector3.Transform(primitiveBoundingBox.Min, drawCommand.WorldMatrix);
            //*/
            
            _sceneObjects.Add(new SceneObject
            {
                WorldMatrix = drawCommand.WorldMatrix,
                AabbMin = primitiveBoundingBox.Min,
                AabbMax =  primitiveBoundingBox.Max,
                IndexCount = drawCommand.IndexCount,
                IndexOffset = drawCommand.IndexOffset,
                VertexOffset = drawCommand.VertexOffset,
                MaterialId = (uint)drawCommand.MaterialIndex
            });
        }

        var debugOriginalAabbLinesArray = debugOriginalAabbLines.ToArray();
        _debugOriginalAabbBuffer!.UpdateElements(debugOriginalAabbLinesArray, 0);
        var sceneObjectsArray = _sceneObjects.ToArray();
        _sceneObjectBuffer!.UpdateElements(sceneObjectsArray, 0);

        var gpuMaterialsArray = _gpuMaterials.ToArray();
        _gpuMaterialBuffer = GraphicsContext.CreateTypedBuffer("SceneMaterials", gpuMaterialsArray, BufferStorageFlags.DynamicStorage);

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
        
        _cullFrustumBuffer?.Dispose();
        _culledDrawCountBuffer?.Dispose();
        _culledDrawElementCommandsBuffer?.Dispose();
        _sceneObjectBuffer?.Dispose();
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
            .WithColorAttachment(_gBufferBaseColorTexture, true, MathHelper.GammaToLinear(Colors.DarkSlateBlue))
            .WithColorAttachment(_gBufferNormalTexture, true, Vector4.Zero)
            .WithDepthAttachment(_gBufferDepthTexture, true, 0.0f)
            .WithViewport(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y)
            .Build("GBuffer");

        _finalTexture = GraphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.R8G8B8A8UNorm, "Final");
        _finalFramebufferDescriptor = GraphicsContext.GetFramebufferDescriptorBuilder()
            .WithColorAttachment(_finalTexture, true, Colors.Transparent)
            .WithViewport(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y)
            .Build("Final");

        _swapchainColorBuffer = GraphicsContext.CreateTexture2D(
            _applicationContext.FramebufferSize.X,
            _applicationContext.FramebufferSize.Y,
            Format.R8G8B8Srgb,
            "SwapchainColor");
        _swapchainDescriptor = GraphicsContext.GetFramebufferDescriptorBuilder()
            .WithColorAttachment(_swapchainColorBuffer, false, Colors.Transparent)
            .WithViewport(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y)
            .Build("Swapchain");
    }

    private bool CreatePipelines()
    {
        var gBufferGraphicsPipelineResult = GraphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/SceneVertexPulling.vs.glsl", "Shaders/Scene.fs.glsl")
            .WithCullingEnabled(CullMode.Back)
            .WithDepthTestEnabled(CompareFunction.Greater)
            .WithClipControlDepth(ClipControlDepth.ZeroToOne)
            .WithDepthClampEnabled()
            .ClearResourceBindingsOnBind()
            .Build("GBuffer-Pipeline");
        if (gBufferGraphicsPipelineResult.IsFailure)
        {
            _logger.Error(gBufferGraphicsPipelineResult.Error);
            return false;
        }

        _gBufferGraphicsPipeline = gBufferGraphicsPipelineResult.Value;

        var finalGraphicsPipelineResult = GraphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/FST.vs.glsl", "Shaders/Texture.fs.glsl")
            .WithVertexAttributesFromDescriptor(new VertexInputDescriptorBuilder()
                .AddAttribute(0, Format.R32G32B32Float, 0)
                .AddAttribute(0, Format.R32G32Float, 12)
                .Build("FST"))
            .ClearResourceBindingsOnBind()
            .WithDepthTestEnabled(CompareFunction.Less)
            .WithClipControlDepth(ClipControlDepth.NegativeOneToOne)
            .Build("Final-Pipeline");
        if (finalGraphicsPipelineResult.IsFailure)
        {
            _logger.Error(finalGraphicsPipelineResult.Error);
            return false;
        }

        _finalGraphicsPipeline = finalGraphicsPipelineResult.Value;

        var cullComputePipelineResult = GraphicsContext.CreateComputePipelineBuilder()
            .WithShaderFromFile("Shaders/Cull.cs.glsl")
            .Build("Cull-By-Aabb");
        if (cullComputePipelineResult.IsFailure)
        {
            _logger.Error(cullComputePipelineResult.Error);
            return false;
        }

        _cullComputePipeline = cullComputePipelineResult.Value;

        var debugLineGraphicsPipelineResult = GraphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/Line.vs.glsl", "Shaders/Line.fs.glsl")
            .WithVertexAttributesFromDescriptor(new VertexInputDescriptorBuilder()
                .AddAttribute(0, Format.R32G32B32Float, 0)
                .AddAttribute(0, Format.R32G32B32Float, 12)
                .Build(nameof(VertexPositionColor)))
            .WithTopology(PrimitiveTopology.Lines)
            .WithFaceWinding(FaceWinding.Clockwise)
            .WithClipControlDepth(ClipControlDepth.ZeroToOne)
            .WithDepthTestEnabled(CompareFunction.Greater)
            .WithBlendingEnabled(ColorBlendAttachmentDescriptor.PreMultiplied)
            .ClearResourceBindingsOnBind()
            .Build("Debug-Lines");

        if (debugLineGraphicsPipelineResult.IsFailure)
        {
            _logger.Error(debugLineGraphicsPipelineResult.Error);
            return false;
        }

        _debugLinesGraphicsPipeline = debugLineGraphicsPipelineResult.Value;

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