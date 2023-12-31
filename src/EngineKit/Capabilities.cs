using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;

namespace EngineKit;

internal sealed class Capabilities : ICapabilities
{
    public int MaxImageUnits { get; private set; }

    public int MaxShaderStorageBlocks { get; private set; }

    public int MaxUniformBlocks { get; private set; }

    public int MaxCombinedTextureImageUnits { get; private set; }
    
    public int TotalAvailableVideoMemoryInKebiBytes { get; private set; }
    
    public int TotalAvailableVideoMemoryInMebiBytes { get; private set; }
    
    public bool SupportsBindlessTextures { get; private set; }
    
    public bool SupportsSwapControl { get; private set; }
    
    public bool SupportsNvx { get; private set; }
    
    public bool Load()
    {
        SupportsBindlessTextures = Glfw.IsExtensionSupported("GL_NV_bindless_texture") ||
                                   Glfw.IsExtensionSupported("GL_ARB_bindless_texture");

        SupportsSwapControl = Glfw.IsExtensionSupported("WGL_EXT_swap_control_tear") ||
                              Glfw.IsExtensionSupported("GLX_EXT_swap_control_tear");

        SupportsNvx = Glfw.IsExtensionSupported("GL_NVX_gpu_memory_info");
        
        MaxImageUnits = GL.GetInteger(0x8F38);
        MaxShaderStorageBlocks = GL.GetInteger(0x90DC);
        MaxUniformBlocks = GL.GetInteger(0x8A2E);
        MaxCombinedTextureImageUnits = GL.GetInteger(0x90DC);

        TotalAvailableVideoMemoryInKebiBytes = SupportsNvx
            ? GL.GetInteger((uint)GL.GpuMemoryInfo.TotalAvailableMemory)
            : 0;
        TotalAvailableVideoMemoryInMebiBytes = TotalAvailableVideoMemoryInKebiBytes / 1024;
        
        return true;
    }

    public int GetCurrentAvailableGpuMemoryInMebiBytes()
    {
        return SupportsNvx
            ? GL.GetInteger((uint)GL.GpuMemoryInfo.CurrentAvailableVideoMemory) / 1024
            : 0;
    }
}