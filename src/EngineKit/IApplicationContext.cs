namespace EngineKit;

public interface IApplicationContext
{
    OpenTK.Mathematics.Vector2i ScreenSize { get; set; }

    OpenTK.Mathematics.Vector2i WindowSize { get; set; }

    OpenTK.Mathematics.Vector2i FramebufferSize { get; set; }

    OpenTK.Mathematics.Vector2i ScaledFramebufferSize { get; set; }

    bool ShowResizeInLog { get; set; }
}