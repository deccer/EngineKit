namespace EngineKit.Graphics;

public record struct DepthStencilDescriptor(
    bool IsDepthTestEnabled = true,
    bool IsDepthWriteEnabled = true,
    CompareOperation DepthCompareOperation = CompareOperation.Less);