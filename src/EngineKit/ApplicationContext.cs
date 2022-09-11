using EngineKit.Mathematics;

namespace EngineKit;

internal sealed class ApplicationContext : IApplicationContext
{
    public Point ScreenSize { get; set; }

    public Point WindowSize { get; set; }

    public Point FramebufferSize { get; set; }

    public Point ScaledFramebufferSize { get; set; }
}