namespace EngineKit;

public interface ICapabilities
{
    bool SupportsBindlessTextures { get; }
    
    bool SupportsSwapControl { get; }
    
    int MaxImageUnits { get; }

    int MaxShaderStorageBlocks { get; }

    int MaxUniformBlocks { get; }

    int MaxCombinedTextureImageUnits { get; }
    
    int TotalAvailableVideoMemoryInKebiBytes { get; }
    
    int TotalAvailableVideoMemoryInMebiBytes { get; }

    int GetCurrentAvailableGpuMemoryInMebiBytes();
    
    bool SupportsNvx { get; }

    bool Load();
}