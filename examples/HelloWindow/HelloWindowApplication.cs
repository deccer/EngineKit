using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using ImGuiNET;
using Microsoft.Extensions.Options;
using Serilog;

namespace HelloWindow;

internal sealed class HelloWindowApplication : GraphicsApplication
{
    private readonly ILogger _logger;

    public HelloWindowApplication(
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
        _logger = logger;
    }

    protected override bool Load()
    {
        if (!base.Load())
        {
            _logger.Error("{Category}: Unable to load", "App");
            return false;
        }

        GL.ClearColor(0.95f, 0.5f, 0.4f, 1.0f);
        return true;
    }

    protected override void Render()
    {
        GL.Clear(GL.ClearBufferMask.ColorBufferBit | GL.ClearBufferMask.DepthBufferBit);

        UIRenderer.BeginLayout();
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
        UIRenderer.ShowDemoWindow();
        UIRenderer.EndLayout();
    }

    protected override void Unload()
    {
        base.Unload();
    }

    protected override void Update()
    {
        base.Update();
        if (IsKeyPressed(Glfw.Key.KeyEscape))
        {
            Close();
        }
    }
}