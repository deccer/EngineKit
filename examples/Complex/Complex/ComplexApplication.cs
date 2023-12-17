using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BepuUtilities;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Mathematics;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using ImGuiNET;
using Microsoft.Extensions.Options;
using Serilog;
using MathHelper = EngineKit.Mathematics.MathHelper;

namespace Complex;

public struct CameraInformation
{
    public Matrix4x4 ProjectionMatrix;
    public Matrix4x4 ViewMatrix;
}

public struct InstanceInformation
{
    public Matrix4x4 WorldMatrix;
}

internal sealed class ComplexApplication : GraphicsApplication
{
    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly ICapabilities _capabilities;
    private readonly IMetrics _metrics;
    private readonly IModelLibrary _modelLibrary;
    private readonly IAssetLoader _assetLoader;
    private readonly ICamera _camera;
    private readonly IScene _scene;

    private SwapchainDescriptor _swapchainDescriptor;
    private IGraphicsPipeline _graphicsPipeline;

    private IBuffer? _vertexBuffer;
    private IBuffer? _indexBuffer;
    private IBuffer? _drawIndirectBuffer;
    
    private CameraInformation _cameraInformation;
    private IBuffer? _cameraInformationBuffer;
    private IList<InstanceInformation> _instanceInformation;
    private IBuffer? _instanceInformationBuffer;
    
    private int _meshIdCount;

    public ComplexApplication(
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
        IModelLibrary modelLibrary,
        IAssetLoader assetLoader,
        ICamera camera,
        IScene scene)
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
        _modelLibrary = modelLibrary;
        _assetLoader = assetLoader;
        _camera = camera;
        _camera.Sensitivity = 0.125f;
        _camera.Mode = CameraMode.PerspectiveInfinity;
        _scene = scene;

        _instanceInformation = new List<InstanceInformation>();
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

        //_assetLoader.ImportAsset("SM_Mesh", "Data/Props/deccer-cubes/SM_Deccer_Cubes_Textured_Complex.gltf");
        //_assetLoader.ImportAsset("SM_Mesh", "Data/Props/Scene/Hierarchy.gltf");
        _modelLibrary.AddModelFromFile("SM_Model", "Data/Props/deccer-cubes/SM_Deccer_Cubes_Textured_Complex.gltf");

        _swapchainDescriptor = new SwapchainDescriptorBuilder()
            .ClearColor(Colors.ForestGreen)
            .ClearDepth(0.0f)
            .EnableSrgb()
            .WithViewport(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y)
            .Build("Swapchain");

        var graphicsPipelineResult = GraphicsContext.CreateGraphicsPipelineBuilder()
            .WithShadersFromFiles("Shaders/Simple.vs.glsl", "Shaders/Simple.fs.glsl")
            .WithTopology(PrimitiveTopology.Triangles)
            .WithVertexAttributesFromVertexType(VertexType.PositionNormalUvTangent)
            .WithDepthTestEnabled(CompareFunction.Greater)
            .WithClipControlDepth(ClipControlDepth.ZeroToOne)
            .Build("GraphicsPipeline");

        if (graphicsPipelineResult.IsFailure)
        {
            _logger.Error("{Category} Unable to create. {Error}", "GraphicsPipeline", graphicsPipelineResult.Error);
            return false;
        }

        _graphicsPipeline = graphicsPipelineResult.Value;

        _cameraInformationBuffer = GraphicsContext.CreateUniformBuffer<CameraInformation>("Camera");
        _cameraInformationBuffer.AllocateStorage(_cameraInformation, StorageAllocationFlags.Dynamic);

        _vertexBuffer = _assetLoader.GetVertexBuffer();
        _indexBuffer = _assetLoader.GetIndexBuffer();

        _scene.AddEntityWithModelRenderer(null, _modelLibrary.GetModelByName("SM_Model"), Vector3.Zero);        

        /*
        var meshNames = _assetLoader.GetMeshPrimitiveNamesByAssetName("SM_Mesh");
        var meshIdAndTransforms = meshNames
            .Select(meshName => _assetLoader.GetMeshIdByMeshPrimitiveName(meshName))
            .Where(meshId => meshId.HasValue)
            .Select(meshId => meshId.Value)
            .ToList();
            
        var meshIndirectElements = meshIdAndTransforms            
            .Select(meshId => new DrawElementIndirectCommand
            {
                BaseInstance = 0,
                BaseVertex = meshId.MeshId.VertexOffset,
                FirstIndex = meshId.MeshId.IndexOffset,
                IndexCount = meshId.MeshId.IndexCount,
                InstanceCount = 1
            })
            .ToArray();
        _meshIdCount = meshIndirectElements.Length;
        
        _drawIndirectBuffer = GraphicsContext.CreateDrawIndirectBuffer("VisibleObjects");
        _drawIndirectBuffer.AllocateStorage(meshIndirectElements, StorageAllocationFlags.Dynamic);

        _instanceInformation = meshIdAndTransforms
            .Select(meshIdAndTransform => new InstanceInformation
            {
                WorldMatrix = meshIdAndTransform.Transform
            }).ToList();
        _instanceInformationBuffer = GraphicsContext.CreateShaderStorageBuffer<InstanceInformation>("Instances");
        _instanceInformationBuffer.AllocateStorage(_instanceInformation.ToArray(), StorageAllocationFlags.Dynamic);
        */
        
        return true;
    }

    private float _angle;
    private bool _firstFrame = true;
    private Matrix4x4 _initialTransform;
    private Vector3 _initialScale;
    private Quaternion _initialRotation;
    private Vector3 _initialPosition;

    protected override void Render(float deltaTime)
    {
        _cameraInformation.ProjectionMatrix = _camera.ProjectionMatrix;
        _cameraInformation.ViewMatrix = _camera.ViewMatrix;
        _cameraInformationBuffer.Update(ref _cameraInformation);

        if (_firstFrame)
        {
            _initialTransform = _instanceInformation[1].WorldMatrix;
            Matrix4x4.Decompose(
                _initialTransform,
                out _initialScale,
                out _initialRotation,
                out _initialPosition);
            _firstFrame = false;
        }
        
        _instanceInformation[1] = new InstanceInformation
        {
            WorldMatrix = Matrix4x4.CreateTranslation(-_initialPosition) * 
                          Matrix4x4.CreateScale(_initialScale) * 
                          
                          Matrix4x4.CreateRotationY(MathHelper.ToRadians(_angle++)) *
                          
                          Matrix4x4.CreateTranslation(_initialPosition)
        };
        var instanceInformationArray = _instanceInformation.ToArray();
        _instanceInformationBuffer.Update(ref instanceInformationArray);
        
        
        GraphicsContext.BeginRenderPass(_swapchainDescriptor);
        GraphicsContext.BindGraphicsPipeline(_graphicsPipeline);
        _graphicsPipeline.BindAsVertexBuffer(_vertexBuffer, 0);
        _graphicsPipeline.BindAsIndexBuffer(_indexBuffer);
        
        _graphicsPipeline.BindAsUniformBuffer(_cameraInformationBuffer, 0);
        _graphicsPipeline.BindAsShaderStorageBuffer(_instanceInformationBuffer, 1);

        _graphicsPipeline.MultiDrawElementsIndirect(_drawIndirectBuffer, _meshIdCount);
        
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

                ImGui.SetCursorPos(new Vector2(ImGui.GetWindowViewport().Size.X - 160, 0));
                ImGui.TextUnformatted($"avg frametime: {_metrics.AverageFrameTime:F2} ms");

                ImGui.EndMenuBar();
            }

            ImGui.EndMainMenuBar();
        }
        UIRenderer.ShowDemoWindow();
        UIRenderer.EndLayout();

        GraphicsContext.EndRenderPass();
        if (_capabilities.IsLaunchedByNSightGraphicsOnLinux)
        {
            GL.Finish();
        }
    }

    protected override void Unload()
    {
        _scene.Dispose();
        base.Unload();
    }

    protected override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        if (IsKeyPressed(Glfw.Key.KeyEscape))
        {
            Close();
        }
        _scene.Update(deltaTime);

        if (IsMousePressed(Glfw.MouseButton.ButtonRight))
        {
            _camera.ProcessMouseMovement();
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
}