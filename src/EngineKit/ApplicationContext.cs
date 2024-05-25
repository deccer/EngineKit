using System.Linq;
using System.Runtime.InteropServices;
using EngineKit.Mathematics;
using Microsoft.Extensions.Options;

namespace EngineKit;

public class ApplicationContext : IApplicationContext
{
    private readonly WindowSettings _windowSettings;

    public ApplicationContext(IOptions<WindowSettings> windowSettings)
    {
        _windowSettings = windowSettings.Value;
        IsLaunchedByNSightGraphicsOnLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
                                            !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("NOMAD_OPENGL_DELIMITER"));

        var renderdocEnvironmentVariables = new[]
        {
            "RENDERDOC_CAPFILE",
            "RENDERDOC_CAPOPTS",
            "RENDERDOC_DEBUG_LOG_FILE",
            "RENDERDOC_ORIGLIBPATH",
            "RENDERDOC_ORIGPRELOAD"
        };
        IsLaunchedByRenderDoc = renderdocEnvironmentVariables
            .Any(renderdocEnvironmentVariable => System.Environment.GetEnvironmentVariable(renderdocEnvironmentVariable) != null);
    }

    public double DesiredFramerate { get; set; } = 120.0;

    public bool IsFrameRateLimited { get; set; } = true;

    public Int2 ScreenSize { get; set; }

    public Int2 WindowSize { get; set; }

    public Int2 WindowFramebufferSize { get; private set; }

    public Int2 WindowScaledFramebufferSize { get; private set; }

    public bool HasWindowFramebufferSizeChanged { get; set; }

    public Int2 SceneViewSize { get; private set; }

    public Int2 SceneViewScaledSize { get; private set; }

    public bool HasSceneViewSizeChanged { get; set; }

    public bool IsWindowMaximized { get; set; }

    public bool IsLaunchedByNSightGraphicsOnLinux { get; }

    public bool IsLaunchedByRenderDoc { get; }

    public void ResizeWindowFramebuffer(int width, int height)
    {
        WindowFramebufferSize = new Int2(width, height);
        WindowScaledFramebufferSize = new Int2(new Double2(
            width * _windowSettings.ResolutionScale,
            height * _windowSettings.ResolutionScale));
        HasWindowFramebufferSizeChanged = true;
    }

    public void ResizeSceneView(int width, int height)
    {
        SceneViewSize = new Int2(width, height);
        SceneViewScaledSize = new Int2(new Double2(
            width * _windowSettings.ResolutionScale,
            height * _windowSettings.ResolutionScale));
        HasSceneViewSizeChanged = true;
    }
}
