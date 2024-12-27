using System.Numerics;
using Complex.Engine;
using EngineKit;
using EngineKit.Core;
using EngineKit.Core.Messages;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Native.Glfw;
using Serilog;

namespace Complex;

internal class Game
{
    private readonly ILogger _logger;
    private readonly IMessageBus _messageBus;
    private readonly IApplicationContext _applicationContext;
    private readonly IGraphicsContext _graphicsContext;

    private readonly ICamera _camera;
    private readonly IInputProvider _inputProvider;
    private readonly IModelLibrary _modelLibrary;
    private readonly IRenderer2 _renderer;
    private readonly IScene _scene;

    public Game(ILogger logger,
        IGraphicsContext graphicsContext,
        IRenderer2 renderer,
        ICamera camera,
        IScene scene,
        IInputProvider inputProvider,
        IMessageBus messageBus,
        IModelLibrary modelLibrary,
        IApplicationContext applicationContext)
    {
        _logger = logger.ForContext<Game>();
        _graphicsContext = graphicsContext;
        _renderer = renderer;
        _camera = camera;
        _scene = scene;
        _inputProvider = inputProvider;
        _messageBus = messageBus;
        _modelLibrary = modelLibrary;
        _applicationContext = applicationContext;
    }

    public bool Load()
    {
        if (!_renderer.Load())
        {
            _logger.Error("{Category} Unable to load renderer", "Game");
            return false;
        }

/*
        _modelLibrary.AddModelFromFile("SM_Scene", "Data/Props/Scene/Scene.glb");
        _modelLibrary.AddModelFromFile("SM_Hierarchy", "Data/Props/Scene/Hierarchy.gltf");
        _modelLibrary.AddModelFromFile("SM_Deccer_Cubes", "Data/Default/SM_Deccer_Cubes_Textured.gltf");
        _modelLibrary.AddModelFromFile("SM_Deccer_Cubes_WR", "Data/Default/SM_Deccer_Cubes_With_Rotation.glb");
*/


        _modelLibrary.AddModelFromFile("SM_Deccer_Cubes_Complex", "Data/Default/SM_Deccer_Cubes_Textured_Complex.gltf");
        /*
                _modelLibrary.AddModelFromFile("Nasa1", "Data/Props/Asteroids/nasa1.glb");
                _modelLibrary.AddModelFromFile("Nasa2", "Data/Props/Asteroids/nasa2.glb");
                _modelLibrary.AddModelFromFile("Nasa3", "Data/Props/Asteroids/nasa3.glb");
                _modelLibrary.AddModelFromFile("Nasa4", "Data/Props/Asteroids/nasa4.glb");
                _modelLibrary.AddModelFromFile("SF", "Data/Props/Test5.gltf");
                */
        //_modelLibrary.AddModelFromFile("SM_Deccer_Cubes_Complex", "/home/deccer/Code/Caldera/SM_Airfield_Ground_mi.glb");
        //_modelLibrary.AddModelFromFile("SM_Deccer_Cubes_Complex", "Data/Props/SimpleInstancing.glb");
        //_modelLibrary.AddModelFromFile("FromSpace", "Data/Props/Asteroids/rock_from_space.glb");
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

        var model = _modelLibrary.GetModelByName("SM_Deccer_Cubes_Complex");
        _scene.AddEntityWithModelRenderer(model.Name, null, model, Matrix4x4.Identity);

        return true;
    }

    public void Render(
        float deltaTime,
        float elapsedSeconds)
    {
        _renderer.Render(_camera);
    }

    public void Update(
        float deltaTime,
        float elapsedSeconds)
    {
        if (_inputProvider.KeyboardState.IsKeyPressed(Glfw.Key.KeyEscape))
        {
            _messageBus.PublishWait(new CloseWindowMessage());
        }

        _scene.Update(deltaTime);

        if (_inputProvider.MouseState.IsButtonDown(Glfw.MouseButton.ButtonRight))
        {
            _camera.ProcessMouseMovement();
        }

        _camera.ProcessKeyboard();
        _camera.AdvanceSimulation(deltaTime);
    }

    public void Unload()
    {
        _scene.Dispose();
    }
}
