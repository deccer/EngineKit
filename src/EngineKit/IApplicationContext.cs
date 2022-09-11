using EngineKit.Mathematics;

namespace EngineKit;

public interface IApplicationContext
{
    Point ScreenSize { get; set; }

    Point WindowSize { get; set; }

    Point FramebufferSize { get; set; }

    Point ScaledFramebufferSize { get; set; }
}