using EngineKit;
using EngineKit.Core;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Mathematics;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using Microsoft.Extensions.Options;
using Serilog;

namespace HelloWindow;

internal sealed class HelloWindowApplication : GraphicsApplication
{
    private readonly IApplicationContext _applicationContext;

    private readonly ICapabilities _capabilities;

    private readonly Color4 _clearColor;

    private readonly ILogger _logger;

    private readonly IMetrics _metrics;

    public HelloWindowApplication(ILogger logger,
                                  IOptions<WindowSettings> windowSettings,
                                  IOptions<ContextSettings> contextSettings,
                                  IMessageBus messageBus,
                                  IApplicationContext applicationContext,
                                  ICapabilities capabilities,
                                  IMetrics metrics,
                                  IInputProvider inputProvider,
                                  IGraphicsContext graphicsContext,
                                  IRenderer renderer,
                                  IUIRenderer uiRenderer)
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
        _capabilities = capabilities;
        _metrics = metrics;
        _clearColor = MathHelper.GammaToLinear(Colors.DarkSlateBlue);
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

        GL.ClearColor(_clearColor.R,
                _clearColor.G,
                _clearColor.B,
                1.0f);
        return true;
    }

    protected override void OnUnload()
    {
        base.OnUnload();
    }

    protected override void OnUpdate(float deltaTime,
                                   float elapsedSeconds)
    {
        base.OnUpdate(deltaTime, elapsedSeconds);
        if (IsKeyPressed(Glfw.Key.KeyEscape))
        {
            Close();
        }
    }
}
