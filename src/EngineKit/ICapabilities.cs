namespace EngineKit;

public interface ICapabilities
{
    bool SupportsBindlessTextures { get; }

    bool SupportsSwapControl { get; }

    bool SupportsMeshShader { get; }

    bool SupportsNvx { get; }

    bool IsIntelRenderer { get; }

    bool IsNvidiaRenderer { get; }

    bool IsAmdRenderer { get; }

    bool IsMesaRenderer { get; }

    int MaxImageUnits { get; }

    int MaxShaderStorageBlocks { get; }

    int MaxUniformBlocks { get; }

    int MaxCombinedTextureImageUnits { get; }

    int TotalAvailableVideoMemoryInKebiBytes { get; }

    int TotalAvailableVideoMemoryInMebiBytes { get; }

    int GetCurrentAvailableGpuMemoryInMebiBytes();

    bool Load();
}
