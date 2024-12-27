using EngineKit;
using EngineKit.Core;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Mathematics;
using EngineKit.Native.Glfw;
using Microsoft.Extensions.Options;
using Serilog;

namespace ForwardRendering;

internal sealed class ForwardRendererApplication : GraphicsApplication
{
    private readonly ILogger _logger;
    private readonly ICamera _camera;

    public ForwardRendererApplication(ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IMessageBus messageBus,
        IApplicationContext applicationContext,
        ICapabilities capabilities,
        IMetrics metrics,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IRenderer renderer,
        IUIRenderer uiRenderer,
        ICamera camera)
        : base(
            logger,
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
        _camera = camera;
        _camera.Sensitivity = 0.125f;
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

        _camera.AdvanceSimulation(0.0f);

        return true;
    }

    protected override void OnHandleDebugger(out bool breakOnError)
    {
        breakOnError = true;
    }

    protected override void OnUpdate(float deltaTime,
        float elapsedSeconds)
    {
        base.OnUpdate(deltaTime, elapsedSeconds);

        if (IsMousePressed(Glfw.MouseButton.ButtonRight))
        {
            _camera.ProcessMouseMovement();
        }

        _camera.ProcessKeyboard();

        if (IsKeyPressed(Glfw.Key.KeyEscape))
        {
            Close();
        }

        if (IsMousePressed(Glfw.MouseButton.ButtonLeft))
        {
            _logger.Debug("Left Mouse Button Pressed");
        }

        if (IsKeyPressed(Glfw.Key.KeyF5))
        {
            HideCursor();
        }

        if (IsKeyPressed(Glfw.Key.KeyF6))
        {
            ShowCursor();
        }
    }
}
