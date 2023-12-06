using EngineKit.Mathematics;

namespace EngineKit;

public interface IApplicationContext
{
    Int2 ScreenSize { get; set; }

    Int2 WindowSize { get; set; }

    Int2 FramebufferSize { get; set; }

    Int2 ScaledFramebufferSize { get; set; }

    bool ShowResizeInLog { get; set; }
}