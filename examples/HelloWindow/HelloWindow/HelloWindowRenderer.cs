using System.Numerics;
using EngineKit;
using EngineKit.Core;
using EngineKit.Core.Messages;
using EngineKit.Graphics;
using EngineKit.Native.OpenGL;
using EngineKit.UI;
using ImGuiNET;

namespace HelloWindow;

internal sealed class HelloWindowRenderer : IRenderer
{
    private readonly IApplicationContext _applicationContext;
    private readonly IMessageBus _messageBus;
    private readonly IUIRenderer _uiRenderer;
    private readonly ICapabilities _capabilities;
    private readonly IMetrics _metrics;

    public HelloWindowRenderer(
        IApplicationContext applicationContext,
        IMessageBus messageBus,
        IUIRenderer uiRenderer,
        ICapabilities capabilities,
        IMetrics metrics)
    {
        _applicationContext = applicationContext;
        _messageBus = messageBus;
        _uiRenderer = uiRenderer;
        _capabilities = capabilities;
        _metrics = metrics;
    }

    public void Dispose()
    {
    }

    public bool Load()
    {
        return true;
    }

    public void Render(float deltaTime, float elapsedTime)
    {
        GL.Clear(GL.FramebufferBit.ColorBufferBit | GL.FramebufferBit.DepthBufferBit);
    }

    public void RenderUi(float deltaTime,
        float elapsedTime)
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Quit"))
                    {
                        _messageBus.PublishWait(new CloseWindowMessage());
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
                        _messageBus.PublishWait(new RestoreWindowMessage());
                    }
                    else
                    {
                        _messageBus.PublishWait(new MaximizeWindowMessage());
                    }
                }

                ImGui.SameLine();
                if (ImGui.Button(MaterialDesignIcons.WindowClose))
                {
                    _messageBus.PublishWait(new CloseWindowMessage());
                }

                ImGui.EndMenuBar();
            }

            ImGui.EndMainMenuBar();
        }

        _uiRenderer.ShowDemoWindow();
    }

    public void ResizeIfNecessary()
    {
    }
}
