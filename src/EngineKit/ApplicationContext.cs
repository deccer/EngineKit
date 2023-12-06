using EngineKit.Mathematics;

namespace EngineKit;

internal sealed class ApplicationContext : IApplicationContext
{
    public Int2 ScreenSize { get; set; }

    public Int2 WindowSize { get; set; }

    public Int2 FramebufferSize { get; set; }

    public Int2 ScaledFramebufferSize { get; set; }

    public bool ShowResizeInLog { get; set; }
}