using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using EngineKit.Native.OpenGL;

namespace EngineKit;

internal sealed class Capabilities : ICapabilities
{
    private readonly IList<string> _extensions;

    public Capabilities()
    {
        _extensions = new List<string>(512);
    }
    
    public bool IsLaunchedByNSightGraphicsOnLinux { get; private set; }
    
    public bool IsLaunchedByRenderDoc { get; private set; }
    
    public bool SupportsBindlessTextures { get; private set; }
    
    public bool SupportsSwapControl { get; private set; }
    
    public bool Load()
    {
        IsLaunchedByNSightGraphicsOnLinux =
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NOMAD_OPENGL_DELIMITER")) &
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        var renderdocEnvironmentVariables = new[]
        {
            "RENDERDOC_CAPFILE",
            "RENDERDOC_CAPOPTS",
            "RENDERDOC_DEBUG_LOG_FILE",
            "RENDERDOC_ORIGLIBPATH",
            "RENDERDOC_ORIGPRELOAD"
        };
        IsLaunchedByRenderDoc = renderdocEnvironmentVariables
            .Any(renderdocEnvironmentVariable => Environment.GetEnvironmentVariable(renderdocEnvironmentVariable) != null);
        
        var extensionCount = GL.GetInteger((uint)GL.GetName.NumExtensions);
        for (var i = 0u; i < extensionCount; i++)
        {
            _extensions.Add(GL.GetString(GL.StringName.Extensions, i));
        }

        SupportsBindlessTextures = _extensions.Contains("GL_NV_bindless_texture") ||
                                       _extensions.Contains("GL_ARB_bindless_texture");

        SupportsSwapControl = _extensions.Contains("WGL_EXT_swap_control_tear") ||
                              _extensions.Contains("GLX_EXT_swap_control_tear");
        
        return true;
    }
}