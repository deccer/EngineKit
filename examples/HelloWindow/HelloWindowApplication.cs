using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using Microsoft.Extensions.Options;
using Serilog;

namespace HelloWindow;

internal sealed class HelloWindowApplication : Application
{
    private readonly ILogger _logger;
    private readonly IGraphicsContext _graphicsContext;

    public HelloWindowApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        IMetrics metrics,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext)
        : base(logger, windowSettings, contextSettings, applicationContext, metrics, inputProvider)
    {
        _logger = logger;
        _graphicsContext = graphicsContext;
    }

    protected override bool Load()
    {
        if (!base.Load())
        {
            _logger.Error("{Category}: Unable to load", "App");
            return false;
        }

        GL.ClearColor(0.4f, 0.2f, 0.1f, 1.0f);
        return true;
    }

    protected override void Render()
    {
        GL.Clear(GL.ClearBufferMask.ColorBufferBit | GL.ClearBufferMask.DepthBufferBit);
    }

    protected override void Unload()
    {
        _graphicsContext.Dispose();
        base.Unload();
    }

    protected override void Update()
    {
        if (IsKeyPressed(Glfw.Key.KeyEscape))
        {
            Close();
        }

        if (IsMousePressed(Glfw.MouseButton.ButtonLeft))
        {
            _logger.Debug("Left Mouse Button Pressed");
        }
    }
}