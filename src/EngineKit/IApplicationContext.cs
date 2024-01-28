using EngineKit.Mathematics;

namespace EngineKit;

public interface IApplicationContext
{
    double DesiredFramerate { get; set; }

    bool IsFrameRateLimited { get; set; }

    Int2 ScreenSize { get; set; }

    Int2 WindowSize { get; set; }

    Int2 FramebufferSize { get; set; }

    Int2 ScaledFramebufferSize { get; set; }

    bool ShowResizeInLog { get; set; }

    bool IsLaunchedByNSightGraphicsOnLinux { get; }

    bool IsLaunchedByRenderDoc { get; }

    bool IsWindowMaximized { get; set; }

    bool IsEditorEnabled { get; set; }

    Int2 EditorFramebufferSize { get; set; }
}
