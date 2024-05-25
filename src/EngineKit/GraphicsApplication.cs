using System.Numerics;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Native.OpenGL;
using Microsoft.Extensions.Options;
using Serilog;

namespace EngineKit;

public abstract class GraphicsApplication : Application
{
    private readonly ILogger _logger;

    private readonly IApplicationContext _applicationContext;

    protected GraphicsApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        ICapabilities capabilities,
        IMetrics metrics,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IUIRenderer uiRenderer)
        : base(logger, windowSettings, contextSettings, applicationContext, capabilities, metrics, inputProvider)
    {
        _logger = logger;
        _applicationContext = applicationContext;
        GraphicsContext = graphicsContext;
        UIRenderer = uiRenderer;
    }

    protected IGraphicsContext GraphicsContext { get; }

    protected IUIRenderer UIRenderer { get; }

    protected override bool OnLoad()
    {
        if (!base.OnLoad())
        {
            _logger.Error("{Category}: Unable to load", "App");
            return false;
        }

        if (!UIRenderer.Load(_applicationContext.WindowFramebufferSize.X, _applicationContext.WindowFramebufferSize.Y))
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

    protected override void OnUnload()
    {
        UIRenderer.Dispose();
        GraphicsContext.Dispose();
        base.OnUnload();
    }

    protected override void OnUpdate(float deltaTime, float elapsedSeconds)
    {
        if (_applicationContext.HasWindowFramebufferSizeChanged)
        {
            UIRenderer.ResizeWindow(_applicationContext.WindowFramebufferSize.X, _applicationContext.WindowFramebufferSize.Y);
        }
        UIRenderer.Update(deltaTime);
    }

    protected override void OnMouseScrolled(double scrollX, double scrollY)
    {
        UIRenderer.MouseScroll(new Vector2((float)scrollX, (float)scrollY));
    }

    protected override void OnCharacterInput(char codePoint)
    {
        UIRenderer.CharacterInput(codePoint);
    }
}
