using System.Threading.Tasks;
using Complex.States;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Messages;
using EngineKit.Native.OpenGL;
using Microsoft.Extensions.Options;
using Serilog;

namespace Complex;

internal sealed class ComplexApplication : GraphicsApplication
{
    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly IModelLibrary _modelLibrary;
    private readonly ICamera _camera;
    private readonly IScene _scene;
    private readonly IRenderer _renderer;
    private readonly ILayeredProgramStates _layeredProgramStates;

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
        IMessageBus messageBus,
        ILayeredProgramStates layeredProgramStates)
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
        _modelLibrary = modelLibrary;
        _camera = camera;
        _camera.Sensitivity = 0.125f;
        _camera.Mode = CameraMode.PerspectiveInfinity;
        _scene = scene;
        _renderer = renderer;
        _layeredProgramStates = layeredProgramStates;
        
        messageBus.Subscribe<CloseWindowMessage>(OnCloseWindowMessage);
        messageBus.Subscribe<MaximizeWindowMessage>(OnMaximizeWindowMessage);
        messageBus.Subscribe<RestoreWindowMessage>(OnRestoreWindowMessage);        
    }
    
    protected override bool Initialize()
    {
        if (!base.Initialize())
        {
            return false;
        }
        
        SetWindowIcon("enginekit-icon.png");

        _layeredProgramStates.ComposeLayeredState("Editor", [nameof(GameProgramState), nameof(EditorProgramState)]);
        _layeredProgramStates.SwitchToState("Editor");

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

        /*
        _modelLibrary.AddModelFromFile("SM_Scene", "Data/Props/Scene/Scene.glb");
        _modelLibrary.AddModelFromFile("SM_Hierarchy", "Data/Props/Scene/Hierarchy.gltf");
        _modelLibrary.AddModelFromFile("SM_Deccer_Cubes", "Data/Default/SM_Deccer_Cubes_Textured.gltf");
        _modelLibrary.AddModelFromFile("SM_Deccer_Cubes_WR", "Data/Default/SM_Deccer_Cubes_With_Rotation.glb");
        */

        _modelLibrary.AddModelFromFile("SM_Deccer_Cubes_Complex", "Data/Default/SM_Deccer_Cubes_Textured_Complex.gltf");
        _modelLibrary.AddModelFromFile("Blender_PositionNormal", "Data/Default/Blender_Cube_PositionNormal.glb");
        _modelLibrary.AddModelFromFile("Blender_PositionNormalUv", "Data/Default/Blender_Cube_PositionNormalUv.glb");
        _modelLibrary.AddModelFromFile("Blender_PositionNormalUvTangent", "Data/Default/Blender_Cube_PositionNormalUvTangent.glb");
        
        _modelLibrary.AddModelFromFile("Asteroid1", "Data/Props/Asteroids/asteroid1.glb");
        _modelLibrary.AddModelFromFile("Asteroid2", "Data/Props/Asteroids/asteroid2.glb");
        _modelLibrary.AddModelFromFile("Asteroid LP", "Data/Props/Asteroids/asteroid_low_poly_3d_model.glb");
        _modelLibrary.AddModelFromFile("Nasa1", "Data/Props/Asteroids/nasa1.glb");
        _modelLibrary.AddModelFromFile("Nasa2", "Data/Props/Asteroids/nasa2.glb");
        _modelLibrary.AddModelFromFile("Nasa3", "Data/Props/Asteroids/nasa3.glb");
        _modelLibrary.AddModelFromFile("Nasa4", "Data/Props/Asteroids/nasa4.glb");
        _modelLibrary.AddModelFromFile("FromSpace", "Data/Props/Asteroids/rock_from_space.glb");
        //_modelLibrary.AddModelFromFile("E1M1", "Data/Scenes/E1M1/E1M1.gltf");
        //_modelLibrary.AddModelFromFile("SmallCity", "Data/Scenes/small_city/small_city.gltf");
        
        /*
        _modelLibrary.AddModelFromFile("SM_Bistor", "Data/Scenes/Bistro/scene.gltf");
        _modelLibrary.AddModelFromFile("SM_IntelSponza", "Data/Scenes/IntelSponza/NewSponza_Main_glTF_002.gltf");
        _modelLibrary.AddModelFromFile("SM_IntelSponzaCurtains", "Data/Scenes/IntelSponzaCurtains/NewSponza_Curtains_glTF.gltf");
        _modelLibrary.AddModelFromFile("SM_IntelSponzaIvy", "Data/Scenes/IntelSponzaIvy/NewSponza_IvyGrowth_glTF.gltf");
        _modelLibrary.AddModelFromFile("SM_IntelSponzaTree", "Data/Scenes/IntelSponzaTree/NewSponza_CypressTree_glTF.gltf");
        _modelLibrary.AddModelFromFile("SM_IntelSponzaCandles", "Data/Scenes/IntelSponzaCandles/NewSponza_4_Combined_glTF.gltf");
*/

        if (!_layeredProgramStates.Load())
        {
            return false;
        }
        
        return true;
    }

    protected override void Render(float deltaTime, float elapsedSeconds)
    {
        _layeredProgramStates.Render(deltaTime, elapsedSeconds);

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

    protected override void Update(float deltaTime, float elapsedSeconds)
    {
        base.Update(deltaTime, elapsedSeconds);

        _layeredProgramStates.Update(deltaTime, elapsedSeconds);
    }

    protected override void FramebufferResized()
    {
        base.FramebufferResized();
        
        _renderer.ResizeFramebufferDependentResources();
    }

    protected override void HandleDebugger(out bool breakOnError)
    {
        breakOnError = true;
    }

    private Task OnCloseWindowMessage(CloseWindowMessage message)
    {
        Close();
        return Task.CompletedTask;
    }

    private Task OnMaximizeWindowMessage(MaximizeWindowMessage message)
    {
        MaximizeWindow();
        return Task.CompletedTask;
    }

    private Task OnRestoreWindowMessage(RestoreWindowMessage message)
    {
        RestoreWindow();
        return Task.CompletedTask;
    }
}