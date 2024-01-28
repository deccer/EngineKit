using System.Numerics;
using System.Threading.Tasks;
using Complex.Windows;
using EngineKit;
using EngineKit.Graphics;
using EngineKit.Messages;
using EngineKit.UI;
using ImGuiNET;
using Serilog;

namespace Complex.States;

internal sealed class EditorProgramState : IProgramState
{
    private readonly ILogger _logger;
    private readonly IApplicationContext _applicationContext;
    private readonly ICapabilities _capabilities;
    private readonly IMetrics _metrics;
    private readonly IGraphicsContext _graphicsContext;
    private readonly IRenderer _renderer;
    private readonly IUIRenderer _uiRenderer;
    private readonly IMessageBus _messageBus;
    private readonly AssetWindow _assetWindow;
    private readonly SceneHierarchyWindow _sceneHierarchyWindow;
    private readonly SceneViewWindow _sceneViewWindow;
    private readonly PropertyWindow _propertyWindow;

    private SwapchainDescriptor _swapchainDescriptor;

    public EditorProgramState(
        ILogger logger,
        IApplicationContext applicationContext,
        ICapabilities capabilities,
        IMetrics metrics,
        IGraphicsContext graphicsContext,
        IRenderer renderer,
        IUIRenderer uiRenderer,
        IMessageBus messageBus,
        AssetWindow assetWindow,
        SceneHierarchyWindow sceneHierarchyWindow,
        SceneViewWindow sceneViewWindow,
        PropertyWindow propertyWindow)
    {
        _logger = logger;
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

        _messageBus.Subscribe<FramebufferResizedMessage>(OnFramebufferResized);
    }

    public void Activate()
    {
        _applicationContext.IsEditorEnabled = true;
    }

    public bool Load()
    {
        _swapchainDescriptor = CreateSwapchainDescriptor();

        _logger.Debug("{Category}: Loaded {ProgramStateName}", "ProgramState", GetType().Name);

        return true;
    }

    public void Render(float deltaTime, float elapsedSeconds)
    {
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
                if (ImGui.Button(_applicationContext.IsWindowMaximized ? MaterialDesignIcons.WindowRestore : MaterialDesignIcons.WindowMaximize))
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

    public void Update(float deltaTime, float elapsedSeconds)
    {

    }

    private Task OnFramebufferResized(FramebufferResizedMessage message)
    {
        _swapchainDescriptor = CreateSwapchainDescriptor();

        return Task.CompletedTask;
    }

    private SwapchainDescriptor CreateSwapchainDescriptor()
    {
        return _graphicsContext
            .GetSwapchainDescriptorBuilder()
            .EnableSrgb()
            .WithFramebufferSizeAsViewport()
            .Build("Swapchain");
    }
}
