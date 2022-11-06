namespace EngineKit;

internal sealed class ApplicationContext : IApplicationContext
{
    public OpenTK.Mathematics.Vector2i ScreenSize { get; set; }

    public OpenTK.Mathematics.Vector2i WindowSize { get; set; }

    public OpenTK.Mathematics.Vector2i FramebufferSize { get; set; }

    public OpenTK.Mathematics.Vector2i ScaledFramebufferSize { get; set; }
}