using System.Threading.Tasks;
using EngineKit;
using EngineKit.Core;
using EngineKit.Core.Messages;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Native.OpenGL;
using Microsoft.Extensions.Options;
using Serilog;

//TODO(deccer): Remove Camera from GraphicsApplication, should be an entity? in a scene?

namespace Complex;

internal sealed class ComplexApplication : GraphicsApplication
{
    private readonly ILogger _logger;

    private readonly IApplicationContext _applicationContext;

    private readonly Game _game;

    private readonly Editor _editor;

    public ComplexApplication(ILogger logger,
                              IOptions<WindowSettings> windowSettings,
                              IOptions<ContextSettings> contextSettings,
                              IApplicationContext applicationContext,
                              ICapabilities capabilities,
                              IMetrics metrics,
                              IInputProvider inputProvider,
                              IGraphicsContext graphicsContext,
                              IUIRenderer uiRenderer,
                              IRenderer renderer,
                              IMessageBus messageBus,
                              Game game,
                              Editor editor)
            : base(logger,
                   windowSettings,
                   contextSettings,
                   messageBus,
                   applicationContext,
                   capabilities,
                   metrics,
                   inputProvider,
                   graphicsContext,
                   renderer,
                   uiRenderer)
    {
        _logger = logger;
        _applicationContext = applicationContext;
        _game = game;
        _editor = editor;
        /* TODO(deccer) move this to where/when camera is created
        _camera.Sensitivity = 0.125f;
        _camera.Mode = CameraMode.PerspectiveInfinity;
        */

        messageBus.Subscribe<CloseWindowMessage>(OnCloseWindowMessage);
        messageBus.Subscribe<MaximizeWindowMessage>(OnMaximizeWindowMessage);
        messageBus.Subscribe<RestoreWindowMessage>(OnRestoreWindowMessage);
    }

    protected override bool OnInitialize()
    {
        if (!base.OnInitialize())
        {
            return false;
        }

        SetWindowIcon("enginekit-icon.png");

        return true;
    }

    protected override bool OnLoad()
    {
        if (!base.OnLoad())
        {
            _logger.Error("{Category}: Unable to load", "App");
            return false;
        }

        if (!_game.Load())
        {
            return false;
        }

        if (!_editor.Load())
        {
            return false;
        }

        return true;
    }

    protected override void OnRender(float deltaTime,
                                   float elapsedSeconds)
    {
        _game.Render(deltaTime, elapsedSeconds);
        _editor.Render(deltaTime, elapsedSeconds);
        if (_applicationContext.IsLaunchedByNSightGraphicsOnLinux)
        {
            GL.Finish();
        }
    }

    protected override void OnUnload()
    {
        _editor.Unload();
        _game.Unload();
        base.OnUnload();
    }

    protected override void OnUpdate(float deltaTime,
                                   float elapsedSeconds)
    {
        base.OnUpdate(deltaTime, elapsedSeconds);

        _game.Update(deltaTime, elapsedSeconds);
        _editor.Update(deltaTime, elapsedSeconds);
    }

    protected override void OnHandleDebugger(out bool breakOnError)
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
