using System.Threading.Tasks;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Messages;
using EngineKit.Native.Glfw;
using Serilog;

namespace Complex.States;

internal class GameProgramState : IProgramState
{
    private readonly IApplicationContext _applicationContext;

    private readonly ICamera _camera;

    private readonly IGraphicsContext _graphicsContext;

    private readonly IInputProvider _inputProvider;

    private readonly ILogger _logger;

    private readonly IMessageBus _messageBus;

    private readonly IRenderer _renderer;

    private readonly IScene _scene;

    public GameProgramState(ILogger logger,
                            IGraphicsContext graphicsContext,
                            IRenderer renderer,
                            ICamera camera,
                            IScene scene,
                            IInputProvider inputProvider,
                            IMessageBus messageBus,
                            IApplicationContext applicationContext)
    {
        _logger = logger.ForContext<GameProgramState>();
        _graphicsContext = graphicsContext;
        _renderer = renderer;
        _camera = camera;
        _scene = scene;
        _inputProvider = inputProvider;
        _messageBus = messageBus;
        _applicationContext = applicationContext;

        _messageBus.Subscribe<FramebufferResizedMessage>(OnFramebufferResized);
    }

    public void Activate()
    {
        _applicationContext.IsEditorEnabled = false;
    }

    public bool Load()
    {
        if (!_renderer.Load())
        {
            return false;
        }

        _logger.Debug("{Category}: Loaded {ProgramStateName}",
                "ProgramState",
                GetType().Name);

        return true;
    }

    public void Render(float deltaTime,
                       float elapsedSeconds)
    {
        _renderer.Render(_camera);
    }

    public void Update(float deltaTime,
                       float elapsedSeconds)
    {
        if (_inputProvider.KeyboardState.IsKeyPressed(Glfw.Key.KeyEscape))
        {
            _messageBus.PublishWait(new CloseWindowMessage());
        }
        _scene.Update(deltaTime);

        if (_inputProvider.MouseState.IsButtonDown(Glfw.MouseButton.ButtonRight)) _camera.ProcessMouseMovement();

        _camera.ProcessKeyboard();
        _camera.AdvanceSimulation(deltaTime);
    }

    private Task OnFramebufferResized(FramebufferResizedMessage message)
    {
        return Task.CompletedTask;
    }
}
