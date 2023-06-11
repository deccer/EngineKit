using System;
using System.Runtime.InteropServices;
using EngineKit.Native.Glfw;
using EngineKit.Native.OpenGL;

namespace EngineKit;

internal sealed class Limits : ILimits
{
    public int MaxImageUnits { get; private set; }

    public int MaxShaderStorageBlocks { get; private set; }

    public int MaxUniformBlocks { get; private set; }

    public int MaxCombinedTextureImageUnits { get; private set; }
    
    public int TotalAvailableVideoMemory { get; private set; }
    
    public bool IsLaunchedByNSightGraphicsOnLinux { get; private set; }

    public bool Load()
    {
        MaxImageUnits = GL.GetInteger(0x8F38);
        MaxShaderStorageBlocks = GL.GetInteger(0x90DC);
        MaxUniformBlocks = GL.GetInteger(0x8A2E);
        MaxCombinedTextureImageUnits = GL.GetInteger(0x90DC);

        if (Glfw.IsExtensionSupported("GL_NVX_gpu_memory_info"))
        {
            TotalAvailableVideoMemory = GL.GetInteger((uint)GL.GpuMemoryInfo.TotalAvailableMemory);
        }
        else
        {
            TotalAvailableVideoMemory = 0;
        }

        IsLaunchedByNSightGraphicsOnLinux =
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NOMAD_OPENGL_DELIMITER")) &
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        
        return true;
    }
}