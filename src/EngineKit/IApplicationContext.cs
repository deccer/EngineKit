using EngineKit.Mathematics;

namespace EngineKit;

public interface IApplicationContext
{
    double DesiredFramerate { get; set; }

    bool IsFrameRateLimited { get; set; }

    Int2 ScreenSize { get; set; }

    Int2 WindowSize { get; set; }

    Int2 WindowFramebufferSize { get; }

    Int2 WindowScaledFramebufferSize { get; }

    bool HasWindowFramebufferSizeChanged { get; set; }

    Int2 SceneViewSize { get; }

    Int2 SceneViewScaledSize { get; }

    bool HasSceneViewSizeChanged { get; set; }

    bool IsLaunchedByNSightGraphicsOnLinux { get; }

    bool IsLaunchedByRenderDoc { get; }

    bool IsWindowMaximized { get; set; }

    void ResizeWindowFramebuffer(int width, int height);

    void ResizeSceneView(int width, int height);
}
