using System.Numerics;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Input;
using EngineKit.Mathematics;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;
using EngineKit.UI;
using ImGuiNET;
using Microsoft.Extensions.Options;
using Serilog;

namespace HelloWindow;

internal sealed class HelloWindowApplication : GraphicsApplication
{
    private readonly IApplicationContext _applicationContext;

    private readonly ICapabilities _capabilities;

    private readonly Color4 _clearColor;

    private readonly ILogger _logger;

    private readonly IMetrics _metrics;

    public HelloWindowApplication(ILogger logger,
                                  IOptions<WindowSettings> windowSettings,
                                  IOptions<ContextSettings> contextSettings,
                                  IApplicationContext applicationContext,
                                  ICapabilities capabilities,
                                  IMetrics metrics,
                                  IInputProvider inputProvider,
                                  IGraphicsContext graphicsContext,
                                  IUIRenderer uiRenderer)
            : base(logger,
                   windowSettings,
                   contextSettings,
                   applicationContext,
                   capabilities,
                   metrics,
                   inputProvider,
                   graphicsContext,
                   uiRenderer)
    {
        _logger = logger;
        _applicationContext = applicationContext;
        _capabilities = capabilities;
        _metrics = metrics;
        _clearColor = MathHelper.GammaToLinear(Colors.DarkSlateBlue);
    }

    protected override bool OnInitialize()
    {
        if (!base.OnInitialize())
        {
            return false;
        }

        SetWindowIcon("enginekit-icon.png");

        return true;
    }

    protected override bool OnLoad()
    {
        if (!base.OnLoad())
        {
            _logger.Error("{Category}: Unable to load", "App");
            return false;
        }

        GL.ClearColor(_clearColor.R,
                _clearColor.G,
                _clearColor.B,
                1.0f);
        return true;
    }

    protected override void OnRender(float deltaTime,
                                   float elapsedSeconds)
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

                var isNvidia = _capabilities.SupportsNvx;
                if (isNvidia)
                {
                    ImGui.SetCursorPos(new Vector2(ImGui.GetWindowViewport().Size.X - 416, 0));
                    ImGui.TextUnformatted($"video memory: {_capabilities.GetCurrentAvailableGpuMemoryInMebiBytes()} MiB");
                    ImGui.SameLine();
                }
                else
                {
                    ImGui.SetCursorPos(new Vector2(ImGui.GetWindowViewport().Size.X - 256, 0));
                }

                ImGui.TextUnformatted($"avg frame time: {_metrics.AverageFrameTime:F2} ms");
                ImGui.SameLine();
                ImGui.Button(MaterialDesignIcons.WindowMinimize);
                ImGui.SameLine();
                if (ImGui.Button(_applicationContext.IsWindowMaximized
                            ? MaterialDesignIcons.WindowRestore
                            : MaterialDesignIcons.WindowMaximize))
                {
                    if (_applicationContext.IsWindowMaximized)
                    {
                        RestoreWindow();
                    }
                    else
                    {
                        MaximizeWindow();
                    }
                }

                ImGui.SameLine();
                if (ImGui.Button(MaterialDesignIcons.WindowClose))
                {
                    Close();
                }

                ImGui.EndMenuBar();
            }

            ImGui.EndMainMenuBar();
        }

        UIRenderer.ShowDemoWindow();
        UIRenderer.EndLayout();

        if (_applicationContext.IsLaunchedByNSightGraphicsOnLinux)
        {
            GL.Finish();
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();
    }

    protected override void OnUpdate(float deltaTime,
                                   float elapsedSeconds)
    {
        base.OnUpdate(deltaTime, elapsedSeconds);
        if (IsKeyPressed(Glfw.Key.KeyEscape)) Close();
    }
}
