namespace EngineKit;

public interface ILimits
{
    int MaxImageUnits { get; }

    int MaxShaderStorageBlocks { get; }

    int MaxUniformBlocks { get; }

    int MaxCombinedTextureImageUnits { get; }
    
    int TotalAvailableVideoMemory { get; }
    
    bool Load();
}