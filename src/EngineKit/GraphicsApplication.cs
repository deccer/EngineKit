using System.Numerics;
using EngineKit.Core;
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
        IMessageBus messageBus,
        IApplicationContext applicationContext,
        ICapabilities capabilities,
        IMetrics metrics,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IRenderer renderer,
        IUIRenderer uiRenderer)
        : base(logger, windowSettings, contextSettings, applicationContext, messageBus, capabilities, metrics, inputProvider)
    {
        _logger = logger;
        _applicationContext = applicationContext;
        Renderer = renderer;
        GraphicsContext = graphicsContext;
        UIRenderer = uiRenderer;
    }

    protected IGraphicsContext GraphicsContext { get; }

    protected IRenderer Renderer { get; }

    protected IUIRenderer UIRenderer { get; }

    protected override bool OnLoad()
    {
        if (!base.OnLoad())
        {
            _logger.Error("{Category}: Unable to load", "App");
            return false;
        }

        if (!Renderer.Load())
        {
            return false;
        }

        if (!UIRenderer.Load())
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
        Renderer.Dispose();
        UIRenderer.Dispose();
        GraphicsContext.Dispose();
        base.OnUnload();
    }

    protected override void OnRender(float deltaTime, float elapsedSeconds)
    {
        if(_applicationContext.HasWindowFramebufferSizeChanged || _applicationContext.HasSceneViewSizeChanged)
        {
            Renderer.ResizeIfNecessary();
            UIRenderer.ResizeWindow(_applicationContext.WindowFramebufferSize.X, _applicationContext.WindowFramebufferSize.Y);

            _applicationContext.HasWindowFramebufferSizeChanged = false;
            _applicationContext.HasSceneViewSizeChanged = false;
        }
        Renderer.Render(deltaTime, elapsedSeconds);

        UIRenderer.BeginLayout();
        Renderer.RenderUi(deltaTime, elapsedSeconds);
        UIRenderer.EndLayout();
    }

    protected override void OnUpdate(float deltaTime, float elapsedSeconds)
    {
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
