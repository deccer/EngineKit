namespace EngineKit;

public interface ICapabilities
{
    bool IsLaunchedByNSightGraphicsOnLinux { get; }
    
    bool IsLaunchedByRenderDoc { get; }
    
    bool SupportsBindlessTextures { get; }
    
    bool SupportsSwapControl { get; }

    bool Load();
}