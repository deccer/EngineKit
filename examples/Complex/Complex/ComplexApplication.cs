using System.Numerics;
using Complex.Windows;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using EngineKit.UI;
using ImGuiNET;
using Microsoft.Extensions.Options;
using Serilog;

namespace Complex;

internal sealed class ComplexApplication : GraphicsApplication
{
    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly ICapabilities _capabilities;
    private readonly IMetrics _metrics;
    private readonly IModelLibrary _modelLibrary;
    private readonly ICamera _camera;
    private readonly IScene _scene;
    private readonly IRenderer _renderer;
    private readonly AssetWindow _assetWindow;
    private readonly SceneHierarchyWindow _sceneHierarchyWindow;
    private readonly SceneViewWindow _sceneViewWindow;
    private readonly PropertyWindow _propertyWindow;

    private SwapchainDescriptor _swapchainDescriptor;

    public ComplexApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        ICapabilities capabilities,
        IMetrics metrics,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IUIRenderer uiRenderer,
        IModelLibrary modelLibrary,
        ICamera camera,
        IScene scene,
        IRenderer renderer,
        AssetWindow assetWindow,
        SceneHierarchyWindow sceneHierarchyWindow,
        SceneViewWindow sceneViewWindow,
        PropertyWindow propertyWindow)
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
        _modelLibrary = modelLibrary;
        _camera = camera;
        _camera.Sensitivity = 0.125f;
        _camera.Mode = CameraMode.PerspectiveInfinity;
        _scene = scene;
        _renderer = renderer;
        _assetWindow = assetWindow;
        _sceneHierarchyWindow = sceneHierarchyWindow;
        _sceneViewWindow = sceneViewWindow;
        _propertyWindow = propertyWindow;
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

        if (!_renderer.Load())
        {
            return false;
        }

        _modelLibrary.AddModelFromFile("SM_Scene", "Data/Props/Scene/Scene.glb");
        _modelLibrary.AddModelFromFile("SM_Hierarchy", "Data/Props/Scene/Hierarchy.gltf");
        _modelLibrary.AddModelFromFile("SM_Deccer_Cubes", "Data/Default/SM_Deccer_Cubes_Textured.gltf");
        _modelLibrary.AddModelFromFile("SM_Complex", "Data/Default/SM_Deccer_Cubes_Textured_Complex.gltf");
        
        /*
        _modelLibrary.AddModelFromFile("SM_Bistor", "Data/Scenes/Bistro/scene.gltf");
        _modelLibrary.AddModelFromFile("SM_IntelSponza", "Data/Scenes/IntelSponza/NewSponza_Main_glTF_002.gltf");
        _modelLibrary.AddModelFromFile("SM_IntelSponzaCurtains", "Data/Scenes/IntelSponzaCurtains/NewSponza_Curtains_glTF.gltf");
        _modelLibrary.AddModelFromFile("SM_IntelSponzaIvy", "Data/Scenes/IntelSponzaIvy/NewSponza_IvyGrowth_glTF.gltf");
        _modelLibrary.AddModelFromFile("SM_IntelSponzaTree", "Data/Scenes/IntelSponzaTree/NewSponza_CypressTree_glTF.gltf");
        _modelLibrary.AddModelFromFile("SM_IntelSponzaCandles", "Data/Scenes/IntelSponzaCandles/NewSponza_4_Combined_glTF.gltf");
*/        

        _swapchainDescriptor = new SwapchainDescriptorBuilder()
            .EnableSrgb()
            .WithViewport(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y)
            .Build("Swapchain");
       
        return true;
    }

    protected override void Render(float deltaTime, float elapsedMilliseconds)
    {
        _renderer.Render(_camera);
        GraphicsContext.BeginRenderPass(_swapchainDescriptor);
        RenderUI();
        GraphicsContext.EndRenderPass();
        if (_applicationContext.IsLaunchedByNSightGraphicsOnLinux)
        {
            GL.Finish();
        }
    }

    protected override void Unload()
    {
        _scene.Dispose();
        base.Unload();
    }

    protected override void Update(float deltaTime, float elapsedMilliseconds)
    {
        base.Update(deltaTime, elapsedMilliseconds);
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

    protected override void FramebufferResized()
    {
        base.FramebufferResized();
        
        _swapchainDescriptor = new SwapchainDescriptorBuilder()
            .EnableSrgb()
            .WithViewport(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y)
            .Build("Swapchain");
        
        _renderer.ResizeFramebufferDependentResources();
    }

    protected override void HandleDebugger(out bool breakOnError)
    {
        breakOnError = true;
    }

    private void RenderUI()
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

                ImGui.EndMenuBar();
            }

            ImGui.EndMainMenuBar();
        }
        
        _assetWindow.Draw();
        _sceneHierarchyWindow.Draw();
        _sceneViewWindow.Draw();
        _propertyWindow.Draw();

        _renderer.RenderUI();
        
        UIRenderer.ShowDemoWindow();
        UIRenderer.EndLayout();
    }
}