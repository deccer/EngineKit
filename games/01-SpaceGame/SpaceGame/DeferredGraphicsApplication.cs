using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using Microsoft.Extensions.Options;
using Serilog;

namespace SpaceGame;

internal class DeferredGraphicsApplication : GraphicsApplication
{
    public DeferredGraphicsApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        IMetrics metrics,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IUIRenderer uiRenderer)
        : base(logger, windowSettings, contextSettings, applicationContext, metrics, inputProvider, graphicsContext, uiRenderer)
    {
    }
}