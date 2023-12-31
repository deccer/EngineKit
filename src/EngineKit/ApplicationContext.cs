using System.Linq;
using System.Runtime.InteropServices;
using EngineKit.Mathematics;

namespace EngineKit;

internal sealed class ApplicationContext : IApplicationContext
{
    public ApplicationContext()
    {
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
    
    public Int2 ScreenSize { get; set; }

    public Int2 WindowSize { get; set; }

    public Int2 FramebufferSize { get; set; }

    public Int2 ScaledFramebufferSize { get; set; }

    public bool ShowResizeInLog { get; set; }
    
    public bool IsLaunchedByNSightGraphicsOnLinux { get; }
    
    public bool IsLaunchedByRenderDoc { get; }
}