using System.Numerics;
using Complex.Engine;
using Complex.Windows;
using EngineKit;
using EngineKit.Core;
using EngineKit.Core.Messages;
using EngineKit.Graphics;
using EngineKit.UI;
using ImGuiNET;
using Microsoft.Extensions.Options;
using Serilog;

namespace Complex;

internal sealed class Editor
{
    private readonly IApplicationContext _applicationContext;

    private readonly AssetWindow _assetWindow;

    private readonly ICapabilities _capabilities;

    private readonly IGraphicsContext _graphicsContext;

    private readonly ILogger _logger;

    private readonly IOptions<WindowSettings> _windowSettings;

    private readonly IMessageBus _messageBus;

    private readonly IMetrics _metrics;

    private readonly PropertyWindow _propertyWindow;

    private readonly IRenderer2 _renderer;

    private readonly SceneHierarchyWindow _sceneHierarchyWindow;

    private readonly SceneViewWindow _sceneViewWindow;

    private readonly IUIRenderer _uiRenderer;

    private SwapchainDescriptor _swapchainDescriptor;

    public Editor(ILogger logger,
        IOptions<WindowSettings> windowSettings,
        IApplicationContext applicationContext,
        ICapabilities capabilities,
        IMetrics metrics,
        IGraphicsContext graphicsContext,
        IRenderer2 renderer,
        IUIRenderer uiRenderer,
        IMessageBus messageBus,
        AssetWindow assetWindow,
        SceneHierarchyWindow sceneHierarchyWindow,
        SceneViewWindow sceneViewWindow,
        PropertyWindow propertyWindow)
    {
        _logger = logger;
        _windowSettings = windowSettings;
        _applicationContext = applicationContext;
        _capabilities = capabilities;
        _metrics = metrics;
        _graphicsContext = graphicsContext;
        _renderer = renderer;
        _uiRenderer = uiRenderer;
        _messageBus = messageBus;
        _assetWindow = assetWindow;
        _sceneHierarchyWindow = sceneHierarchyWindow;
        _sceneViewWindow = sceneViewWindow;
        _propertyWindow = propertyWindow;
    }

    public bool Load()
    {
        _swapchainDescriptor = CreateSwapchainDescriptor(
            _applicationContext.WindowFramebufferSize.X,
            _applicationContext.WindowFramebufferSize.Y);

        _logger.Debug("{Category}: Loaded {ProgramStateName}",
                "ProgramState",
                GetType().Name);

        return true;
    }

    public void Render(float deltaTime,
                       float elapsedSeconds)
    {
        //if (_applicationContext.HasWindowFramebufferSizeChanged)
        {
            _swapchainDescriptor = CreateSwapchainDescriptor(_applicationContext.WindowFramebufferSize.X, _applicationContext.WindowFramebufferSize.Y);
        }

        _graphicsContext.BeginRenderPass(_swapchainDescriptor);
        _uiRenderer.BeginLayout();
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

        _assetWindow.Draw();
        _sceneHierarchyWindow.Draw();
        _sceneViewWindow.Draw();
        _propertyWindow.Draw();

        _renderer.RenderUI();

        _uiRenderer.ShowDemoWindow();
        _uiRenderer.EndLayout();

        _graphicsContext.EndRenderPass();
    }

    public void Update(float deltaTime,
                       float elapsedSeconds)
    {
    }

    public void Unload()
    {
    }

    private SwapchainDescriptor CreateSwapchainDescriptor(int width, int height)
    {
        return _graphicsContext
            .GetSwapchainDescriptorBuilder()
            .EnableSrgb()
            .WithViewport(width, height)
            .Build("Swapchain");
    }
}
