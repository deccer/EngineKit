using System.Numerics;
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
    private readonly IMetrics _metrics;

    public HelloWindowApplication(
        ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IOptions<ContextSettings> contextSettings,
        IApplicationContext applicationContext,
        ICapabilities capabilities,
        IMetrics metrics,
        ILimits limits,
        IInputProvider inputProvider,
        IGraphicsContext graphicsContext,
        IUIRenderer uiRenderer)
        : base(
            logger,
            windowSettings,
            contextSettings,
            applicationContext,
            capabilities,
            metrics,
            limits,
            inputProvider,
            graphicsContext,
            uiRenderer)
    {
        _logger = logger;
        _metrics = metrics;
    }
    
    protected override bool Initialize()
    {
        if (!base.Initialize())
        {
            return false;
        }
        
        SetWindowIcon("enginekit-icon.png");

        return true;
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

    protected override void Render(float deltaTime)
    {
        GL.Clear(GL.FramebufferBit.ColorBufferBit | GL.FramebufferBit.DepthBufferBit);

        UIRenderer.BeginLayout();
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

                ImGui.SetCursorPos(new Vector2(ImGui.GetWindowViewport().Size.X - 64, 0));
                ImGui.TextUnformatted($"Fps: {_metrics.AverageFrameTime}");

                ImGui.EndMenuBar();
            }

            ImGui.EndMainMenuBar();
        }
        UIRenderer.ShowDemoWindow();
        UIRenderer.EndLayout();

        GL.Finish();
    }

    protected override void Unload()
    {
        base.Unload();
    }

    protected override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        if (IsKeyPressed(Glfw.Key.KeyEscape))
        {
            Close();
        }
    }
}