using EngineKit;
using EngineKit.Core;
using EngineKit.Graphics;
using EngineKit.Input;
using Microsoft.Extensions.Options;
using Serilog;

namespace Complex.Engine;

public class GameApplication<TGame> : GraphicsApplication where TGame : IGame
{
    private readonly TGame _game;

    public GameApplication(ILogger logger,
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
        TGame game) : base(logger, windowSettings, contextSettings, messageBus, applicationContext, capabilities, metrics, inputProvider, graphicsContext, renderer, uiRenderer)
    {
        _game = game;
    }

    protected override bool OnLoad()
    {
        if (!base.OnLoad())
        {
            return false;
        }

        if (!_game.Load())
        {
            return false;
        }

        return true;
    }

    protected override void OnUnload()
    {
        _game.Unload();
        base.OnUnload();
    }
}
