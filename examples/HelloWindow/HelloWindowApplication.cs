using EngineKit;
using EngineKit.Native.Glfw;
using Serilog;

namespace HelloWindow;

internal sealed class HelloWindowApplication : Application
{
    private readonly ILogger _logger;

    public HelloWindowApplication(
        ILogger logger,
        IApplicationContext applicationContext,
        IMetrics metrics,
        IInputProvider inputProvider)
        : base(logger, applicationContext, metrics, inputProvider)
    {
        _logger = logger;
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