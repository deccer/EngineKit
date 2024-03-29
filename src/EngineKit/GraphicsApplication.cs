using System.Numerics;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Messages;
using EngineKit.Native.OpenGL;
using Microsoft.Extensions.Options;
using Serilog;

namespace EngineKit;

public abstract class GraphicsApplication : Application
{
    private readonly ILogger _logger;

    private readonly IApplicationContext _applicationContext;
    private readonly IMessageBus _messageBus;

    protected IGraphicsContext GraphicsContext { get; }
    protected IUIRenderer UIRenderer { get; }

    protected GraphicsApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        ICapabilities capabilities,
        IMetrics metrics,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IUIRenderer uiRenderer,
        IMessageBus messageBus)
        : base(logger, windowSettings, contextSettings, applicationContext, capabilities, metrics, inputProvider)
    {
        _logger = logger;
        _applicationContext = applicationContext;
        _messageBus = messageBus;
        GraphicsContext = graphicsContext;
        UIRenderer = uiRenderer;
    }

    protected override void FramebufferResized()
    {
        base.FramebufferResized();
        _messageBus.PublishWait(new FramebufferResizedMessage());
        UIRenderer.WindowResized(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y);
    }

    protected override bool Load()
    {
        if (!base.Load())
        {
            _logger.Error("{Category}: Unable to load", "App");
            return false;
        }

        if (!UIRenderer.Load(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y))
        {
            return false;
        }

        GL.Disable(GL.EnableType.Blend);
        GL.Disable(GL.EnableType.CullFace);
        GL.Disable(GL.EnableType.ScissorTest);
        GL.Disable(GL.EnableType.DepthTest);
        GL.Disable(GL.EnableType.StencilTest);
        GL.Disable(GL.EnableType.SampleCoverage);
        GL.Disable(GL.EnableType.SampleAlphaToCoverage);
        GL.Disable(GL.EnableType.PolygonOffsetFill);
        GL.Disable(GL.EnableType.Multisample);
        GL.Enable(GL.EnableType.FramebufferSrgb);
        
        return true;
    }

    protected override void Unload()
    {
        UIRenderer.Dispose();
        GraphicsContext.Dispose();
        base.Unload();
    }

    protected override void Update(float deltaTime, float elapsedSeconds)
    {
        UIRenderer.Update(deltaTime);
    }

    protected override void MouseScrolled(double scrollX, double scrollY)
    {
        UIRenderer.MouseScroll(new Vector2((float)scrollX, (float)scrollY));
    }

    protected override void CharacterInput(char codePoint)
    {
        UIRenderer.CharacterInput(codePoint);
    }
}