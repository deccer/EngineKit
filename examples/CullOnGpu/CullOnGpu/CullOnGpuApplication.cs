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

    private Model _deccerCubesModel;

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

    private IList<GpuModelMeshInstance> _gpuModelMeshInstances;
    private IBuffer? _gpuModelMeshInstanceBuffer;
    private IList<DrawCommand> _drawCommands;

    private IList<GpuMaterial> _gpuMaterials;
    private IList<string> _gpuMaterialsInUse;
    private IBuffer _gpuMaterialBuffer;

    private IDictionary<string, ITexture> _textures;

    private IComputePipeline? _cullComputePipeline;
    private IBuffer? _culledDrawElementCommandsBuffer;
    private IBuffer? _sceneObjectBuffer;
    private IBuffer? _culledDrawCountBuffer;
    private IBuffer? _cullFrustumBuffer;
    private IList<SceneObject> _sceneObjects;

    private IGraphicsPipeline? _debugLinesGraphicsPipeline;
    private IBuffer? _debugLinesBuffer;
    private IBuffer? _debugOriginalAabbBuffer;
    private BoundingFrustum _frozenFrustum;
    private Vector4[] _frozenFrustumPlanes;
    private bool _isCameraFrustumFrozen;

    private bool _cullOnGpu;

    public CullOnGpuApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        ICapabilities capabilities,
        IMetrics metrics,
        ILimits limits,
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
            limits,
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
        CreateSwapchainDescriptor();

        if (!CreatePipelines())
        {
            return false;
        }

        LoadModels();
        
        _gpuModelMeshInstanceBuffer = GraphicsContext.CreateShaderStorageBuffer<GpuModelMeshInstance>("ModelMeshInstances");
        _gpuModelMeshInstanceBuffer.AllocateStorage(Marshal.SizeOf<GpuModelMeshInstance>() * 20000, StorageAllocationFlags.Dynamic);

        _gpuCameraConstants.ViewProjection = _camera.ViewMatrix * _camera.ProjectionMatrix;
        _gpuCameraConstantsBuffer = GraphicsContext.CreateUniformBuffer<GpuCameraConstants>("CameraConstants");
        _gpuCameraConstantsBuffer.AllocateStorage(_gpuCameraConstants, StorageAllocationFlags.Dynamic);

        _culledDrawCountBuffer = GraphicsContext.CreateShaderStorageBuffer<uint>("DrawCount");
        _culledDrawCountBuffer.AllocateStorage<uint>(0, StorageAllocationFlags.Dynamic);

        _culledDrawElementCommandsBuffer = GraphicsContext.CreateShaderStorageBuffer<DrawElementIndirectCommand>("CullDrawElements");
        _culledDrawElementCommandsBuffer.AllocateStorage(64 * 1024 * Marshal.SizeOf<DrawElementIndirectCommand>(), StorageAllocationFlags.Dynamic);

        _sceneObjectBuffer = GraphicsContext.CreateShaderStorageBuffer<SceneObject>("SceneObjects");
        _sceneObjectBuffer.AllocateStorage(64 * 1024 * Marshal.SizeOf<SceneObject>(), StorageAllocationFlags.Dynamic);

        _cullFrustumBuffer = GraphicsContext.CreateShaderStorageBuffer<Vector4>("FrustumPlanes");
        _cullFrustumBuffer.AllocateStorage(6 * Marshal.SizeOf<Vector4>(), StorageAllocationFlags.Dynamic);

        _debugOriginalAabbBuffer = GraphicsContext.CreateVertexBuffer<VertexPositionColor>("DebugAabbRaw");
        _debugOriginalAabbBuffer.AllocateStorage(24 * 20000 * Marshal.SizeOf<VertexPositionColor>(), StorageAllocationFlags.Dynamic);

        SetDebugLines(_camera);
        
        PrepareScene();
       
        _camera.ProcessMouseMovement();
        _frozenFrustum = new BoundingFrustum(_camera.ViewMatrix * _camera.ProjectionMatrix);
        _frozenFrustumPlanes = MakeFrustumPlanes(_camera.ViewMatrix * _camera.ProjectionMatrix);
        
        return true;
    }

    protected override void Render(float deltaTime)
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
                SetDebugLines(_camera);
            }
            BoundingFrustum boundingFrustum = _isCameraFrustumFrozen
                ? _frozenFrustum
                : cameraFrustum;
            
            var drawCommands = _sceneObjects
                .Where(so => boundingFrustum.Intersects(new BoundingBox(so.AabbMin, so.AabbMax)))
                .Select(so => new DrawElementIndirectCommand
                {
                    BaseInstance = 1,
                    BaseVertex = so.VertexOffset,
                    FirstIndex = (uint)so.IndexOffset,
                    IndexCount = (uint)so.IndexCount,
                    InstanceCount = 1
                }).ToArray();
            _culledDrawElementCommandsBuffer.Update(ref drawCommands, 0);
            var drawCommandCount = drawCommands.Length;
            _culledDrawCountBuffer.Update(ref drawCommandCount);

            var instances = _sceneObjects
                .Where(so => boundingFrustum.Intersects(new BoundingBox(so.AabbMin, so.AabbMax)))
                .Select(so => new GpuModelMeshInstance
                {
                    WorldMatrix = so.WorldMatrix,
                    MaterialId = new Int4((int)so.MaterialId, 0, 0, 0)
                })
                .ToArray();
            _gpuModelMeshInstanceBuffer.Update(ref instances, 0);
        }
        
        _gpuCameraConstantsBuffer.Update(ref _gpuCameraConstants, Offset.Zero);

        GraphicsContext.BeginRenderPass(_gBufferFramebufferDescriptor);
        
        // actual render into gbuffer
        GraphicsContext.BindGraphicsPipeline(_gBufferGraphicsPipeline);
        _gBufferGraphicsPipeline.BindAsUniformBuffer(_gpuCameraConstantsBuffer, 0);
        _gBufferGraphicsPipeline.BindAsShaderStorageBuffer(_gpuModelMeshInstanceBuffer, 1);
        _gBufferGraphicsPipeline.BindAsShaderStorageBuffer(_gpuMaterialBuffer, 2);
        _gBufferGraphicsPipeline.BindAsShaderStorageBuffer(_gpuVertexBuffer, 3);
        _gBufferGraphicsPipeline.BindAsIndexBuffer(_gpuIndexBuffer);
        _gBufferGraphicsPipeline.MultiDrawElementsIndirectCount(
            _culledDrawElementCommandsBuffer,
            _culledDrawCountBuffer,
            _sceneObjects.Count);

        // debug lines - render camera frustum
        GraphicsContext.BindGraphicsPipeline(_debugLinesGraphicsPipeline);
        _debugLinesGraphicsPipeline.BindAsVertexBuffer(_debugLinesBuffer, 0, 0);
        _debugLinesGraphicsPipeline.BindAsUniformBuffer(_gpuCameraConstantsBuffer, 0);
        _debugLinesGraphicsPipeline.DrawArrays((uint)_debugLinesBuffer.Count, 0);
        // debug lines - render cube aabbs
        _debugLinesGraphicsPipeline.BindAsVertexBuffer(_debugOriginalAabbBuffer, 0, 0);
        _debugLinesGraphicsPipeline.DrawArrays((uint)_debugOriginalAabbBuffer.Count, 0);
        GraphicsContext.EndRenderPass();

        GraphicsContext.BeginRenderPass(_finalFramebufferDescriptor);
        GraphicsContext.BindGraphicsPipeline(_finalGraphicsPipeline);
        _finalGraphicsPipeline.BindSampledTexture(_pointSampler, _gBufferBaseColorTexture, 0);
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

                ImGui.SetCursorPos(new Vector2(ImGui.GetWindowViewport().Size.X - 64, 0));
                ImGui.TextUnformatted($"Fps: {_metrics.AverageFrameTime}");

                ImGui.EndMenuBar();
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
                    SetDebugLines(_camera);
                }

                ImGui.Image((nint)_gBufferBaseColorTexture.Id, new Vector2(320, 180), new Vector2(0, 1), new Vector2(1, 0));
                ImGui.Image((nint)_gBufferNormalTexture.Id, new Vector2(320, 180), new Vector2(0, 1), new Vector2(1, 0));
                ImGui.Image((nint)_gBufferDepthTexture.Id, new Vector2(320, 180), new Vector2(0, 1), new Vector2(1, 0));

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
        
        if (_capabilities.IsLaunchedByNSightGraphicsOnLinux)
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

    protected override void WindowResized()
    {
        CreateSwapchainDescriptor();
    }

    private void SetDebugLines(ICamera camera)
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
        
        _debugLinesBuffer?.Dispose();
        _debugLinesBuffer = GraphicsContext.CreateVertexBuffer<VertexPositionColor>("DebugLines");
        unsafe
        {
            var sizeInBytes = sizeof(VertexPositionColor) * vertices.Length;
            _debugLinesBuffer.AllocateStorage(sizeInBytes, StorageAllocationFlags.Dynamic);
        }

        _debugLinesBuffer.Update(ref vertices);
    }

    private void RenderComputeCull(ICamera camera)
    {
        var cameraBoundingFrustum = _isCameraFrustumFrozen
            ? _frozenFrustum
            : new BoundingFrustum(_camera.ViewMatrix * _camera.ProjectionMatrix);
        var bottomPlane = new Vector4(cameraBoundingFrustum.Bottom.Normal, cameraBoundingFrustum.Bottom.D);
        var topPlane = new Vector4(cameraBoundingFrustum.Top.Normal, cameraBoundingFrustum.Top.D);
        var leftPlane = new Vector4(cameraBoundingFrustum.Left.Normal, cameraBoundingFrustum.Left.D);
        var rightPlane = new Vector4(cameraBoundingFrustum.Right.Normal, cameraBoundingFrustum.Right.D);
        var nearPlane = new Vector4(cameraBoundingFrustum.Near.Normal, cameraBoundingFrustum.Near.D);
        var farPlane = new Vector4(cameraBoundingFrustum.Far.Normal, cameraBoundingFrustum.Far.D);

        //var planes = new[] { bottomPlane, topPlane, leftPlane, rightPlane, nearPlane, farPlane };
        var planes = MakeFrustumPlanes(_camera.ViewMatrix * camera.ProjectionMatrix);

        GraphicsContext.BindComputePipeline(_cullComputePipeline);
        if (_isCameraFrustumFrozen)
        {
            _cullFrustumBuffer.Update(ref _frozenFrustumPlanes);            
        }
        else
        {
            _cullFrustumBuffer.Update(ref planes);            
        }

        _culledDrawElementCommandsBuffer.ClearWith(new BufferClearInfo{Offset = 0, Size = SizeInBytes.Whole, Data = 0u});
        _culledDrawCountBuffer.ClearAll();

        _cullComputePipeline.BindAsShaderStorageBuffer(_sceneObjectBuffer, 0);
        _cullComputePipeline.BindAsShaderStorageBuffer(_culledDrawElementCommandsBuffer, 1);
        _cullComputePipeline.BindAsShaderStorageBuffer(_cullFrustumBuffer, 2);
        _cullComputePipeline.BindAsShaderStorageBuffer(_culledDrawCountBuffer, 3);
        _cullComputePipeline.BindAsShaderStorageBuffer(_gpuModelMeshInstanceBuffer, 4);
        _cullComputePipeline.BindAsUniformBuffer(_gpuCameraConstantsBuffer, 5);
        //_cullComputePipeline.Uniform(0, false, _camera.ViewMatrix * camera.ProjectionMatrix);

        GraphicsContext.InsertMemoryBarrier(BarrierMask.BufferUpdate);
        _cullComputePipeline.Dispatch(((uint)_sceneObjects.Count + 31) / 32, 1, 1);
        GraphicsContext.InsertMemoryBarrier(BarrierMask.ShaderStorage | BarrierMask.Command);
    }

    private VertexPositionColor[] CreateAabbLinesFromBoundingBox(BoundingBox boundingBox)
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

    private Vector4[] MakeFrustumPlanes(Matrix4x4 viewProj)
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

        var range = Enumerable.Range(0, 20000);
        foreach (var i in range)
        {
            var position = new Vector3(-200.0f + 400 * Random.Shared.NextSingle(), -200.0f + 400 * Random.Shared.NextSingle(), -200.0f + 400 * Random.Shared.NextSingle());
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
            var modelMesh = _deccerCubesModel.ModelMeshes.FirstOrDefault(m => m.Name == drawCommand.Name);
            var modelMeshBB = modelMesh.MeshData.BoundingBox;
            modelMeshBB.Max = Vector3.Transform(modelMeshBB.Max, drawCommand.WorldMatrix);
            modelMeshBB.Min = Vector3.Transform(modelMeshBB.Min, drawCommand.WorldMatrix);
            var lines = CreateAabbLinesFromBoundingBox(modelMeshBB);
            debugOriginalAabbLines.AddRange(lines);

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
                        if (!_capabilities.IsLaunchedByRenderDoc)
                        {
                            texture.MakeResident();
                        }
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
            drawCommand.IndexCount = meshPrimitive.IndexCount;
            drawCommand.IndexOffset = meshPrimitive.IndexOffset;
            drawCommand.VertexOffset = meshPrimitive.VertexOffset;
            
            var modelMeshBB = meshPrimitive.BoundingBox;
            ///*
            //modelMeshBB.Max = Vector3.Transform(modelMeshBB.Max, drawCommand.WorldMatrix);
            //modelMeshBB.Min = Vector3.Transform(modelMeshBB.Min, drawCommand.WorldMatrix);
            //*/
            
            _sceneObjects.Add(new SceneObject
            {
                WorldMatrix = drawCommand.WorldMatrix,
                AabbMin = modelMeshBB.Min,
                AabbMax =  modelMeshBB.Max,
                IndexCount = drawCommand.IndexCount,
                IndexOffset = drawCommand.IndexOffset,
                VertexOffset = drawCommand.VertexOffset,
                MaterialId = (uint)drawCommand.MaterialIndex
            });
        }

        var debugOriginalAabbLinesArray = debugOriginalAabbLines.ToArray();
        _debugOriginalAabbBuffer.Update(ref debugOriginalAabbLinesArray);
        var sceneObjectsArray = _sceneObjects.ToArray();
        _sceneObjectBuffer.Update(ref sceneObjectsArray, 0);

        var gpuMaterialsArray = _gpuMaterials.ToArray();
        _gpuMaterialBuffer = GraphicsContext.CreateShaderStorageBuffer<GpuMaterial>("SceneMaterials");
        _gpuMaterialBuffer.AllocateStorage(Marshal.SizeOf<GpuMaterial>() * _gpuMaterials.Count, StorageAllocationFlags.Dynamic);
        _gpuMaterialBuffer.Update(ref gpuMaterialsArray, 0);

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

        _gBufferFramebufferDescriptor = new FramebufferDescriptorBuilder()
            .WithColorAttachment(_gBufferBaseColorTexture, true, Vector4.Zero)
            .WithColorAttachment(_gBufferNormalTexture, true, Vector4.Zero)
            .WithDepthAttachment(_gBufferDepthTexture, true, 0.0f)
            .WithViewport(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y)
            .Build("GBuffer");

        _finalTexture = GraphicsContext.CreateTexture2D(_applicationContext.ScaledFramebufferSize.X,
            _applicationContext.ScaledFramebufferSize.Y, Format.R8G8B8A8UNorm, "Final");
        _finalFramebufferDescriptor = new FramebufferDescriptorBuilder()
            .WithColorAttachment(_finalTexture, true, new Vector4(0.2f, 0.2f, 0.2f, 1.0f))
            .WithViewport(_applicationContext.ScaledFramebufferSize.X, _applicationContext.ScaledFramebufferSize.Y)
            .Build("Final");

        _swapchainColorBuffer = GraphicsContext.CreateTexture2D(
            _applicationContext.FramebufferSize.X,
            _applicationContext.FramebufferSize.Y,
            Format.R8G8B8Srgb,
            "SwapchainColor");
        _swapchainDescriptor = new FramebufferDescriptorBuilder()
            .WithColorAttachment(_swapchainColorBuffer, true, new Vector4(0.2f, 0.2f, 0.2f, 1.0f))
            .WithViewport(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y)
            .Build("Swapchain");
    }

    private void CreateSwapchainDescriptor()
    {
        /*
        _swapchainDescriptor = new SwapchainDescriptorBuilder()
            .WithViewport(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y)
            .ClearDepth(0.0f)
            .Build("Swapchain");
            */
    }

    private bool CreatePipelines()
    {
        var gBufferGraphicsPipelineResult = GraphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/SceneVertexPulling.vs.glsl", "Shaders/Scene.fs.glsl")
            .WithVertexAttributesFromDescriptor(new VertexInputDescriptorBuilder()
                .AddAttribute(0, Format.R32G32B32Float, 0)
                .AddAttribute(0, Format.R32G32B32Float, 12)
                .AddAttribute(0, Format.R32G32Float, 24)
                .AddAttribute(0, Format.R32G32B32A32Float, 32)
                .Build("Scene"))
            .WithCullingEnabled(CullMode.Back)
            .WithDepthTestEnabled(CompareFunction.Greater)
            .WithClipControlDepth(ClipControlDepth.NegativeOneToOne)
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