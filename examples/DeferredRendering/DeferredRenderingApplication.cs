using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using ImGuiNET;
using Microsoft.Extensions.Options;
using Serilog;

namespace HelloWindow;

internal sealed class DeferredRenderingApplication : Application
{
    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly IGraphicsContext _graphicsContext;
    private readonly IUIRenderer _uiRenderer;

    public DeferredRenderingApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        IMetrics metrics,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IUIRenderer uiRenderer)
        : base(logger, windowSettings, contextSettings, applicationContext, metrics, inputProvider)
    {
        _logger = logger;
        _applicationContext = applicationContext;
        _graphicsContext = graphicsContext;
        _uiRenderer = uiRenderer;
    }

    protected override bool Load()
    {
        if (!base.Load())
        {
            _logger.Error("{Category}: Unable to load", "App");
            return false;
        }

        if (!_uiRenderer.Load(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y))
        {
            return false;
        }

        GL.ClearColor(0.95f, 0.5f, 0.4f, 1.0f);
        return true;
    }

    protected override void Render()
    {
        GL.Clear(GL.ClearBufferMask.ColorBufferBit | GL.ClearBufferMask.DepthBufferBit);

        _uiRenderer.BeginLayout();
        ImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode);
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Quit"))
                    {
                        Close();
                    }
                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            ImGui.EndMainMenuBar();
        }
        _uiRenderer.ShowDemoWindow();
        _uiRenderer.EndLayout();
    }

    protected override void FramebufferResized()
    {
        base.FramebufferResized();
        _uiRenderer.WindowResized(_applicationContext.FramebufferSize.X, _applicationContext.FramebufferSize.Y);
    }

    protected override void Unload()
    {
        _graphicsContext.Dispose();
        base.Unload();
    }

    protected override void Update()
    {
        _uiRenderer?.Update(1.0f / 60.0f);
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